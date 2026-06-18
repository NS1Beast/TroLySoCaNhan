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

        // Bước hiện tại: 1 = Chọn, 2 = Quét QR, 3 = Thành công
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
            set { if (SetProperty(ref _isProcessing, value)) ThanhToanCommand.RaiseCanExecuteChanged(); }
        }

        public string TenGoi { get; } = "Gói Pro";
        public string GiaTien { get; } = "199.000đ / tháng";

        public RelayCommand ThanhToanCommand { get; }
        public RelayCommand HuyCommand { get; }
        public RelayCommand DongCommand { get; }

        public event EventHandler? CloseRequested;

        public UpgradeViewModel(UserDto currentUser)
        {
            _currentUser = currentUser;

            QuyenLois = new ObservableCollection<QuyenLoi>
            {
                new() { NoiDung = "Mở rộng 100GB lưu trữ R2 Cloud" },
                new() { NoiDung = "Tăng 1000 lượt sử dụng Trợ lý AI mỗi tháng" },
                new() { NoiDung = "Mở khóa chức năng quét OCR tự động hàng loạt" },
                new() { NoiDung = "Hỗ trợ kỹ thuật ưu tiên 24/7" },
                new() { NoiDung = "Chia sẻ tài liệu bảo mật E2EE không giới hạn" }
            };

            PhuongThucs = new ObservableCollection<PhuongThucThanhToan>
            {
                new() { Ten = "VNPay", Color = "#0066CC", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Payment24 } },
                new() { Ten = "Ví MoMo", Color = "#A50064", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.Wallet24 } },
                new() { Ten = "Chuyển khoản Ngân hàng", Color = "#10b981", Icon = new() { Symbol = Wpf.Ui.Controls.SymbolRegular.BuildingBank24 } }
            };

            ThanhToanCommand = new RelayCommand(async _ => await DoThanhToanAsync(), _ => !IsProcessing && SelectedPhuongThuc != null);
            HuyCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
            DongCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task DoThanhToanAsync()
        {
            if (SelectedPhuongThuc == null) return;
            IsProcessing = true;

            // Chuyển sang bước 2: Hiện mã QR
            CurrentStep = 2;
            try
            {
                // Giả lập thời gian user dùng điện thoại quét mã QR
                await Task.Delay(3000);

                // Ghi dữ liệu xuống DB bằng RAW SQL (Vì chưa có DbSet cho LichSuGiaoDich)
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // 1. Thêm Lịch sử giao dịch (Mã LoaiGiaoDich 2 = Mua gói, TrangThai 1 = Thành công)
                    Guid transactionId = Guid.NewGuid();
                    string sqlInsertTransaction = @"
                        INSERT INTO LichSuGiaoDich (ID, MaNguoiDung, LoaiGiaoDich, SoTien, PhuongThuc, TrangThai, NgayGiaoDich)
                        VALUES ({0}, {1}, 2, 199000, {2}, 1, {3})";
                    db.Database.ExecuteSqlRaw(sqlInsertTransaction, transactionId, _currentUser.DbId, SelectedPhuongThuc.Ten, DateTime.Now);

                    // 2. Nâng cấp User: Cộng 100GB (107374182400 Bytes) và 1000 lượt AI
                    string sqlUpdateUser = @"
                        UPDATE NguoiDung 
                        SET DungLuongToiDa = DungLuongToiDa + 107374182400,
                            LuotAISuDung = LuotAISuDung + 1000,
                            NgayCapNhat = {1}
                        WHERE ID = {0}";
                    db.Database.ExecuteSqlRaw(sqlUpdateUser, _currentUser.DbId, DateTime.Now);
                });

                // Cập nhật RAM (ViewModel)
                _currentUser.Plan = "Gói Pro";

                // Chuyển sang bước 3: Báo thành công
                CurrentStep = 3;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán DB: " + ex.Message, "Lỗi");
                CurrentStep = 1;
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}