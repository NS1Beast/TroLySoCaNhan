using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    public class UpgradeViewModel : ViewModelBase
    {
        private readonly UserDto _currentUser;

        public class QuyenLoi { public string NoiDung { get; set; } = string.Empty; }

        public class PhuongThucThanhToan
        {
            public string Ten { get; set; } = string.Empty;
            public SymbolIconInfo Icon { get; set; } = new();
            public string Color { get; set; } = "#3b82f6";
        }
        public class SymbolIconInfo { public Wpf.Ui.Controls.SymbolRegular Symbol { get; set; } }

        // Model để chứa thông tin gói cước
        public class PackageItem
        {
            public Guid Id { get; set; }
            public string TenGoi { get; set; } = string.Empty;
            public string GiaTienStr { get; set; } = string.Empty;
            public decimal GiaTien { get; set; }
            public long DungLuongTang { get; set; }
            public int LuotAITang { get; set; }
            public string MoTa { get; set; } = string.Empty;
            public string Color1 { get; set; } = string.Empty;
            public string Color2 { get; set; } = string.Empty;
            public string ShadowColor { get; set; } = string.Empty;
            public ObservableCollection<QuyenLoi> QuyenLois { get; set; } = new();
        }

        public ObservableCollection<PackageItem> AvailablePackages { get; }
        public ObservableCollection<PhuongThucThanhToan> PhuongThucs { get; }

        // Binding Gói cước đang chọn
        private PackageItem? _selectedPackage;
        public PackageItem? SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                if (SetProperty(ref _selectedPackage, value))
                    ThanhToanCommand?.RaiseCanExecuteChanged(); // Đã thêm dấu ?
            }
        }

        // Binding Phương thức đang chọn
        private PhuongThucThanhToan? _selectedPhuongThuc;
        public PhuongThucThanhToan? SelectedPhuongThuc
        {
            get => _selectedPhuongThuc;
            set
            {
                if (SetProperty(ref _selectedPhuongThuc, value))
                    ThanhToanCommand?.RaiseCanExecuteChanged(); // Đã thêm dấu ?
            }
        }

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
                    ThanhToanCommand?.RaiseCanExecuteChanged(); // Đã thêm dấu ? 
            }
        }

        public RelayCommand ThanhToanCommand { get; }
        public RelayCommand HuyCommand { get; }
        public RelayCommand DongCommand { get; }

        public event EventHandler? CloseRequested;

        public UpgradeViewModel(UserDto currentUser)
        {
            _currentUser = currentUser;

            // 💡 ĐÃ CHUYỂN KHỞI TẠO COMMAND LÊN ĐÂY (TRƯỚC KHI SET DỮ LIỆU)
            ThanhToanCommand = new RelayCommand(async _ => await DoThanhToanAsync(), _ => !IsProcessing && SelectedPhuongThuc != null && SelectedPackage != null);
            HuyCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
            DongCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));

            // Nạp dữ liệu 2 gói cước (Khớp với ID trong SQL)
            AvailablePackages = new ObservableCollection<PackageItem>
            {
                new PackageItem
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    TenGoi = "Gói VIP",
                    GiaTien = 99000,
                    GiaTienStr = "99.000đ / tháng",
                    DungLuongTang = 53687091200, // 50GB
                    LuotAITang = 500,
                    MoTa = "Dành cho cá nhân cần không gian làm việc vừa phải.",
                    Color1 = "#3B82F6", Color2 = "#8B5CF6", ShadowColor = "#60A5FA",
                    QuyenLois = new ObservableCollection<QuyenLoi> {
                        new() { NoiDung = "+ 50GB dung lượng R2 Cloud" },
                        new() { NoiDung = "+ 500 lượt sử dụng Trợ lý AI OCR" },
                        new() { NoiDung = "Hỗ trợ chia sẻ file mã hóa E2EE" }
                    }
                },
                new PackageItem
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    TenGoi = "Gói SVIP",
                    GiaTien = 199000,
                    GiaTienStr = "199.000đ / tháng",
                    DungLuongTang = 214748364800, // 200GB
                    LuotAITang = 2000,
                    MoTa = "Trải nghiệm sức mạnh tối đa cho nhóm và chuyên gia.",
                    Color1 = "#F59E0B", Color2 = "#EF4444", ShadowColor = "#FBBF24",
                    QuyenLois = new ObservableCollection<QuyenLoi> {
                        new() { NoiDung = "+ 200GB dung lượng R2 Cloud" },
                        new() { NoiDung = "+ 2000 lượt sử dụng Trợ lý AI OCR" },
                        new() { NoiDung = "Tự động hóa Batch Processing tối đa" },
                        new() { NoiDung = "Hỗ trợ kỹ thuật ưu tiên 24/7" }
                    }
                }
            };

            // Mặc định chọn gói VIP
            SelectedPackage = AvailablePackages[0];

            PhuongThucs = new ObservableCollection<PhuongThucThanhToan>
            {
                new() { Ten = "VNPay", Color = "#0066CC", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Payment24 } },
                new() { Ten = "Ví MoMo", Color = "#A50064", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Wallet24 } },
                new() { Ten = "Chuyển khoản Ngân hàng", Color = "#10b981", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.BuildingBank24 } }
            };
        }

        private async Task DoThanhToanAsync()
        {
            if (SelectedPhuongThuc == null || SelectedPackage == null) return;
            IsProcessing = true;

            CurrentStep = 2; // Hiện QR Code
            try
            {
                // Giả lập thời gian quét mã
                await Task.Delay(3500);

                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // 1. Thêm Lịch sử giao dịch (Sử dụng đúng ID gói dịch vụ)
                    Guid transactionId = Guid.NewGuid();
                    string sqlInsertTransaction = @"
                        INSERT INTO LichSuGiaoDich (ID, MaNguoiDung, LoaiGiaoDich, SoTien, MaGoiDichVu, PhuongThuc, TrangThai, NgayGiaoDich)
                        VALUES ({0}, {1}, 2, {2}, {3}, {4}, 1, {5})";
                    db.Database.ExecuteSqlRaw(sqlInsertTransaction,
                        transactionId, _currentUser.DbId, SelectedPackage.GiaTien,
                        SelectedPackage.Id, SelectedPhuongThuc.Ten, DateTime.Now);

                    // 2. Nâng cấp User: Cộng chính xác số GB và Luợt AI từ gói
                    string sqlUpdateUser = @"
                        UPDATE NguoiDung 
                        SET DungLuongToiDa = DungLuongToiDa + {1},
                            LuotAISuDung = LuotAISuDung + {2},
                            NgayCapNhat = {3}
                        WHERE ID = {0}";
                    db.Database.ExecuteSqlRaw(sqlUpdateUser,
                        _currentUser.DbId, SelectedPackage.DungLuongTang,
                        SelectedPackage.LuotAITang, DateTime.Now);
                });

                // Cập nhật RAM
                _currentUser.Plan = SelectedPackage.TenGoi;
                CurrentStep = 3; // Báo thành công
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống CSDL: " + ex.Message, "Lỗi Thanh Toán");
                CurrentStep = 1;
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}