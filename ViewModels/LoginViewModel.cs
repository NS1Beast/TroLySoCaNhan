using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TroLySoCaNhan.Models;
using BCrypt.Net;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event EventHandler? LoginSucceeded;

        // --- CÁC BIẾN GIAO DIỆN ---
        private string _activePanel = "Login";
        public string ActivePanel { get => _activePanel; set { _activePanel = value; OnPropertyChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        // --- BIẾN ĐĂNG NHẬP ---
        private string _username = string.Empty;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private string _password = string.Empty;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private bool _rememberMe;
        public bool RememberMe { get => _rememberMe; set { _rememberMe = value; OnPropertyChanged(); } }

        private bool _isLoggingIn;
        public bool IsLoggingIn { get => _isLoggingIn; set { _isLoggingIn = value; OnPropertyChanged(); } }

        // --- BIẾN ĐĂNG KÝ (ĐÃ CẬP NHẬT THEO YÊU CẦU MỚI) ---
        private string _regUsername = string.Empty;
        public string RegUsername { get => _regUsername; set { _regUsername = value; OnPropertyChanged(); } }

        private string _regPassword = string.Empty;
        public string RegPassword { get => _regPassword; set { _regPassword = value; OnPropertyChanged(); } }

        private string _regConfirmPassword = string.Empty;
        public string RegConfirmPassword { get => _regConfirmPassword; set { _regConfirmPassword = value; OnPropertyChanged(); } }

        private bool _isRegistering;
        public bool IsRegistering { get => _isRegistering; set { _isRegistering = value; OnPropertyChanged(); } }

        // --- BIẾN QUÊN MẬT KHẨU ---
        private string _forgotEmail = string.Empty;
        public string ForgotEmail { get => _forgotEmail; set { _forgotEmail = value; OnPropertyChanged(); } }
        private bool _isSendingForgot;
        public bool IsSendingForgot { get => _isSendingForgot; set { _isSendingForgot = value; OnPropertyChanged(); } }

        // --- LỆNH NÚT BẤM ---
        public ICommand ShowLoginCommand { get; }
        public ICommand ShowRegisterCommand { get; }
        public ICommand ShowForgotCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public LoginViewModel()
        {
            ShowLoginCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Login"; });
            ShowRegisterCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Register"; });
            ShowForgotCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Forgot"; });

            LoginCommand = new RelayCommand(async _ => await DoLoginAsync());
            RegisterCommand = new RelayCommand(async _ => await DoRegisterAsync());
            ForgotPasswordCommand = new RelayCommand(async _ => await DoForgotAsync());
        }

        private async Task DoLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return;
            }

            ErrorMessage = "";
            IsLoggingIn = true;

            try
            {
                using var db = new TroLySoCaNhanContext();
                var user = db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == Username || u.Email == Username);

                await Task.Delay(800);

                if (user == null || string.IsNullOrEmpty(user.MatKhauHash))
                {
                    ErrorMessage = "Tài khoản không tồn tại hoặc đăng nhập qua nền tảng khác.";
                    return;
                }

                if (user.TrangThai == false)
                {
                    ErrorMessage = "Tài khoản của bạn đã bị khóa.";
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(Password, user.MatKhauHash))
                {
                    ErrorMessage = "Sai mật khẩu. Vui lòng thử lại.";
                    return;
                }

                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối máy chủ: " + ex.Message;
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private async Task DoRegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(RegUsername) || string.IsNullOrWhiteSpace(RegPassword))
            {
                ErrorMessage = "Vui lòng điền đủ Tên đăng nhập và Mật khẩu.";
                return;
            }

            if (RegPassword != RegConfirmPassword)
            {
                ErrorMessage = "Mật khẩu nhập lại không khớp.";
                return;
            }

            IsRegistering = true;
            ErrorMessage = "";

            try
            {
                using var db = new TroLySoCaNhanContext();

                // 1. Kiểm tra xem Username đã tồn tại chưa
                if (db.NguoiDungs.Any(u => u.TenDangNhap == RegUsername))
                {
                    ErrorMessage = "Tên đăng nhập này đã có người sử dụng.";
                    return;
                }

                await Task.Delay(800);

                // 2. THUẬT TOÁN TẠO EMAIL MẪU TỰ TĂNG
                int count = db.NguoiDungs.Count() + 1;
                string dummyEmail = $"useremail{count}@gmail.com";
                while (db.NguoiDungs.Any(u => u.Email == dummyEmail))
                {
                    count++;
                    dummyEmail = $"useremail{count}@gmail.com";
                }

                // 3. THUẬT TOÁN TẠO MÃ NGẪU NHIÊN DUY NHẤT (MaNgauNhien)
                string newUid = "";
                bool isUniqueUid = false;
                while (!isUniqueUid)
                {
                    // Lấy 6 ký tự ngẫu nhiên (chữ HOA + số)
                    string randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                    newUid = $"UID-{randomPart}";

                    // Kiểm tra dưới database xem mã này đã có ai xài chưa
                    if (!db.NguoiDungs.Any(u => u.MaNgauNhien == newUid))
                    {
                        isUniqueUid = true; // Nếu chưa có ai xài thì thoát vòng lặp, lấy mã này
                    }
                }

                // 4. THÊM MỚI VÀO DATABASE
                var newUser = new NguoiDung
                {
                    Id = Guid.NewGuid(),
                    TenDangNhap = RegUsername,
                    TenHienThi = RegUsername,
                    Email = dummyEmail,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(RegPassword),
                    MaNgauNhien = newUid, // <-- Sử dụng mã UID độc nhất vừa sinh ra
                    VaiTro = (byte)1, // Ép kiểu byte cho TINYINT của SQL
                    TrangThai = true,
                    SoDuVi = 0,
                    DungLuongToiDa = 5368709120, // 5GB mặc định
                    LuotAisuDung = 100,
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };

                db.NguoiDungs.Add(newUser);
                await db.SaveChangesAsync();

                // Đăng ký xong tự nhảy về form đăng nhập
                ErrorMessage = "Tạo tài khoản thành công! Hãy đăng nhập.";
                ActivePanel = "Login";
                Username = RegUsername;

                // Reset ô password cho an toàn
                RegPassword = string.Empty;
                RegConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                // Gọi InnerException để lấy lỗi sâu nhất từ SQL Server (nếu có)
                ErrorMessage = "Lỗi tạo tài khoản: " + (ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private async Task DoForgotAsync()
        {
            if (string.IsNullOrWhiteSpace(ForgotEmail) || !ForgotEmail.Contains("@"))
            {
                ErrorMessage = "Vui lòng nhập Email hợp lệ.";
                return;
            }

            IsSendingForgot = true;
            ErrorMessage = "";

            try
            {
                using var db = new TroLySoCaNhanContext();
                var user = db.NguoiDungs.FirstOrDefault(u => u.Email == ForgotEmail);

                await Task.Delay(1200);

                // NGĂN CHẶN KHÔI PHỤC BẰNG EMAIL GIẢ LẬP
                if (user == null || user.Email.StartsWith("useremail") && user.Email.EndsWith("@gmail.com"))
                {
                    ErrorMessage = "Tài khoản của bạn chưa cập nhật Email bảo mật. Vui lòng liên hệ Admin.";
                    return;
                }

                // TODO: Gọi dịch vụ gửi Email thật ở đây
                ErrorMessage = "Mã khôi phục đã được gửi vào Email của bạn!";
                ActivePanel = "Login";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi xử lý: " + ex.Message;
            }
            finally
            {
                IsSendingForgot = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}