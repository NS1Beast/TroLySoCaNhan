using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TroLySoCaNhan.MVVM;
// Bỏ using TroLySoCaNhan.Models; đi để không bị nhầm lẫn với Database

namespace TroLySoCaNhan.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        // ==========================================
        // DỮ LIỆU
        // ==========================================
        // Sử dụng DTO thay vì Model Database
        public UserDto CurrentUser { get; } = new UserDto
        {
            Id = "TL-7F2A-9C81",
            UserName = "nguyenvana",
            DisplayName = "Nguyễn Văn A",
            Email = "vana@example.com",
            Plan = "Miễn phí"
        };

        // Danh sách hiển thị trên UI dùng class TaiLieuItem
        public ObservableCollection<TaiLieuItem> TaiLieus { get; } = new ObservableCollection<TaiLieuItem>();

        // ==========================================
        // TÌM KIẾM / LỌC / PHÂN TRANG
        // ==========================================
        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        private string _selectedCategory = "Tất cả";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private int _pageSize = 50;
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set => SetProperty(ref _totalItems, value);
        }

        // ==========================================
        // TRẠNG THÁI LOADING / TIẾN TRÌNH
        // ==========================================
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    LoadDocumentsCommand.RaiseCanExecuteChanged();
                    NextPageCommand.RaiseCanExecuteChanged();
                    PrevPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isProcessingOcr;
        public bool IsProcessingOcr
        {
            get => _isProcessingOcr;
            set
            {
                if (SetProperty(ref _isProcessingOcr, value))
                {
                    StartOcrCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private double _tiLeHoanThanh;
        public double TiLeHoanThanh
        {
            get => _tiLeHoanThanh;
            set => SetProperty(ref _tiLeHoanThanh, value);
        }

        private string _ocrStatusMessage = string.Empty;
        public string OcrStatusMessage
        {
            get => _ocrStatusMessage;
            set => SetProperty(ref _ocrStatusMessage, value);
        }

        private TaiLieuItem? _selectedTaiLieu;
        public TaiLieuItem? SelectedTaiLieu
        {
            get => _selectedTaiLieu;
            set => SetProperty(ref _selectedTaiLieu, value);
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public RelayCommand LoadDocumentsCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PrevPageCommand { get; }
        public RelayCommand StartOcrCommand { get; }
        public RelayCommand OpenProfileCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenUpgradeCommand { get; }
        public RelayCommand OpenGroupCommand { get; }
        public RelayCommand DeleteTaiLieuCommand { get; }
        public RelayCommand OpenStorageCommand { get; }
        public RelayCommand UploadTaiLieuCommand { get; }

        public DashboardViewModel()
        {
            LoadDocumentsCommand = new RelayCommand(async _ => await LoadDocumentsAsync(), _ => !IsLoading);
            SearchCommand = new RelayCommand(async _ => { CurrentPage = 1; await LoadDocumentsAsync(); });
            NextPageCommand = new RelayCommand(async _ => { if (CurrentPage < TotalPages) { CurrentPage++; await LoadDocumentsAsync(); } }, _ => !IsLoading && CurrentPage < TotalPages);
            PrevPageCommand = new RelayCommand(async _ => { if (CurrentPage > 1) { CurrentPage--; await LoadDocumentsAsync(); } }, _ => !IsLoading && CurrentPage > 1);
            StartOcrCommand = new RelayCommand(async _ => await SimulateOcrAsync(), _ => !IsProcessingOcr && SelectedTaiLieu != null);
            OpenProfileCommand = new RelayCommand(_ => ProfileRequested?.Invoke(this, EventArgs.Empty));
            OpenSettingsCommand = new RelayCommand(_ => SettingsRequested?.Invoke(this, EventArgs.Empty));
            OpenUpgradeCommand = new RelayCommand(_ => UpgradeRequested?.Invoke(this, EventArgs.Empty));
            DeleteTaiLieuCommand = new RelayCommand(t => { if (t is TaiLieuItem tl) TaiLieus.Remove(tl); });
            UploadTaiLieuCommand = new RelayCommand(async _ => await SimulateUploadAsync(), _ => !IsLoading);
            OpenGroupCommand = new RelayCommand(_ => GroupRequested?.Invoke(this, EventArgs.Empty));
            OpenStorageCommand = new RelayCommand(_ => StorageRequested?.Invoke(this, EventArgs.Empty));
        }

        // ==========================================
        // EVENTS
        // ==========================================
        public event EventHandler? ProfileRequested;
        public event EventHandler? SettingsRequested;
        public event EventHandler? UpgradeRequested;
        public event EventHandler? GroupRequested;
        public event EventHandler? StorageRequested;

        // ==========================================
        // HANDLERS
        // ==========================================
        private async Task LoadDocumentsAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(500);
                TaiLieus.Clear();
                var seed = (CurrentPage - 1) * PageSize + 1;
                for (int i = 0; i < 12; i++)
                {
                    TaiLieus.Add(new TaiLieuItem
                    {
                        Id = $"DOC-{seed + i:00000}",
                        TenFile = $"Báo cáo tài chính Q{(seed + i) % 4 + 1}.pdf",
                        DinhDang = "pdf",
                        DungLuong = 2_500_000L + i * 350_000L,
                        DanhMuc = i % 2 == 0 ? "Tài chính" : "Nhân sự",
                        TrangThai = i % 5 == 0 ? "Đang OCR..." : "Sẵn sàng",
                        NgayTao = DateTime.Now.AddDays(-i),
                        NguoiTao = CurrentUser.DisplayName
                    });
                }
                TotalItems = 1234;
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SimulateOcrAsync()
        {
            if (SelectedTaiLieu is null) return;
            IsProcessingOcr = true;
            TiLeHoanThanh = 0;
            try
            {
                for (int p = 0; p <= 100; p += 5)
                {
                    TiLeHoanThanh = p;
                    OcrStatusMessage = $"Đang OCR trang {p / 5}/20 — {SelectedTaiLieu.TenFile}";
                    await Task.Delay(80);
                }
                SelectedTaiLieu.TrangThai = "Đã OCR xong";
            }
            finally
            {
                IsProcessingOcr = false;
                OcrStatusMessage = string.Empty;
            }
        }

        private async Task SimulateUploadAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(1200);
                TaiLieus.Insert(0, new TaiLieuItem
                {
                    Id = $"DOC-{DateTime.Now:HHmmss}",
                    TenFile = "Tài liệu mới upload.pdf",
                    DinhDang = "pdf",
                    DungLuong = 1_200_000L,
                    DanhMuc = "Mới",
                    TrangThai = "Sẵn sàng",
                    NgayTao = DateTime.Now,
                    NguoiTao = CurrentUser.DisplayName
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // =======================================================
    // CÁC LỚP DATA TRANSFER OBJECT (DTO) PHỤC VỤ RIÊNG CHO UI
    // =======================================================

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
    }

    public class TaiLieuItem : ViewModelBase
    {
        public string Id { get; set; } = string.Empty;
        public string TenFile { get; set; } = string.Empty;
        public string DinhDang { get; set; } = string.Empty;
        public long DungLuong { get; set; }
        public string DanhMuc { get; set; } = string.Empty;

        private string _trangThai = string.Empty;
        public string TrangThai
        {
            get => _trangThai;
            set => SetProperty(ref _trangThai, value);
        }

        public DateTime NgayTao { get; set; }
        public string NguoiTao { get; set; } = string.Empty;

        // Tự động tính toán dung lượng cho UI hiển thị đẹp mắt
        public string DungLuongHienThi
        {
            get
            {
                if (DungLuong >= 1048576) return $"{(DungLuong / 1048576.0):0.##} MB";
                return $"{(DungLuong / 1024.0):0} KB";
            }
        }
    }
}