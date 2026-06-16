using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;
using BCrypt.Net;

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

        // --- BIẾN ĐĂNG KÝ ---
        private string _regFullName = string.Empty;
        public string RegFullName { get => _regFullName; set { _regFullName = value; OnPropertyChanged(); } }
        private string _regEmail = string.Empty; // Sẽ dùng làm Username
        public string RegEmail { get => _regEmail; set { _regEmail = value; OnPropertyChanged(); } }
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

        // --- LỆNH NÚT BẤM (COMMANDS) ---
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
                // Truy vấn Database
                using var db = new TroLySoCaNhanContext();
                var user = db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == Username || u.Email == Username);

                // Giả lập chút delay để UI quay vòng vòng nhìn cho xịn
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

                // Verify mật khẩu băm
                if (!BCrypt.Net.BCrypt.Verify(Password, user.MatKhauHash))
                {
                    ErrorMessage = "Sai mật khẩu. Vui lòng thử lại.";
                    return;
                }

                // Thành công -> Bắn event để form Login.xaml.cs nhảy trang
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
            if (string.IsNullOrWhiteSpace(RegFullName) || string.IsNullOrWhiteSpace(RegEmail) || string.IsNullOrWhiteSpace(RegPassword))
            {
                ErrorMessage = "Vui lòng điền đủ thông tin.";
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

                // Kiểm tra xem Username (hiện đang nhập ở ô Email) đã tồn tại chưa
                if (db.NguoiDungs.Any(u => u.TenDangNhap == RegEmail))
                {
                    ErrorMessage = "Tên đăng nhập này đã có người sử dụng.";
                    return;
                }

                await Task.Delay(1000); // Fake delay cho mượt UI

                // Tạo 1 email giả nếu họ không nhập dạng email thật (để qua ải DB NOT NULL)
                string realEmail = RegEmail.Contains("@") ? RegEmail : $"{RegEmail}@no-email.local";

                var newUser = new NguoiDung
                {
                    Id = Guid.NewGuid(),
                    TenDangNhap = RegEmail, // Dùng giá trị họ nhập làm Username
                    TenHienThi = RegFullName,
                    Email = realEmail,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(RegPassword), // BĂM MẬT KHẨU
                    MaNgauNhien = $"UID-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}", // Khởi tạo ID ngẫu nhiên
                    VaiTro = 1,
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
                Username = RegEmail; // Gắn sẵn tên vừa tạo vào ô đăng nhập
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi tạo tài khoản: " + ex.Message;
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

                await Task.Delay(1200); // Fake gửi email

                // Kiểm tra xem email có phải email giả không
                if (user == null || user.Email.EndsWith("@no-email.local"))
                {
                    ErrorMessage = "Tài khoản của bạn chưa liên kết Email hợp lệ để khôi phục.";
                    return;
                }

                // TODO: Code gửi Email thật bằng SmtpClient ở đây

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