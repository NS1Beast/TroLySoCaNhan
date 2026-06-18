using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        public UserDto CurrentUser { get; }

        public ObservableCollection<GroupItem> Groups { get; } = new ObservableCollection<GroupItem>();
        public ObservableCollection<GroupMemberItem> Members { get; } = new ObservableCollection<GroupMemberItem>();
        public ObservableCollection<GroupDocItem> Documents { get; } = new ObservableCollection<GroupDocItem>();
        public ObservableCollection<GroupDocItem> SelectedDocsToSend { get; } = new ObservableCollection<GroupDocItem>();

        private GroupItem? _selectedGroup;
        public GroupItem? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    SelectedMember = null;
                    SelectedDocument = null;
                    Members.Clear();
                    Documents.Clear();

                    OnPropertyChanged(nameof(CanManageGroup));

                    if (_selectedGroup != null)
                    {
                        _ = LoadGroupDetailsAsync();
                    }
                }
            }
        }

        private GroupMemberItem? _selectedMember;
        public GroupMemberItem? SelectedMember
        {
            get => _selectedMember;
            set
            {
                if (SetProperty(ref _selectedMember, value))
                {
                    OnPropertyChanged(nameof(CanEditSelectedMember));
                    SaveMemberPermissionCommand?.RaiseCanExecuteChanged();
                    RemoveMemberCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private GroupDocItem? _selectedDocument;
        public GroupDocItem? SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (SetProperty(ref _selectedDocument, value))
                {
                    OnPropertyChanged(nameof(CanEditSelectedDocument));
                    SaveDocPermissionCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newGroupName = string.Empty;
        public string NewGroupName
        {
            get => _newGroupName;
            set { if (SetProperty(ref _newGroupName, value)) ConfirmCreateGroupCommand?.RaiseCanExecuteChanged(); }
        }

        private string _inviteUid = string.Empty;
        public string InviteUid
        {
            get => _inviteUid;
            set { if (SetProperty(ref _inviteUid, value)) InviteMemberCommand?.RaiseCanExecuteChanged(); }
        }

        private string _bulkShareUids = string.Empty;
        public string BulkShareUids { get => _bulkShareUids; set => SetProperty(ref _bulkShareUids, value); }

        private bool _isCreateGroupDialogOpen;
        public bool IsCreateGroupDialogOpen { get => _isCreateGroupDialogOpen; set => SetProperty(ref _isCreateGroupDialogOpen, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        // ==========================================
        // LOGIC BẢO MẬT GIAO DIỆN
        // ==========================================
        public bool CanManageGroup
        {
            get
            {
                if (SelectedGroup == null) return false;
                return SelectedGroup.VaiTroCuaToi == "Owner" || SelectedGroup.VaiTroCuaToi == "Manager";
            }
        }

        public bool CanEditSelectedMember
        {
            get
            {
                if (SelectedGroup == null || SelectedMember == null) return false;
                if (SelectedMember.DbId == CurrentUser.DbId) return false;

                bool isMeOwner = SelectedGroup.VaiTroCuaToi == "Owner";
                bool isMeManager = SelectedGroup.VaiTroCuaToi == "Manager";
                bool isTargetOwner = SelectedMember.VaiTroTrongNhom == "Owner";
                bool isTargetManager = SelectedMember.VaiTroTrongNhom == "Manager";

                if (isTargetOwner) return false;
                if (isMeOwner) return true;
                if (isMeManager && !isTargetManager && !isTargetOwner) return true;
                return false;
            }
        }

        public bool CanEditSelectedDocument
        {
            get
            {
                if (SelectedGroup == null || SelectedDocument == null) return false;
                if (SelectedGroup.VaiTroCuaToi == "Owner" || SelectedGroup.VaiTroCuaToi == "Manager") return true;
                if (SelectedDocument.NguoiUp == CurrentUser.DisplayName) return true;
                return false;
            }
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public RelayCommand SelectGroupCommand { get; }
        public RelayCommand OpenCreateGroupCommand { get; }
        public RelayCommand CloseCreateGroupCommand { get; }
        public RelayCommand ConfirmCreateGroupCommand { get; }
        public RelayCommand LeaveGroupCommand { get; }

        public RelayCommand InviteMemberCommand { get; }
        public RelayCommand SaveMemberPermissionCommand { get; }
        public RelayCommand RemoveMemberCommand { get; }

        public RelayCommand UploadGroupDocCommand { get; }
        public RelayCommand DownloadGroupDocCommand { get; }
        public RelayCommand DeleteGroupDocCommand { get; }
        public RelayCommand SaveDocPermissionCommand { get; }

        public RelayCommand AddFileToSendListCommand { get; }
        public RelayCommand ClearSendListCommand { get; }
        public RelayCommand BulkShareCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public GroupViewModel(UserDto user)
        {
            CurrentUser = user;

            SelectGroupCommand = new RelayCommand(async t => { if (t is GroupItem group) { SelectedGroup = group; } });

            OpenCreateGroupCommand = new RelayCommand(_ => { NewGroupName = ""; IsCreateGroupDialogOpen = true; });
            CloseCreateGroupCommand = new RelayCommand(_ => IsCreateGroupDialogOpen = false);
            ConfirmCreateGroupCommand = new RelayCommand(async _ => await CreateGroupAsync(), _ => !string.IsNullOrWhiteSpace(NewGroupName));
            LeaveGroupCommand = new RelayCommand(async _ => await LeaveGroupAsync());

            InviteMemberCommand = new RelayCommand(async _ => await InviteMemberAsync(), _ => CanManageGroup && !string.IsNullOrWhiteSpace(InviteUid));
            SaveMemberPermissionCommand = new RelayCommand(async _ => await SaveMemberPermissionAsync(), _ => CanEditSelectedMember);
            RemoveMemberCommand = new RelayCommand(async _ => await RemoveMemberAsync(), _ => CanEditSelectedMember);

            UploadGroupDocCommand = new RelayCommand(async _ => await UploadDocumentToGroupAsync());
            DownloadGroupDocCommand = new RelayCommand(async _ => await DownloadDocumentToLocalAsync());
            DeleteGroupDocCommand = new RelayCommand(async _ => await DeleteGroupDocumentAsync());
            SaveDocPermissionCommand = new RelayCommand(_ => MessageBox.Show("Đã lưu giới hạn! (Demo)"), _ => CanEditSelectedDocument);

            AddFileToSendListCommand = new RelayCommand(_ => { if (SelectedDocument != null && !SelectedDocsToSend.Contains(SelectedDocument)) SelectedDocsToSend.Add(SelectedDocument); });
            ClearSendListCommand = new RelayCommand(_ => SelectedDocsToSend.Clear());
            BulkShareCommand = new RelayCommand(async _ => await BulkShareAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadGroupsAsync());

            _ = LoadGroupsAsync();
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ NHÓM VÀ THÀNH VIÊN
        // ==========================================
        private async Task LoadGroupsAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var query = from tv in db.ThanhVienNhoms
                                join n in db.NhomLuuTrus on tv.MaNhom equals n.Id
                                where tv.MaNguoiDung == CurrentUser.DbId
                                select new GroupItem
                                {
                                    Id = n.Id,
                                    TenNhom = n.TenNhom,
                                    SoThanhVien = db.ThanhVienNhoms.Count(x => x.MaNhom == n.Id),
                                    VaiTroCuaToi = n.MaNguoiTao == CurrentUser.DbId ? "Owner" : (tv.QuyenHan >= 32 ? "Manager" : "Member")
                                };
                    var list = query.ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Groups.Clear();
                        foreach (var g in list) Groups.Add(g);
                        if (Groups.Any()) SelectedGroup = Groups[0];
                        else SelectedGroup = null;
                    });
                });
            }
            finally { IsLoading = false; }
        }

        private async Task CreateGroupAsync()
        {
            IsLoading = true;
            IsCreateGroupDialogOpen = false;
            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var newGroup = new NhomLuuTru { Id = Guid.NewGuid(), TenNhom = NewGroupName.Trim(), MaNguoiTao = CurrentUser.DbId, NgayTao = DateTime.Now };
                    db.NhomLuuTrus.Add(newGroup);
                    db.ThanhVienNhoms.Add(new ThanhVienNhom { MaNhom = newGroup.Id, MaNguoiDung = CurrentUser.DbId, QuyenHan = 63, NgayThamGia = DateTime.Now });
                    db.SaveChanges();
                });
                await LoadGroupsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tạo nhóm: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task LeaveGroupAsync()
        {
            if (SelectedGroup == null) return;
            if (MessageBox.Show($"Bạn có chắc chắn muốn rời nhóm '{SelectedGroup.TenNhom}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var tv = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);

                    var group = db.NhomLuuTrus.FirstOrDefault(n => n.Id == SelectedGroup.Id);
                    if (group != null && group.MaNguoiTao == CurrentUser.DbId)
                        throw new Exception("Chủ nhóm không thể tự rời nhóm. Vui lòng chuyển nhượng hoặc xóa nhóm.");

                    if (tv != null) { db.ThanhVienNhoms.Remove(tv); db.SaveChanges(); }
                });
                SelectedGroup = null;
                await LoadGroupsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi rời nhóm: " + ex.Message, "Hệ thống"); }
        }

        private async Task LoadGroupDetailsAsync()
        {
            if (SelectedGroup == null) return;
            IsLoading = true;
            try
            {
                Guid groupId = SelectedGroup.Id;
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    var memberQuery = from tv in db.ThanhVienNhoms
                                      join nd in db.NguoiDungs on tv.MaNguoiDung equals nd.Id
                                      where tv.MaNhom == groupId
                                      select new
                                      {
                                          nd.Id,
                                          nd.TenHienThi,
                                          nd.MaNgauNhien,
                                          tv.QuyenHan,
                                          tv.NgayThamGia,
                                          IsOwner = db.NhomLuuTrus.Any(n => n.Id == groupId && n.MaNguoiTao == nd.Id)
                                      };

                    var memberList = memberQuery.ToList().Select(m => {
                        var item = new GroupMemberItem
                        {
                            DbId = m.Id,
                            TenHienThi = m.TenHienThi + (m.Id == CurrentUser.DbId ? " (Bạn)" : ""),
                            MaNgauNhien = m.MaNgauNhien,
                            NgayThamGia = m.NgayThamGia ?? DateTime.Now,
                            VaiTroTrongNhom = m.IsOwner ? "Owner" : (m.QuyenHan >= 32 ? "Manager" : "Member")
                        };
                        item.SetPermissionsFromMask(m.QuyenHan);
                        return item;
                    }).ToList();

                    var docQuery = from t in db.TaiLieus
                                   join p in db.PhienBanTaiLieus on t.Id equals p.MaTaiLieu
                                   join nd in db.NguoiDungs on p.MaNguoiCapNhat equals nd.Id
                                   where t.MaNhomLuuTru == groupId && t.DaXoa == false
                                   orderby p.NgayCapNhat descending
                                   select new GroupDocItem
                                   {
                                       DbId = t.Id,
                                       TenFile = t.TenTaiLieu,
                                       DungLuong = p.KichThuoc,
                                       NguoiUp = nd.TenHienThi,
                                       NgayCapNhat = p.NgayCapNhat ?? DateTime.Now
                                   };
                    var docList = docQuery.ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Members.Clear(); foreach (var m in memberList) Members.Add(m);
                        Documents.Clear(); foreach (var d in docList) Documents.Add(d);
                    });
                });
            }
            finally { IsLoading = false; }
        }

        private async Task InviteMemberAsync()
        {
            if (SelectedGroup == null || !CanManageGroup) { MessageBox.Show("Bạn không có quyền mời!"); return; }
            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    bool isOwner = db.NhomLuuTrus.Any(n => n.Id == SelectedGroup.Id && n.MaNguoiTao == CurrentUser.DbId);

                    if (!isOwner && myRole != null && (myRole.QuyenHan & 32) != 32)
                        throw new Exception("Access Denied: Bạn không có quyền thêm thành viên.");

                    var targetUser = db.NguoiDungs.FirstOrDefault(u => u.MaNgauNhien == InviteUid.Trim() || u.Email == InviteUid.Trim());
                    if (targetUser == null) throw new Exception("Không tìm thấy người dùng!");
                    if (db.ThanhVienNhoms.Any(tv => tv.MaNhom == SelectedGroup.Id && tv.MaNguoiDung == targetUser.Id)) throw new Exception("Người này đã ở trong nhóm.");

                    db.ThanhVienNhoms.Add(new ThanhVienNhom { MaNhom = SelectedGroup.Id, MaNguoiDung = targetUser.Id, QuyenHan = 3, NgayThamGia = DateTime.Now });
                    db.SaveChanges();
                });
                MessageBox.Show("Thêm thành viên thành công!");
                InviteUid = "";
                await LoadGroupDetailsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Bảo mật"); }
        }

        private async Task SaveMemberPermissionAsync()
        {
            if (SelectedGroup == null || SelectedMember == null || !CanEditSelectedMember) return;
            try
            {
                int newMask = SelectedMember.GetMaskFromPermissions();
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var group = db.NhomLuuTrus.FirstOrDefault(n => n.Id == SelectedGroup.Id);
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    var targetRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == SelectedMember.DbId);

                    if (group == null || myRole == null || targetRole == null) throw new Exception("Dữ liệu không hợp lệ.");

                    bool isMeOwnerDB = group.MaNguoiTao == CurrentUser.DbId;
                    bool isMeManagerDB = (myRole.QuyenHan & 32) == 32;
                    bool isTargetOwnerDB = group.MaNguoiTao == SelectedMember.DbId;
                    bool isTargetManagerDB = (targetRole.QuyenHan & 32) == 32;

                    if (SelectedMember.DbId == CurrentUser.DbId) throw new Exception("Access Denied: Không thể tự sửa quyền của chính mình.");
                    if (isTargetOwnerDB) throw new Exception("Access Denied: Không thể sửa quyền của Chủ nhóm.");
                    if (!isMeOwnerDB && !isMeManagerDB) throw new Exception("Access Denied: Bạn không có quyền phân quyền.");
                    if (isMeManagerDB && !isMeOwnerDB && isTargetManagerDB) throw new Exception("Access Denied: Quản lý không được sửa quyền của Quản lý khác.");

                    targetRole.QuyenHan = newMask;
                    db.SaveChanges();
                });
                MessageBox.Show("Đã cập nhật phân quyền!");
                await LoadGroupDetailsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi bảo mật: " + ex.Message, "Hệ thống"); }
        }

        private async Task RemoveMemberAsync()
        {
            if (SelectedGroup == null || SelectedMember == null || !CanEditSelectedMember) return;
            if (MessageBox.Show($"Xóa {SelectedMember.TenHienThi} khỏi nhóm?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var group = db.NhomLuuTrus.FirstOrDefault(n => n.Id == SelectedGroup.Id);
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    var targetRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == SelectedMember.DbId);

                    if (group == null || myRole == null || targetRole == null) return;

                    bool isMeOwnerDB = group.MaNguoiTao == CurrentUser.DbId;
                    bool isMeManagerDB = (myRole.QuyenHan & 32) == 32;
                    bool isTargetOwnerDB = group.MaNguoiTao == SelectedMember.DbId;
                    bool isTargetManagerDB = (targetRole.QuyenHan & 32) == 32;

                    if (SelectedMember.DbId == CurrentUser.DbId) throw new Exception("Vui lòng dùng tính năng Rời nhóm thay vì xóa mình.");
                    if (isTargetOwnerDB) throw new Exception("Access Denied: Không thể khai trừ Chủ nhóm.");
                    if (!isMeOwnerDB && !isMeManagerDB) throw new Exception("Access Denied: Bạn không có quyền khai trừ thành viên.");
                    if (isMeManagerDB && !isMeOwnerDB && isTargetManagerDB) throw new Exception("Access Denied: Quản lý không được xóa Quản lý khác.");

                    db.ThanhVienNhoms.Remove(targetRole);
                    db.SaveChanges();
                });
                await LoadGroupDetailsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi bảo mật: " + ex.Message, "Hệ thống"); }
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ TÀI LIỆU NHÓM ĐÃ FIX 100%
        // ==========================================
        private async Task UploadDocumentToGroupAsync()
        {
            if (SelectedGroup == null) return;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Title = "Chọn tài liệu tải lên nhóm", Filter = "Tất cả các file (*.*)|*.*" };
            if (openFileDialog.ShowDialog() != true) return;

            string selectedFilePath = openFileDialog.FileName;
            string fileName = Path.GetFileName(selectedFilePath);
            string fileExtension = Path.GetExtension(selectedFilePath).Replace(".", "");
            long fileSize = new FileInfo(selectedFilePath).Length;

            IsLoading = true;
            try
            {
                await Task.Run(async () =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // KIỂM TRA BẢO MẬT: Có quyền Thêm (Bitmask 2) không?
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    bool isOwner = db.NhomLuuTrus.Any(n => n.Id == SelectedGroup.Id && n.MaNguoiTao == CurrentUser.DbId);
                    if (!isOwner && myRole != null && (myRole.QuyenHan & 2) != 2 && (myRole.QuyenHan & 32) != 32)
                        throw new Exception("Access Denied: Bạn không được cấp quyền tải lên nhóm này.");

                    var dbUser = db.NguoiDungs.FirstOrDefault(u => u.Id == CurrentUser.DbId);
                    if (dbUser == null || string.IsNullOrEmpty(dbUser.KhoaCongKhaiPgp))
                        throw new Exception("Tài khoản chưa có khóa E2EE.");

                    // TẠO KHÓA MÃ HÓA RIÊNG CHO FILE VÀ LƯU VAULT TRƯỚC
                    byte[] aesKey = TroLySoCaNhan.Services.CryptoService.GenerateAesKey();
                    string fileId = Guid.NewGuid().ToString();
                    string encryptedFilePath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), $"{fileId}.enc");

                    await TroLySoCaNhan.Services.CryptoService.EncryptFileAsync(selectedFilePath, encryptedFilePath, aesKey);

                    string objectKeyR2 = "docs/" + fileId + ".enc";

                    // UPLOAD CLOUDFLARE R2
                    await TroLySoCaNhan.Services.CloudflareR2Service.UploadFileAsync(encryptedFilePath, objectKeyR2);

                    // LƯU DATA FILE
                    var taiLieu = new TaiLieu { Id = Guid.NewGuid(), TenTaiLieu = fileName, MaChuSoHuu = dbUser.Id, MaNhomLuuTru = SelectedGroup.Id, DaXoa = false, NgayTao = DateTime.Now };
                    db.TaiLieus.Add(taiLieu);

                    db.PhienBanTaiLieus.Add(new PhienBanTaiLieu { Id = Guid.NewGuid(), MaTaiLieu = taiLieu.Id, PhienBan = 1, DinhDang = fileExtension, KichThuoc = fileSize, HashFile = "GROUP-E2EE", ObjectKeyR2 = objectKeyR2, DaMaHoa = true, TrangThaiUpload = 1, MaNguoiCapNhat = dbUser.Id, NgayCapNhat = DateTime.Now });

                    // BƯỚC PHÂN KHÓA ĐẾN TỪNG THÀNH VIÊN TRONG NHÓM ĐỂ HỌ ĐỌC ĐƯỢC MÀ KHÔNG CẦN CHỦ NHÓM
                    var groupMembers = db.ThanhVienNhoms.Where(tv => tv.MaNhom == SelectedGroup.Id).Select(tv => tv.MaNguoiDung).ToList();
                    var usersInGroup = db.NguoiDungs.Where(u => groupMembers.Contains(u.Id) || u.Id == CurrentUser.DbId).ToList();

                    foreach (var member in usersInGroup)
                    {
                        if (string.IsNullOrEmpty(member.KhoaCongKhaiPgp)) continue;

                        string memberEncKey = TroLySoCaNhan.Services.CryptoService.EncryptAesKey(aesKey, member.KhoaCongKhaiPgp);
                        db.ChiaSeTaiLieuCaNhans.Add(new ChiaSeTaiLieuCaNhan
                        {
                            Id = Guid.NewGuid(),
                            MaTaiLieu = taiLieu.Id,
                            MaNguoiNhan = member.Id,
                            Quyen = (member.Id == dbUser.Id) ? (byte)2 : (byte)1,
                            FileKeyDaMaHoa = memberEncKey,
                            NgayChiaSe = DateTime.Now
                        });
                    }
                    db.SaveChanges();
                });
                await LoadGroupDetailsAsync();
                MessageBox.Show("Tải lên và đồng bộ E2EE Nhóm thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi up file: " + ex.Message, "Lỗi"); }
            finally { IsLoading = false; }
        }

        private async Task DownloadDocumentToLocalAsync()
        {
            if (SelectedDocument == null || SelectedGroup == null) return;
            var saveDialog = new Microsoft.Win32.SaveFileDialog { Title = "Tải về máy cá nhân", FileName = SelectedDocument.TenFile };
            if (saveDialog.ShowDialog() != true) return;

            IsLoading = true;
            try
            {
                string objectKey = "", encryptedFileKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // DB Check: Quyền Xem tài liệu (Bitmask 1)
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    bool isOwner = db.NhomLuuTrus.Any(n => n.Id == SelectedGroup.Id && n.MaNguoiTao == CurrentUser.DbId);
                    if (!isOwner && myRole != null && (myRole.QuyenHan & 1) != 1 && (myRole.QuyenHan & 32) != 32)
                        throw new Exception("Access Denied: Bạn không có quyền Xem/Tải tài liệu này.");

                    var pb = db.PhienBanTaiLieus.OrderByDescending(p => p.NgayCapNhat).FirstOrDefault(p => p.MaTaiLieu == SelectedDocument.DbId);
                    if (pb != null) objectKey = pb.ObjectKeyR2;

                    // Lấy khóa giải mã riêng của tài khoản đang đăng nhập
                    var cs = db.ChiaSeTaiLieuCaNhans.FirstOrDefault(c => c.MaTaiLieu == SelectedDocument.DbId && c.MaNguoiNhan == CurrentUser.DbId);
                    if (cs == null) throw new Exception("Tài khoản của bạn chưa được cấp khóa mã hóa cho file này.");
                    encryptedFileKey = cs.FileKeyDaMaHoa;
                });

                if (string.IsNullOrEmpty(objectKey)) throw new Exception("Không tìm thấy đường dẫn Cloud.");

                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKey));

                // Kéo từ Cloud R2 về LocalVault ẩn nếu chưa có
                if (!File.Exists(localEncPath))
                {
                    await TroLySoCaNhan.Services.CloudflareR2Service.DownloadFileAsync(objectKey, localEncPath);
                }

                // Dùng khóa Private Key giải mã ngược ra bản PDF/Word gốc cho người dùng
                byte[] aesKey = TroLySoCaNhan.Services.CryptoService.DecryptAesKey(encryptedFileKey!, TroLySoCaNhan.Services.CryptoService.UnprotectPrivateKey(CurrentUser.Id));
                await TroLySoCaNhan.Services.CryptoService.DecryptFileAsync(localEncPath, saveDialog.FileName, aesKey);

                MessageBox.Show("Đã giải mã và Tải xuống thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải xuống: " + ex.Message, "Bảo mật"); }
            finally { IsLoading = false; }
        }

        private async Task DeleteGroupDocumentAsync()
        {
            if (SelectedDocument == null || SelectedGroup == null) return;
            if (MessageBox.Show($"Xóa file '{SelectedDocument.TenFile}' khỏi nhóm?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                await Task.Run(async () =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // DB Check: Quyền Xóa (Bitmask 8) hoặc là người đã Upload lên
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    bool isOwner = db.NhomLuuTrus.Any(n => n.Id == SelectedGroup.Id && n.MaNguoiTao == CurrentUser.DbId);
                    var nd = db.NguoiDungs.FirstOrDefault(u => u.Id == CurrentUser.DbId);
                    bool isUploader = SelectedDocument.NguoiUp == nd?.TenHienThi;

                    if (!isOwner && !isUploader && myRole != null && (myRole.QuyenHan & 8) != 8 && (myRole.QuyenHan & 32) != 32)
                        throw new Exception("Access Denied: Bạn không có quyền xóa tài liệu của nhóm.");

                    var tl = db.TaiLieus.FirstOrDefault(t => t.Id == SelectedDocument.DbId);
                    var pb = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == SelectedDocument.DbId);

                    if (tl != null)
                    {
                        if (pb != null) await TroLySoCaNhan.Services.CloudflareR2Service.DeleteFileAsync(pb.ObjectKeyR2);

                        db.Database.ExecuteSqlRaw("DELETE FROM PhanLoaiTaiLieu WHERE MaTaiLieu = {0}", SelectedDocument.DbId);
                        db.ChiaSeTaiLieuCaNhans.RemoveRange(db.ChiaSeTaiLieuCaNhans.Where(c => c.MaTaiLieu == SelectedDocument.DbId));
                        db.PhienBanTaiLieus.RemoveRange(db.PhienBanTaiLieus.Where(p => p.MaTaiLieu == SelectedDocument.DbId));
                        db.NhatKyTaiLieus.RemoveRange(db.NhatKyTaiLieus.Where(n => n.MaTaiLieu == SelectedDocument.DbId));
                        db.TacVuNenChiTiets.RemoveRange(db.TacVuNenChiTiets.Where(a => a.MaTaiLieuGoc == SelectedDocument.DbId));

                        db.TaiLieus.Remove(tl);
                        db.SaveChanges();
                    }
                });
                await LoadGroupDetailsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa file: " + ex.Message, "Bảo mật"); }
            finally { IsLoading = false; }
        }

        private async Task BulkShareAsync()
        {
            if (SelectedDocsToSend.Count == 0 || string.IsNullOrWhiteSpace(BulkShareUids) || SelectedGroup == null) return;
            IsLoading = true;
            try
            {
                string[] uids = BulkShareUids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim()).ToArray();
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // DB Check: Quyền Chia sẻ (Bitmask 16)
                    var myRole = db.ThanhVienNhoms.FirstOrDefault(x => x.MaNhom == SelectedGroup.Id && x.MaNguoiDung == CurrentUser.DbId);
                    bool isOwner = db.NhomLuuTrus.Any(n => n.Id == SelectedGroup.Id && n.MaNguoiTao == CurrentUser.DbId);
                    if (!isOwner && myRole != null && (myRole.QuyenHan & 16) != 16 && (myRole.QuyenHan & 32) != 32)
                        throw new Exception("Access Denied: Bạn không có quyền chia sẻ tài liệu nhóm ra ngoài.");

                    string myPrivateKey = TroLySoCaNhan.Services.CryptoService.UnprotectPrivateKey(CurrentUser.Id);

                    foreach (var uid in uids)
                    {
                        var targetUser = db.NguoiDungs.FirstOrDefault(u => u.MaNgauNhien == uid || u.Email == uid);
                        if (targetUser == null || string.IsNullOrEmpty(targetUser.KhoaCongKhaiPgp)) continue;

                        foreach (var doc in SelectedDocsToSend)
                        {
                            if (!db.ChiaSeTaiLieuCaNhans.Any(c => c.MaTaiLieu == doc.DbId && c.MaNguoiNhan == targetUser.Id))
                            {
                                // Giải mã khóa từ bản sao của mình
                                var myShare = db.ChiaSeTaiLieuCaNhans.FirstOrDefault(c => c.MaTaiLieu == doc.DbId && c.MaNguoiNhan == CurrentUser.DbId);
                                if (myShare == null) continue;

                                byte[] rawAesKey = TroLySoCaNhan.Services.CryptoService.DecryptAesKey(myShare.FileKeyDaMaHoa!, myPrivateKey);

                                // Mã hóa lại Khóa AES bằng Public Key của người nhận
                                string targetEncKey = TroLySoCaNhan.Services.CryptoService.EncryptAesKey(rawAesKey, targetUser.KhoaCongKhaiPgp);

                                db.ChiaSeTaiLieuCaNhans.Add(new ChiaSeTaiLieuCaNhan { Id = Guid.NewGuid(), MaTaiLieu = doc.DbId, MaNguoiNhan = targetUser.Id, Quyen = 1, FileKeyDaMaHoa = targetEncKey, NgayChiaSe = DateTime.Now });
                            }
                        }
                    }
                    db.SaveChanges();
                });
                MessageBox.Show($"Đã phân phát khóa giải mã cho {SelectedDocsToSend.Count} tài liệu thành công!");
                SelectedDocsToSend.Clear();
                BulkShareUids = "";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi hàng loạt: " + ex.Message, "Bảo mật"); }
            finally { IsLoading = false; }
        }
    }

    public class GroupItem : ViewModelBase
    {
        public Guid Id { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public int SoThanhVien { get; set; }
        public string VaiTroCuaToi { get; set; } = string.Empty;
    }

    public class GroupMemberItem : ViewModelBase
    {
        public Guid DbId { get; set; }
        public string TenHienThi { get; set; } = string.Empty;
        public string MaNgauNhien { get; set; } = string.Empty;
        public string VaiTroTrongNhom { get; set; } = string.Empty;

        private string _quyenHienThi = string.Empty;
        public string QuyenHienThi { get => _quyenHienThi; set => SetProperty(ref _quyenHienThi, value); }

        public DateTime NgayThamGia { get; set; }

        private bool _canView; public bool CanView { get => _canView; set => SetProperty(ref _canView, value); }
        private bool _canAdd; public bool CanAdd { get => _canAdd; set => SetProperty(ref _canAdd, value); }
        private bool _canEdit; public bool CanEdit { get => _canEdit; set => SetProperty(ref _canEdit, value); }
        private bool _canDelete; public bool CanDelete { get => _canDelete; set => SetProperty(ref _canDelete, value); }
        private bool _canShare; public bool CanShare { get => _canShare; set => SetProperty(ref _canShare, value); }
        private bool _canManage; public bool CanManage { get => _canManage; set => SetProperty(ref _canManage, value); }

        public void SetPermissionsFromMask(int mask)
        {
            CanView = (mask & 1) == 1;     // Bit 1: Xem
            CanAdd = (mask & 2) == 2;      // Bit 2: Thêm
            CanEdit = (mask & 4) == 4;     // Bit 3: Sửa
            CanDelete = (mask & 8) == 8;   // Bit 4: Xóa
            CanShare = (mask & 16) == 16;  // Bit 5: Chia sẻ
            CanManage = (mask & 32) == 32; // Bit 6: Quản lý

            System.Collections.Generic.List<string> q = new System.Collections.Generic.List<string>();
            if (mask >= 63) { QuyenHienThi = "Toàn quyền"; return; }
            if (CanView) q.Add("Xem");
            if (CanAdd) q.Add("Thêm");
            if (CanEdit) q.Add("Sửa");
            if (CanDelete) q.Add("Xóa");
            if (CanShare) q.Add("Share");
            if (CanManage) q.Add("Admin");
            QuyenHienThi = q.Count > 0 ? string.Join(", ", q) : "Bị khóa";
        }

        public int GetMaskFromPermissions()
        {
            int mask = 0;
            if (CanView) mask |= 1;
            if (CanAdd) mask |= 2;
            if (CanEdit) mask |= 4;
            if (CanDelete) mask |= 8;
            if (CanShare) mask |= 16;
            if (CanManage) mask |= 32;
            return mask;
        }
    }

    public class GroupDocItem : ViewModelBase
    {
        public Guid DbId { get; set; }
        public string TenFile { get; set; } = string.Empty;
        public long DungLuong { get; set; }
        public string DungLuongHienThi => DungLuong >= 1048576 ? $"{(DungLuong / 1048576.0):0.##} MB" : $"{(DungLuong / 1024.0):0} KB";
        public string NguoiUp { get; set; } = string.Empty;
        public DateTime NgayCapNhat { get; set; }

        private bool _isReadOnly = false; public bool IsReadOnly { get => _isReadOnly; set => SetProperty(ref _isReadOnly, value); }
        private bool _isManagerOnly = false; public bool IsManagerOnly { get => _isManagerOnly; set => SetProperty(ref _isManagerOnly, value); }
    }
}