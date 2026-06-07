using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    /// <summary>
    /// ViewModel cho dialog Nâng cấp tài khoản.
    /// Hỗ trợ chọn phương thức thanh toán (VNPay / Momo) và hiển thị QR giả lập.
    /// </summary>
    public class UpgradeViewModel : ViewModelBase
    {
        public class QuyenLoi
        {
            public string NoiDung { get; set; } = string.Empty;
        }

        public class PhuongThucThanhToan
        {
            public string Ten { get; set; } = string.Empty;
            public SymbolIconInfo Icon { get; set; } = new();
            public string Color { get; set; } = "#3b82f6";
        }

        public class SymbolIconInfo
        {
            public Wpf.Ui.Controls.SymbolRegular Symbol { get; set; }
        }

        public ObservableCollection<QuyenLoi> QuyenLois { get; }
        public ObservableCollection<PhuongThucThanhToan> PhuongThucs { get; }

        private PhuongThucThanhToan? _selectedPhuongThuc;
        public PhuongThucThanhToan? SelectedPhuongThuc
        {
            get => _selectedPhuongThuc;
            set
            {
                if (SetProperty(ref _selectedPhuongThuc, value))
                {
                    OnPropertyChanged(nameof(HasSelectedPhuongThuc));
                    ThanhToanCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public bool HasSelectedPhuongThuc => _selectedPhuongThuc != null;

        // Bước hiện tại: 1 = chọn phương thức, 2 = đang chờ QR, 3 = thành công
        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(ShowSelectPayment));
                    OnPropertyChanged(nameof(ShowWaitingPayment));
                    OnPropertyChanged(nameof(ShowSuccess));
                }
            }
        }
        public bool ShowSelectPayment => _currentStep == 1;
        public bool ShowWaitingPayment => _currentStep == 2;
        public bool ShowSuccess => _currentStep == 3;

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                    ThanhToanCommand.RaiseCanExecuteChanged();
            }
        }

        public string TenGoi { get; } = "Gói Pro";
        public string GiaTien { get; } = "199.000đ / tháng";

        public RelayCommand ThanhToanCommand { get; }
        public RelayCommand HuyCommand { get; }
        public RelayCommand DongCommand { get; }
        public RelayCommand ChonPhuongThucCommand { get; }

        public event EventHandler? CloseRequested;

        public UpgradeViewModel()
        {
            QuyenLois = new ObservableCollection<QuyenLoi>
            {
                new() { NoiDung = "Mở rộng 100GB lưu trữ R2" },
                new() { NoiDung = "Tăng 1000 lượt AI mỗi tháng" },
                new() { NoiDung = "Mở khóa quét OCR hàng loạt" },
                new() { NoiDung = "Hỗ trợ kỹ thuật ưu tiên 24/7" },
                new() { NoiDung = "Xóa watermark tài liệu xuất bản" },
                new() { NoiDung = "Chia sẻ file với dung lượng không giới hạn" }
            };

            PhuongThucs = new ObservableCollection<PhuongThucThanhToan>
            {
                new() { Ten = "VNPay", Color = "#0066CC", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Payment24 } },
                new() { Ten = "Momo", Color = "#A50064", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Wallet24 } },
                new() { Ten = "Chuyển khoản ngân hàng", Color = "#10b981", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.BuildingBank24 } }
            };

            ThanhToanCommand = new RelayCommand(async _ => await DoThanhToanAsync(),
                _ => !IsProcessing && SelectedPhuongThuc != null);
            HuyCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
            DongCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
            ChonPhuongThucCommand = new RelayCommand(p => SelectedPhuongThuc = p as PhuongThucThanhToan);
        }

        private async Task DoThanhToanAsync()
        {
            if (SelectedPhuongThuc is null) return;
            IsProcessing = true;
            CurrentStep = 2; // Hiện QR
            try
            {
                // TODO: gọi API tạo đơn hàng + nhận QR code (base64) từ server
                await Task.Delay(2500);
                CurrentStep = 3; // Thành công
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
