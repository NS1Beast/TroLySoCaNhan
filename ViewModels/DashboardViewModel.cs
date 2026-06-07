using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Dashboard.
    /// Quản lý danh sách tài liệu, trạng thái loading, tiến trình OCR, tìm kiếm, phân trang.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        // ==========================================
        // DỮ LIỆU
        // ==========================================
        public User CurrentUser { get; } = new User
        {
            Id = "TL-7F2A-9C81",
            UserName = "nguyenvana",
            DisplayName = "Nguyễn Văn A",
            Email = "vana@example.com",
            AvatarUrl = "",
            Plan = "Miễn phí"
        };

        public ObservableCollection<TaiLieu> TaiLieus { get; } = new ObservableCollection<TaiLieu>();

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
        /// <summary>Tiến trình OCR (0-100). Bind vào ProgressBar Value.</summary>
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

        private TaiLieu? _selectedTaiLieu;
        public TaiLieu? SelectedTaiLieu
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
        public RelayCommand DeleteTaiLieuCommand { get; }
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
            DeleteTaiLieuCommand = new RelayCommand(t => { if (t is TaiLieu tl) TaiLieus.Remove(tl); });
            UploadTaiLieuCommand = new RelayCommand(async _ => await SimulateUploadAsync(), _ => !IsLoading);
        }

        // ==========================================
        // EVENTS — code-behind Dashboard sẽ subscribe để mở Window mới
        // ==========================================
        public event EventHandler? ProfileRequested;
        public event EventHandler? SettingsRequested;
        public event EventHandler? UpgradeRequested;

        // ==========================================
        // HANDLERS
        // ==========================================
        private async Task LoadDocumentsAsync()
        {
            IsLoading = true;
            try
            {
                // TODO: gọi API phân trang thật ở đây
                await Task.Delay(500);

                // Giả lập dữ liệu
                TaiLieus.Clear();
                var seed = (CurrentPage - 1) * PageSize + 1;
                for (int i = 0; i < 12; i++)
                {
                    TaiLieus.Add(new TaiLieu
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

        /// <summary>Mô phỏng OCR: bind TiLeHoanThanh + ProgressRing hiển thị.</summary>
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

        /// <summary>Mô phỏng upload file (kèm IsLoading).</summary>
        private async Task SimulateUploadAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(1200);
                TaiLieus.Insert(0, new TaiLieu
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
}
