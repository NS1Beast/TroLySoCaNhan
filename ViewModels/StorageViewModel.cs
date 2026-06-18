using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    public class StorageViewModel : ViewModelBase
    {
        public UserDto CurrentUser { get; }

        public ObservableCollection<StorageTaiLieuItem> TaiLieus { get; } = new ObservableCollection<StorageTaiLieuItem>();
        public ObservableCollection<StorageFolderItem> Folders { get; } = new ObservableCollection<StorageFolderItem>();

        private string _currentFolderName = "Trang chủ";
        public string CurrentFolderName { get => _currentFolderName; set => SetProperty(ref _currentFolderName, value); }

        private Guid? _currentCategoryId = null;

        private string _searchKeyword = string.Empty;
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }

        private int _totalItems;
        public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }

        // ==========================================
        // QUẢN LÝ DIALOG TẠO THƯ MỤC
        // ==========================================
        private bool _isCreateFolderDialogOpen;
        public bool IsCreateFolderDialogOpen { get => _isCreateFolderDialogOpen; set => SetProperty(ref _isCreateFolderDialogOpen, value); }

        private string _newFolderName = string.Empty;
        public string NewFolderName
        {
            get => _newFolderName;
            set { SetProperty(ref _newFolderName, value); ConfirmCreateFolderCommand?.RaiseCanExecuteChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    UploadTaiLieuCommand?.RaiseCanExecuteChanged();
                    SearchCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private StorageTaiLieuItem? _selectedTaiLieu;
        public StorageTaiLieuItem? SelectedTaiLieu
        {
            get => _selectedTaiLieu;
            set => SetProperty(ref _selectedTaiLieu, value);
        }

        private bool _isProcessingOcr;
        public bool IsProcessingOcr { get => _isProcessingOcr; set => SetProperty(ref _isProcessingOcr, value); }

        private double _tiLeHoanThanh;
        public double TiLeHoanThanh { get => _tiLeHoanThanh; set => SetProperty(ref _tiLeHoanThanh, value); }

        private string _ocrStatusMessage = string.Empty;
        public string OcrStatusMessage { get => _ocrStatusMessage; set => SetProperty(ref _ocrStatusMessage, value); }

        public List<string> SortOptions { get; } = new List<string>
        {
            "Ngày tạo (Mới nhất)", "Ngày tạo (Cũ nhất)", "A-Z (Tên file)", "Kích thước (Lớn - Nhỏ)", "Loại file (Định dạng)"
        };

        private string _selectedSortOption = "Ngày tạo (Mới nhất)";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set { if (SetProperty(ref _selectedSortOption, value)) _ = LoadDocumentsAsync(); }
        }

        // KHAI BÁO COMMAND
        public RelayCommand SearchCommand { get; }
        public RelayCommand GoToHomeCommand { get; }
        public RelayCommand OpenFolderCommand { get; }

        public RelayCommand OpenCreateFolderDialogCommand { get; }
        public RelayCommand CancelCreateFolderCommand { get; }
        public RelayCommand ConfirmCreateFolderCommand { get; }

        public RelayCommand UploadTaiLieuCommand { get; }
        public RelayCommand UploadCloudCommand { get; }
        public RelayCommand RemoveFromCloudCommand { get; }
        public RelayCommand RemoveLocalFileCommand { get; }
        public RelayCommand OpenTaiLieuCommand { get; }
        public RelayCommand DownloadTaiLieuCommand { get; }
        public RelayCommand DeleteTaiLieuCommand { get; }
        public RelayCommand OpenLocalFolderCommand { get; }
        public RelayCommand ChangeLocalFolderCommand { get; }

        public StorageViewModel(UserDto user)
        {
            CurrentUser = user;

            SearchCommand = new RelayCommand(async _ => await LoadDocumentsAsync(), _ => !IsLoading);

            GoToHomeCommand = new RelayCommand(async _ =>
            {
                _currentCategoryId = null;
                CurrentFolderName = "Trang chủ";
                await LoadDocumentsAsync();
            });

            OpenFolderCommand = new RelayCommand(async t =>
            {
                if (t is StorageFolderItem folder)
                {
                    _currentCategoryId = folder.DbId;
                    CurrentFolderName = folder.TenThuMuc;
                    await LoadDocumentsAsync();
                }
            });

            // LOGIC DIALOG TẠO THƯ MỤC
            OpenCreateFolderDialogCommand = new RelayCommand(_ => { NewFolderName = string.Empty; IsCreateFolderDialogOpen = true; });
            CancelCreateFolderCommand = new RelayCommand(_ => { IsCreateFolderDialogOpen = false; });
            ConfirmCreateFolderCommand = new RelayCommand(async _ => await ConfirmCreateFolderAsync(), _ => !string.IsNullOrWhiteSpace(NewFolderName));

            UploadTaiLieuCommand = new RelayCommand(async _ => await SaveToLocalVaultAsync(), _ => !IsLoading);
            UploadCloudCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await SyncSelectedFileToCloudAsync(item); });
            RemoveFromCloudCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await RemoveFromCloudAsync(item); });
            RemoveLocalFileCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await RemoveLocalFileAsync(item); });
            OpenTaiLieuCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await OpenTaiLieuAsync(item); });
            DownloadTaiLieuCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await DownloadTaiLieuAsync(item); });
            DeleteTaiLieuCommand = new RelayCommand(async t => { if (t is StorageTaiLieuItem item) await DeleteTaiLieuAsync(item); });

            OpenLocalFolderCommand = new RelayCommand(_ => { System.Diagnostics.Process.Start("explorer.exe", TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id)); });
            ChangeLocalFolderCommand = new RelayCommand(_ => {
                var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Chọn nơi lưu trữ Local" };
                if (dialog.ShowDialog() == true) { TroLySoCaNhan.Services.LocalVaultService.SetCustomVaultPath(dialog.FolderName); }
            });

            _ = LoadDocumentsAsync();
        }

        // ==========================================
        // LƯU THƯ MỤC MỚI VÀO DB
        // ==========================================
        private async Task ConfirmCreateFolderAsync()
        {
            IsLoading = true;
            IsCreateFolderDialogOpen = false; // Đóng hộp thoại
            try
            {
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var newFolder = new DanhMuc { Id = Guid.NewGuid(), MaChuSoHuu = CurrentUser.DbId, TenDanhMuc = NewFolderName.Trim() };
                    db.DanhMucs.Add(newFolder);
                    db.SaveChanges();
                });
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tạo thư mục: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task LoadDocumentsAsync()
        {
            IsLoading = true;
            try
            {
                string vaultPath = TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id);
                string keyword = SearchKeyword?.Trim().ToLower() ?? "";

                var listFolders = new List<StorageFolderItem>();
                var displayList = new List<StorageTaiLieuItem>();

                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    if (_currentCategoryId == null && string.IsNullOrEmpty(keyword))
                    {
                        var dbFolders = db.DanhMucs.Where(dm => dm.MaChuSoHuu == CurrentUser.DbId).ToList();

                        db.Database.OpenConnection();
                        using var cmd = db.Database.GetDbConnection().CreateCommand();

                        foreach (var dm in dbFolders)
                        {
                            cmd.CommandText = $"SELECT COUNT(*) FROM PhanLoaiTaiLieu WHERE MaDanhMuc = '{dm.Id}'";
                            int count = Convert.ToInt32(cmd.ExecuteScalar());
                            listFolders.Add(new StorageFolderItem { DbId = dm.Id, TenThuMuc = dm.TenDanhMuc, SoLuongFile = count });
                        }
                        db.Database.CloseConnection();
                    }

                    IQueryable<TaiLieu> baseQuery = db.TaiLieus.Where(t => t.MaChuSoHuu == CurrentUser.DbId && t.DaXoa == false);

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        baseQuery = baseQuery.Where(x => x.TenTaiLieu != null && x.TenTaiLieu.ToLower().Contains(keyword));
                    }
                    else
                    {
                        if (_currentCategoryId == null)
                        {
                            baseQuery = db.TaiLieus.FromSqlRaw("SELECT * FROM TaiLieu WHERE MaChuSoHuu = {0} AND DaXoa = 0 AND Id NOT IN (SELECT MaTaiLieu FROM PhanLoaiTaiLieu)", CurrentUser.DbId);
                        }
                        else
                        {
                            baseQuery = db.TaiLieus.FromSqlRaw("SELECT t.* FROM TaiLieu t INNER JOIN PhanLoaiTaiLieu pl ON t.Id = pl.MaTaiLieu WHERE t.MaChuSoHuu = {0} AND t.DaXoa = 0 AND pl.MaDanhMuc = {1}", CurrentUser.DbId, _currentCategoryId.Value);
                        }
                    }

                    var rawList = baseQuery.Select(t => new
                    {
                        t.Id,
                        t.TenTaiLieu,
                        t.NgayTao,
                        PhienBan = db.PhienBanTaiLieus.OrderByDescending(p => p.NgayCapNhat).FirstOrDefault(p => p.MaTaiLieu == t.Id)
                    }).ToList();

                    IEnumerable<dynamic> sortedList = rawList;
                    switch (SelectedSortOption)
                    {
                        case "A-Z (Tên file)": sortedList = rawList.OrderBy(x => x.TenTaiLieu); break;
                        case "Kích thước (Lớn - Nhỏ)": sortedList = rawList.OrderByDescending(x => x.PhienBan?.KichThuoc ?? 0); break;
                        case "Loại file (Định dạng)": sortedList = rawList.OrderBy(x => x.PhienBan?.DinhDang ?? ""); break;
                        case "Ngày tạo (Cũ nhất)": sortedList = rawList.OrderBy(x => x.NgayTao); break;
                        default: sortedList = rawList.OrderByDescending(x => x.NgayTao); break;
                    }

                    displayList = sortedList.Select(x =>
                    {
                        bool isCloud = x.PhienBan?.TrangThaiUpload == 1;
                        bool isLocal = false;
                        long fileSize = x.PhienBan?.KichThuoc ?? 0;

                        if (x.PhienBan != null && !string.IsNullOrEmpty(x.PhienBan.ObjectKeyR2))
                            isLocal = File.Exists(Path.Combine(vaultPath, Path.GetFileName(x.PhienBan.ObjectKeyR2)));

                        string status = "Lỗi";
                        if (isCloud && isLocal) status = "Đồng bộ (Cloud + Local)";
                        else if (isCloud && !isLocal) status = "Chỉ trên Cloud";
                        else if (!isCloud && isLocal) status = "Chỉ lưu Local";

                        return new StorageTaiLieuItem
                        {
                            DbId = x.Id,
                            Id = x.Id.ToString().Substring(0, 8).ToUpper(),
                            TenFile = x.TenTaiLieu ?? "Không tên",
                            DungLuong = fileSize,
                            TrangThai = status,
                            IsOnCloud = isCloud,
                            IsOnLocal = isLocal,
                            NgayTao = x.NgayTao ?? DateTime.Now,
                            NguoiTao = CurrentUser.DisplayName
                        };
                    }).ToList();
                });

                App.Current.Dispatcher.Invoke(() => {
                    Folders.Clear();
                    foreach (var f in listFolders) Folders.Add(f);

                    TaiLieus.Clear();
                    foreach (var item in displayList) TaiLieus.Add(item);
                    TotalItems = displayList.Count;
                });
            }
            finally { IsLoading = false; }
        }

        private async Task SaveToLocalVaultAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Title = "Chọn tài liệu thêm vào máy", Filter = "Tất cả các file (*.*)|*.*" };
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
                    if (dbUser == null) throw new Exception("Tài khoản lỗi.");

                    byte[] aesKey = TroLySoCaNhan.Services.CryptoService.GenerateAesKey();
                    string fileId = Guid.NewGuid().ToString();
                    string encryptedFilePath = Path.Combine(TroLySoCaNhan.Services.LocalVaultService.GetVaultPath(CurrentUser.Id), $"{fileId}.enc");
                    await TroLySoCaNhan.Services.CryptoService.EncryptFileAsync(selectedFilePath, encryptedFilePath, aesKey);

                    var taiLieu = new TaiLieu { Id = Guid.NewGuid(), TenTaiLieu = fileName, MaChuSoHuu = dbUser.Id, DaXoa = false, NgayTao = DateTime.Now };
                    db.TaiLieus.Add(taiLieu);

                    db.PhienBanTaiLieus.Add(new PhienBanTaiLieu { Id = Guid.NewGuid(), MaTaiLieu = taiLieu.Id, PhienBan = 1, DinhDang = fileExtension, KichThuoc = fileSize, HashFile = "DUMMYHASH", ObjectKeyR2 = "docs/" + fileId + ".enc", DaMaHoa = true, TrangThaiUpload = 0, MaNguoiCapNhat = dbUser.Id, NgayCapNhat = DateTime.Now });

                    string myEncKey = TroLySoCaNhan.Services.CryptoService.EncryptAesKey(aesKey, dbUser.KhoaCongKhaiPgp!);
                    db.ChiaSeTaiLieuCaNhans.Add(new ChiaSeTaiLieuCaNhan { Id = Guid.NewGuid(), MaTaiLieu = taiLieu.Id, MaNguoiNhan = dbUser.Id, Quyen = 2, FileKeyDaMaHoa = myEncKey, NgayChiaSe = DateTime.Now });
                    db.SaveChanges();

                    if (_currentCategoryId != null)
                    {
                        db.Database.ExecuteSqlRaw("INSERT INTO PhanLoaiTaiLieu (MaTaiLieu, MaDanhMuc) VALUES ({0}, {1})", taiLieu.Id, _currentCategoryId.Value);
                    }
                });
                await LoadDocumentsAsync();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task SyncSelectedFileToCloudAsync(StorageTaiLieuItem item) { /* Code ẩn R2 Sync */ }
        private async Task RemoveFromCloudAsync(StorageTaiLieuItem item) { /* Code ẩn Xóa R2 */ }
        private async Task RemoveLocalFileAsync(StorageTaiLieuItem item) { /* Code ẩn Xóa Local */ }
        private async Task OpenTaiLieuAsync(StorageTaiLieuItem item) { /* Code ẩn Mở File */ }
        private async Task DownloadTaiLieuAsync(StorageTaiLieuItem item) { /* Code ẩn SaveFile */ }
        private async Task DeleteTaiLieuAsync(StorageTaiLieuItem item)
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
    }

    public class StorageFolderItem : ViewModelBase
    {
        public Guid DbId { get; set; }
        public string TenThuMuc { get; set; } = string.Empty;
        public int SoLuongFile { get; set; }
    }

    public class StorageTaiLieuItem : ViewModelBase
    {
        public Guid DbId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string TenFile { get; set; } = string.Empty;
        public long DungLuong { get; set; }
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