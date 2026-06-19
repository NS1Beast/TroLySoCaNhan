using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;
using System.Windows;

namespace TroLySoCaNhan.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public UserDto CurrentUser { get; }
        public ObservableCollection<TaiLieuItem> TaiLieus { get; } = new ObservableCollection<TaiLieuItem>();

        private string _searchKeyword = string.Empty;
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }

        private int _currentPage = 1;
        public int CurrentPage { get => _currentPage; set => SetProperty(ref _currentPage, value); }
        private int _totalPages = 1;
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }
        private int _totalItems;
        public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }

        // 💡 THÊM THUỘC TÍNH HIỂN THỊ GÓI DỊCH VỤ
        private string _currentPlanDisplay = "Gói thường";
        public string CurrentPlanDisplay { get => _currentPlanDisplay; set => SetProperty(ref _currentPlanDisplay, value); }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    LoadDocumentsCommand?.RaiseCanExecuteChanged();
                    UploadTaiLieuCommand?.RaiseCanExecuteChanged();
                    UploadCloudCommand?.RaiseCanExecuteChanged();
                    RemoveFromCloudCommand?.RaiseCanExecuteChanged();
                    RemoveLocalFileCommand?.RaiseCanExecuteChanged();
                    DeleteTaiLieuCommand?.RaiseCanExecuteChanged();
                    DownloadTaiLieuCommand?.RaiseCanExecuteChanged();
                    ShareTaiLieuCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isProcessingOcr;
        public bool IsProcessingOcr { get => _isProcessingOcr; set { if (SetProperty(ref _isProcessingOcr, value)) StartOcrCommand?.RaiseCanExecuteChanged(); } }
        private double _tiLeHoanThanh;
        public double TiLeHoanThanh { get => _tiLeHoanThanh; set => SetProperty(ref _tiLeHoanThanh, value); }
        private string _ocrStatusMessage = string.Empty;
        public string OcrStatusMessage { get => _ocrStatusMessage; set => SetProperty(ref _ocrStatusMessage, value); }

        private TaiLieuItem? _selectedTaiLieu;
        public TaiLieuItem? SelectedTaiLieu
        {
            get => _selectedTaiLieu;
            set
            {
                if (SetProperty(ref _selectedTaiLieu, value))
                {
                    UploadCloudCommand?.RaiseCanExecuteChanged();
                    RemoveFromCloudCommand?.RaiseCanExecuteChanged();
                    RemoveLocalFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public System.Collections.Generic.List<string> SortOptions { get; } = new System.Collections.Generic.List<string>
        {
            "Ngày tạo (Mới nhất)",
            "Ngày tạo (Cũ nhất)",
            "A-Z (Tên file)",
            "Kích thước (Lớn - Nhỏ)",
            "Loại file (Định dạng)"
        };

        private string _selectedSortOption = "Ngày tạo (Mới nhất)";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                {
                    _ = LoadDocumentsAsync();
                }
            }
        }

        public RelayCommand LoadDocumentsCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PrevPageCommand { get; }
        public RelayCommand StartOcrCommand { get; }
        public RelayCommand UploadTaiLieuCommand { get; }
        public RelayCommand UploadCloudCommand { get; }
        public RelayCommand RemoveFromCloudCommand { get; }
        public RelayCommand RemoveLocalFileCommand { get; }
        public RelayCommand OpenTaiLieuCommand { get; }
        public RelayCommand ShareTaiLieuCommand { get; }
        public RelayCommand DownloadTaiLieuCommand { get; }
        public RelayCommand DeleteTaiLieuCommand { get; }
        public RelayCommand OpenLocalFolderCommand { get; }
        public RelayCommand ChangeLocalFolderCommand { get; }
        public RelayCommand OpenOcrCommand { get; }

        public RelayCommand OpenProfileCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenUpgradeCommand { get; }
        public RelayCommand OpenGroupCommand { get; }
        public RelayCommand OpenStorageCommand { get; }

        public event EventHandler? ProfileRequested;
        public event EventHandler? SettingsRequested;
        public event EventHandler? UpgradeRequested;
        public event EventHandler? GroupRequested;
        public event EventHandler? StorageRequested;
        public event EventHandler? OcrRequested;

        public DashboardViewModel(NguoiDung userDb)
        {
            CurrentUser = new UserDto
            {
                DbId = userDb.Id,
                Id = userDb.MaNgauNhien,
                ShareCode = userDb.MaNgauNhien,
                UserName = string.IsNullOrEmpty(userDb.TenDangNhap) ? userDb.Email : userDb.TenDangNhap,
                DisplayName = userDb.TenHienThi,
                Email = userDb.Email,
                Plan = "Gói thường"
            };

            LoadDocumentsCommand = new RelayCommand(async _ => await LoadDocumentsAsync(), _ => !IsLoading);
            SearchCommand = new RelayCommand(async _ => { CurrentPage = 1; await LoadDocumentsAsync(); });
            NextPageCommand = new RelayCommand(async _ => { if (CurrentPage < TotalPages) { CurrentPage++; await LoadDocumentsAsync(); } }, _ => !IsLoading && CurrentPage < TotalPages);
            PrevPageCommand = new RelayCommand(async _ => { if (CurrentPage > 1) { CurrentPage--; await LoadDocumentsAsync(); } }, _ => !IsLoading && CurrentPage > 1);

            UploadTaiLieuCommand = new RelayCommand(async _ => await SaveToLocalVaultAsync(), _ => !IsLoading);
            UploadCloudCommand = new RelayCommand(async t => { var item = t as TaiLieuItem ?? SelectedTaiLieu; if (item != null) await SyncSelectedFileToCloudAsync(item); }, _ => !IsLoading);

            RemoveFromCloudCommand = new RelayCommand(async t => { var item = t as TaiLieuItem ?? SelectedTaiLieu; if (item != null) await RemoveFromCloudAsync(item); }, _ => !IsLoading);
            RemoveLocalFileCommand = new RelayCommand(async t => { var item = t as TaiLieuItem ?? SelectedTaiLieu; if (item != null) await RemoveLocalFileAsync(item); }, _ => !IsLoading);
            OpenOcrCommand = new RelayCommand(_ => OcrRequested?.Invoke(this, EventArgs.Empty));
            OpenTaiLieuCommand = new RelayCommand(async t => { if (t is TaiLieuItem item) await OpenTaiLieuAsync(item); }, _ => !IsLoading);
            DownloadTaiLieuCommand = new RelayCommand(async t => { if (t is TaiLieuItem item) await DownloadTaiLieuAsync(item); }, _ => !IsLoading);
            DeleteTaiLieuCommand = new RelayCommand(async t => { if (t is TaiLieuItem item) await DeleteTaiLieuAsync(item); }, _ => !IsLoading);

            OpenLocalFolderCommand = new RelayCommand(_ =>
            {
                var path = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                System.Diagnostics.Process.Start("explorer.exe", path);
            });

            ChangeLocalFolderCommand = new RelayCommand(_ =>
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Chọn nơi lưu trữ Local" };
                if (dialog.ShowDialog() == true)
                {
                    string oldVaultPath = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                    string oldKeyPath = Path.Combine(oldVaultPath, "private.key.enc");

                    TroLySoCaNhan.Services.LocalVaultService.SetCustomVaultPath(dialog.FolderName);

                    string newVaultPath = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                    string newKeyPath = Path.Combine(newVaultPath, "private.key.enc");

                    try
                    {
                        if (File.Exists(oldKeyPath) && oldKeyPath != newKeyPath)
                        {
                            File.Copy(oldKeyPath, newKeyPath, overwrite: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể di dời Private Key: " + ex.Message, "Lỗi bảo mật");
                    }

                    MessageBox.Show($"Đã thay đổi nơi lưu trữ sang:\n{dialog.FolderName}\n\n(Chìa khóa bảo mật đã được tự động di dời sang thư mục mới).", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    _ = LoadDocumentsAsync();
                }
            });

            StartOcrCommand = new RelayCommand(async t => await RunOcrTaskAsync(t as TaiLieuItem), _ => !IsLoading);

            ShareTaiLieuCommand = new RelayCommand(async t =>
            {
                if (t is TaiLieuItem item)
                {
                    string testRecipientUid = "UID-TEST";
                    await ShareTaiLieuAsync(item, testRecipientUid, 1);
                }
            }, _ => !IsLoading);

            OpenProfileCommand = new RelayCommand(_ => ProfileRequested?.Invoke(this, EventArgs.Empty));
            OpenSettingsCommand = new RelayCommand(_ => SettingsRequested?.Invoke(this, EventArgs.Empty));

            // 💡 CẬP NHẬT LẠI LỆNH NÀY: MỞ CỬA HÀNG XONG LÀ TỰ ĐỘNG LOAD LẠI DATA
            OpenUpgradeCommand = new RelayCommand(_ =>
            {
                UpgradeRequested?.Invoke(this, EventArgs.Empty);
                _ = LoadDocumentsAsync();
            });

            OpenGroupCommand = new RelayCommand(_ => GroupRequested?.Invoke(this, EventArgs.Empty));
            OpenStorageCommand = new RelayCommand(_ => StorageRequested?.Invoke(this, EventArgs.Empty));

            _ = LoadDocumentsAsync();
        }

        private async Task LoadDocumentsAsync()
        {
            IsLoading = true;
            try
            {
                string vaultPath = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                string keyword = SearchKeyword?.Trim().ToLower() ?? "";
                string fetchedPlanName = "Gói thường";

                var listFiles = await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // 1. TỰ ĐỘNG KIỂM TRA GÓI CƯỚC TỪ LỊCH SỬ GIAO DỊCH
                    try
                    {
                        var latestPlan = (from ls in db.LichSuGiaoDiches
                                          join g in db.GoiDichVus on ls.MaGoiDichVu equals g.Id
                                          where ls.MaNguoiDung == CurrentUser.DbId && ls.LoaiGiaoDich == 2 && ls.TrangThai == 1
                                          orderby ls.NgayGiaoDich descending
                                          select g.TenGoi).FirstOrDefault();

                        if (!string.IsNullOrEmpty(latestPlan))
                        {
                            fetchedPlanName = latestPlan;
                        }
                    }
                    catch { /* Bỏ qua nếu chưa có bảng để ứng dụng không bị Crash */ }

                    // 2. QUERY DANH SÁCH FILE (Giữ nguyên như cũ)
                    var query = db.TaiLieus
                        .Where(t => t.MaChuSoHuu == CurrentUser.DbId && t.DaXoa == false)
                        .Select(t => new
                        {
                            t.Id,
                            t.TenTaiLieu,
                            t.NgayTao,
                            PhienBan = db.PhienBanTaiLieus.OrderByDescending(p => p.NgayCapNhat).FirstOrDefault(p => p.MaTaiLieu == t.Id)
                        }).ToList();

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(x =>
                            (x.TenTaiLieu != null && x.TenTaiLieu.ToLower().Contains(keyword)) ||
                            x.Id.ToString().ToLower().Contains(keyword)
                        ).ToList();
                    }

                    System.Collections.Generic.IEnumerable<dynamic> sortedList = query;
                    switch (SelectedSortOption)
                    {
                        case "A-Z (Tên file)":
                            sortedList = query.OrderBy(x => x.TenTaiLieu);
                            break;
                        case "Kích thước (Lớn - Nhỏ)":
                            sortedList = query.OrderByDescending(x => x.PhienBan?.KichThuoc ?? 0);
                            break;
                        case "Loại file (Định dạng)":
                            sortedList = query.OrderBy(x => x.PhienBan?.DinhDang ?? "");
                            break;
                        case "Ngày tạo (Cũ nhất)":
                            sortedList = query.OrderBy(x => x.NgayTao);
                            break;
                        case "Ngày tạo (Mới nhất)":
                        default:
                            sortedList = query.OrderByDescending(x => x.NgayTao);
                            break;
                    }

                    return sortedList.Select(x =>
                    {
                        bool isCloud = x.PhienBan?.TrangThaiUpload == 1;
                        bool isLocal = false;
                        long fileSize = x.PhienBan?.KichThuoc ?? 0;

                        if (x.PhienBan != null && !string.IsNullOrEmpty(x.PhienBan.ObjectKeyR2))
                        {
                            isLocal = File.Exists(Path.Combine(vaultPath, Path.GetFileName(x.PhienBan.ObjectKeyR2)));
                        }

                        string status = "Lỗi";
                        if (isCloud && isLocal) status = "Đồng bộ (Cloud + Local)";
                        else if (isCloud && !isLocal) status = "Chỉ trên Cloud (Free Space)";
                        else if (!isCloud && isLocal) status = "Chỉ lưu Local (Chờ Upload)";

                        return new TaiLieuItem
                        {
                            DbId = x.Id,
                            Id = x.Id.ToString().Substring(0, 8).ToUpper(),
                            TenFile = x.TenTaiLieu ?? "Không tên",
                            DungLuong = fileSize,
                            DanhMuc = "Gần đây",
                            TrangThai = status,
                            IsOnCloud = isCloud,
                            IsOnLocal = isLocal,
                            NgayTao = x.NgayTao ?? DateTime.Now,
                            NguoiTao = CurrentUser.DisplayName
                        };
                    }).ToList();
                });

                // Cập nhật lại UI Plan sau khi quét DB xong
                CurrentPlanDisplay = fetchedPlanName;

                TaiLieus.Clear();
                foreach (var item in listFiles) TaiLieus.Add(item);
                TotalItems = listFiles.Count;
            }
            finally { IsLoading = false; }
        }

        private async Task RemoveFromCloudAsync(TaiLieuItem item)
        {
            if (item == null || !item.IsOnCloud) return;

            var result = MessageBox.Show($"Bạn có muốn gỡ '{item.TenFile}' khỏi Cloud?\n(Tài liệu vẫn sẽ được giữ lại an toàn trên máy tính của bạn)", "Gỡ khỏi Cloud", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                string objectKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var phienBan = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    if (phienBan != null) objectKey = phienBan.ObjectKeyR2;
                });

                string vaultPath = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                string localEncPath = Path.Combine(vaultPath, Path.GetFileName(objectKey));

                if (!File.Exists(localEncPath))
                {
                    await TroLySoCaNhan.Services.CloudflareR2Service.DownloadFileAsync(objectKey, localEncPath);
                }

                await TroLySoCaNhan.Services.CloudflareR2Service.DeleteFileAsync(objectKey);

                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var phienBanDb = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    if (phienBanDb != null)
                    {
                        phienBanDb.TrangThaiUpload = 0;
                        db.SaveChanges();
                    }
                });

                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gỡ khỏi Cloud: " + ex.Message, "Lỗi"); }
            finally { IsLoading = false; }
        }

        private async Task RemoveLocalFileAsync(TaiLieuItem item)
        {
            if (item == null || !item.IsOnLocal) return;

            if (!item.IsOnCloud)
            {
                MessageBox.Show("Tài liệu này CHƯA được đồng bộ lên Cloud. Bạn không thể xóa bản Local để tránh mất dữ liệu vĩnh viễn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                string objectKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var phienBan = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    if (phienBan != null) objectKey = phienBan.ObjectKeyR2;
                });

                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKey));
                if (File.Exists(localEncPath)) File.Delete(localEncPath);

                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa bản Local: " + ex.Message, "Lỗi"); }
            finally { IsLoading = false; }
        }

        private async Task SaveToLocalVaultAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Title = "Chọn tài liệu thêm vào máy", Filter = "Tất cả các file (*.*)|*.*|Tài liệu PDF (*.pdf)|*.pdf|Word (*.docx)|*.docx" };
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
                    var dbUser = db.NguoiDungs.FirstOrDefault(u => u.Id == CurrentUser.DbId);
                    if (dbUser == null || string.IsNullOrEmpty(dbUser.KhoaCongKhaiPgp)) throw new Exception("Tài khoản lỗi hoặc chưa có khóa E2EE.");

                    byte[] aesKey = TroLySoCaNhan.Services.CryptoService.GenerateAesKey();
                    string fileId = Guid.NewGuid().ToString();
                    string encryptedFilePath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), $"{fileId}.enc");
                    await TroLySoCaNhan.Services.CryptoService.EncryptFileAsync(selectedFilePath, encryptedFilePath, aesKey);

                    string hashFile = "";
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    {
                        using var fs = File.OpenRead(encryptedFilePath);
                        hashFile = BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
                    }

                    var taiLieu = new TaiLieu { Id = Guid.NewGuid(), TenTaiLieu = fileName, MaChuSoHuu = dbUser.Id, DaXoa = false, NgayTao = DateTime.Now };
                    db.TaiLieus.Add(taiLieu);

                    db.PhienBanTaiLieus.Add(new PhienBanTaiLieu { Id = Guid.NewGuid(), MaTaiLieu = taiLieu.Id, PhienBan = 1, DinhDang = fileExtension, KichThuoc = fileSize, HashFile = hashFile, ObjectKeyR2 = "docs/" + fileId + ".enc", DaMaHoa = true, TrangThaiUpload = 0, MaNguoiCapNhat = dbUser.Id, NgayCapNhat = DateTime.Now });

                    string myEncKey = TroLySoCaNhan.Services.CryptoService.EncryptAesKey(aesKey, dbUser.KhoaCongKhaiPgp!);
                    db.ChiaSeTaiLieuCaNhans.Add(new ChiaSeTaiLieuCaNhan { Id = Guid.NewGuid(), MaTaiLieu = taiLieu.Id, MaNguoiNhan = dbUser.Id, Quyen = 2, FileKeyDaMaHoa = myEncKey, NgayChiaSe = DateTime.Now });
                    db.SaveChanges();
                });
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task SyncSelectedFileToCloudAsync(TaiLieuItem item)
        {
            if (item == null) return;
            IsLoading = true;
            try
            {
                string objectKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var phienBan = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    if (phienBan == null) throw new Exception("Không tìm thấy dữ liệu.");
                    objectKey = phienBan.ObjectKeyR2;
                });

                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKey));
                if (!File.Exists(localEncPath)) throw new Exception("Không tìm thấy tệp mã hóa trong máy!");

                await Task.Run(async () =>
                {
                    await TroLySoCaNhan.Services.CloudflareR2Service.UploadFileAsync(localEncPath, objectKey);
                    using var db = new TroLySoCaNhanContext();
                    var phienBanDb = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    if (phienBanDb != null) { phienBanDb.TrangThaiUpload = 1; db.SaveChanges(); }
                });
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Cloud Sync: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task OpenTaiLieuAsync(TaiLieuItem item)
        {
            IsLoading = true;
            try
            {
                string objectKey = "", encryptedFileKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var phienBan = db.PhienBanTaiLieus.OrderByDescending(p => p.NgayCapNhat).FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    var chiaSe = db.ChiaSeTaiLieuCaNhans.FirstOrDefault(c => c.MaTaiLieu == item.DbId && c.MaNguoiNhan == CurrentUser.DbId);
                    if (phienBan == null || chiaSe == null) throw new Exception("Không có quyền mở!");
                    objectKey = phienBan.ObjectKeyR2; encryptedFileKey = chiaSe.FileKeyDaMaHoa;
                });

                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKey));
                if (!File.Exists(localEncPath)) await TroLySoCaNhan.Services.CloudflareR2Service.DownloadFileAsync(objectKey, localEncPath);

                byte[] aesKey = TroLySoCaNhan.Services.CryptoService.DecryptAesKey(encryptedFileKey!, TroLySoCaNhan.Services.CryptoService.UnprotectPrivateKey(CurrentUser.Id));
                string tempFilePath = Path.Combine(Path.GetTempPath(), item.TenFile);
                await TroLySoCaNhan.Services.CryptoService.DecryptFileAsync(localEncPath, tempFilePath, aesKey);

                var process = new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(tempFilePath) { UseShellExecute = true }, EnableRaisingEvents = true };
                process.Exited += (s, e) => { try { File.Delete(tempFilePath); } catch { } };
                process.Start();
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mở file: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task DownloadTaiLieuAsync(TaiLieuItem item)
        {
            if (item == null) return;
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog { Title = "Tải tài liệu xuống", FileName = item.TenFile };
            if (saveFileDialog.ShowDialog() != true) return;

            IsLoading = true;
            try
            {
                string objectKey = "", encryptedFileKey = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var pb = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);
                    var cs = db.ChiaSeTaiLieuCaNhans.FirstOrDefault(c => c.MaTaiLieu == item.DbId && c.MaNguoiNhan == CurrentUser.DbId);
                    objectKey = pb!.ObjectKeyR2; encryptedFileKey = cs!.FileKeyDaMaHoa;
                });

                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKey));
                if (!File.Exists(localEncPath)) await TroLySoCaNhan.Services.CloudflareR2Service.DownloadFileAsync(objectKey, localEncPath);

                byte[] aesKey = TroLySoCaNhan.Services.CryptoService.DecryptAesKey(encryptedFileKey!, TroLySoCaNhan.Services.CryptoService.UnprotectPrivateKey(CurrentUser.Id));
                await TroLySoCaNhan.Services.CryptoService.DecryptFileAsync(localEncPath, saveFileDialog.FileName, aesKey);
                MessageBox.Show("Đã tải xuống thành công!");
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải xuống: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task DeleteTaiLieuAsync(TaiLieuItem item)
        {
            if (item == null) return;
            if (MessageBox.Show($"Xóa VĨNH VIỄN '{item.TenFile}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                string objectKeyR2 = "";
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var taiLieu = db.TaiLieus.FirstOrDefault(t => t.Id == item.DbId);
                    var phienBan = db.PhienBanTaiLieus.FirstOrDefault(p => p.MaTaiLieu == item.DbId);

                    if (taiLieu != null)
                    {
                        if (phienBan != null) objectKeyR2 = phienBan.ObjectKeyR2;

                        db.Database.ExecuteSqlRaw("DELETE FROM PhanLoaiTaiLieu WHERE MaTaiLieu = {0}", item.DbId);

                        db.ChiaSeTaiLieuCaNhans.RemoveRange(db.ChiaSeTaiLieuCaNhans.Where(c => c.MaTaiLieu == item.DbId));
                        db.PhienBanTaiLieus.RemoveRange(db.PhienBanTaiLieus.Where(p => p.MaTaiLieu == item.DbId));
                        db.NhatKyTaiLieus.RemoveRange(db.NhatKyTaiLieus.Where(n => n.MaTaiLieu == item.DbId));
                        db.TacVuNenChiTiets.RemoveRange(db.TacVuNenChiTiets.Where(a => a.MaTaiLieuGoc == item.DbId));

                        var t = db.TaiLieus.FirstOrDefault(x => x.Id == item.DbId);
                        if (t != null) db.TaiLieus.Remove(t);
                        db.SaveChanges();
                    }
                });

                if (!string.IsNullOrEmpty(objectKeyR2)) await TroLySoCaNhan.Services.CloudflareR2Service.DeleteFileAsync(objectKeyR2);
                string localEncPath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), Path.GetFileName(objectKeyR2));
                if (File.Exists(localEncPath)) File.Delete(localEncPath);

                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task ShareTaiLieuAsync(TaiLieuItem item, string recipientShareCode, byte quyen)
        {
            if (item == null) return;
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var recipient = db.NguoiDungs.FirstOrDefault(u => u.MaNgauNhien == recipientShareCode);
                    if (recipient == null) throw new Exception("Không tìm thấy người dùng!");
                    if (db.ChiaSeTaiLieuCaNhans.Any(c => c.MaTaiLieu == item.DbId && c.MaNguoiNhan == recipient.Id)) throw new Exception("Tài liệu đã được chia sẻ.");
                    var myShareRecord = db.ChiaSeTaiLieuCaNhans.FirstOrDefault(c => c.MaTaiLieu == item.DbId && c.MaNguoiNhan == CurrentUser.DbId);
                    if (myShareRecord == null) throw new Exception("Không có quyền sở hữu!");

                    string myPrivateKey = TroLySoCaNhan.Services.CryptoService.UnprotectPrivateKey(CurrentUser.Id);
                    byte[] rawAesKey = TroLySoCaNhan.Services.CryptoService.DecryptAesKey(myShareRecord.FileKeyDaMaHoa!, myPrivateKey);
                    string recipientEncryptedKey = TroLySoCaNhan.Services.CryptoService.EncryptAesKey(rawAesKey, recipient.KhoaCongKhaiPgp!);

                    db.ChiaSeTaiLieuCaNhans.Add(new ChiaSeTaiLieuCaNhan { Id = Guid.NewGuid(), MaTaiLieu = item.DbId, MaNguoiNhan = recipient.Id, Quyen = quyen, FileKeyDaMaHoa = recipientEncryptedKey, NgayChiaSe = DateTime.Now });
                    db.SaveChanges();
                });
                MessageBox.Show("Chia sẻ thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task RunOcrTaskAsync(TaiLieuItem? item) { await Task.Delay(10); }
    }

    public class TaiLieuItem : ViewModelBase
    {
        public Guid DbId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string TenFile { get; set; } = string.Empty;
        public long DungLuong { get; set; }
        public string DanhMuc { get; set; } = string.Empty;

        private string _trangThai = string.Empty;
        public string TrangThai { get => _trangThai; set => SetProperty(ref _trangThai, value); }

        private bool _isOnCloud;
        public bool IsOnCloud { get => _isOnCloud; set => SetProperty(ref _isOnCloud, value); }

        private bool _isOnLocal;
        public bool IsOnLocal { get => _isOnLocal; set => SetProperty(ref _isOnLocal, value); }

        public DateTime NgayTao { get; set; }
        public string NguoiTao { get; set; } = string.Empty;
        public string DungLuongHienThi => DungLuong >= 1048576 ? $"{(DungLuong / 1048576.0):0.##} MB" : $"{(DungLuong / 1024.0):0} KB";
    }
}