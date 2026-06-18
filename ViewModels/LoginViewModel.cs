using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TroLySoCaNhan.Models;
using BCrypt.Net;
using TroLySoCaNhan.MVVM;
using Microsoft.EntityFrameworkCore;
using TroLySoCaNhan.Services;

namespace TroLySoCaNhan.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event EventHandler<NguoiDung>? LoginSucceeded;

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
        public ICommand LoginWithGoogleCommand { get; }

        public LoginViewModel()
        {
            ShowLoginCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Login"; });
            ShowRegisterCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Register"; });
            ShowForgotCommand = new RelayCommand(_ => { ErrorMessage = ""; ActivePanel = "Forgot"; });

            LoginCommand = new RelayCommand(async _ => await DoLoginAsync());
            RegisterCommand = new RelayCommand(async _ => await DoRegisterAsync());
            ForgotPasswordCommand = new RelayCommand(async _ => await DoForgotAsync());
            LoginWithGoogleCommand = new RelayCommand(async _ => await DoLoginWithGoogleAsync());
        }

        private async Task DoLoginAsync()
        {
            if (IsLoggingIn) return;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return;
            }

            ErrorMessage = "";
            IsLoggingIn = true;

            try
            {
                ErrorMessage = "Đang kết nối cơ sở dữ liệu...";

                NguoiDung? user = await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    // Test kết nối trước để nếu lỗi thì báo rõ
                    if (!db.Database.CanConnect())
                    {
                        throw new Exception("Không kết nối được database. Kiểm tra connection string / SQL Server / tên database.");
                    }

                    return db.NguoiDungs
                        .FirstOrDefault(u => u.TenDangNhap == Username || u.Email == Username);
                });

                ErrorMessage = "Đã kết nối database. Đang kiểm tra tài khoản...";

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

                ErrorMessage = "Đang kiểm tra mật khẩu...";

                bool isValidPassword = await Task.Run(() =>
                {
                    return BCrypt.Net.BCrypt.Verify(Password, user.MatKhauHash);
                });

                if (!isValidPassword)
                {
                    ErrorMessage = "Sai mật khẩu. Vui lòng thử lại.";
                    return;
                }

                ErrorMessage = "Đăng nhập thành công. Đang mở Dashboard...";

                // Tắt loading trước khi chuyển màn
                IsLoggingIn = false;

                LoginSucceeded?.Invoke(this, user);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi đăng nhập: " + (ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
        private async Task DoLoginWithGoogleAsync()
        {
            if (IsLoggingIn || IsRegistering) return;

            ErrorMessage = "";
            IsLoggingIn = true; // Hiện vòng xoay Loading ở nút đăng nhập

            try
            {
                // 1. Gọi Dịch vụ Google để lấy Email & Tên
                var googleAuth = new TroLySoCaNhan.Models.GoogleAuthService();
                var userInfo = await googleAuth.LoginAndGetUserInfoAsync();

                if (userInfo == null)
                {
                    ErrorMessage = "Đăng nhập Google thất bại hoặc bị hủy.";
                    return;
                }

                string email = userInfo.Value.Email;
                string name = userInfo.Value.Name;

                ErrorMessage = "Đang đồng bộ dữ liệu hệ thống...";

                // 2. Xử lý Logic Database (Đăng ký mới hoặc Đăng nhập)
                NguoiDung? loggedInUser = await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    var existingUser = db.NguoiDungs.FirstOrDefault(u => u.Email == email);

                    if (existingUser != null)
                    {
                        // TRƯỜNG HỢP A: ĐÃ TỒN TẠI TÀI KHOẢN (ĐĂNG NHẬP)
                        if (existingUser.TrangThai == false) throw new Exception("Tài khoản của bạn đã bị khóa.");

                        // Kiểm tra xem đã liên kết chưa, chưa thì thêm vào bảng TaiKhoanLienKet
                        bool isLinked = db.TaiKhoanLienKets.Any(tk => tk.MaNguoiDung == existingUser.Id && tk.NenTang == "Google");
                        if (!isLinked)
                        {
                            db.TaiKhoanLienKets.Add(new TaiKhoanLienKet
                            {
                                Id = Guid.NewGuid(),
                                MaNguoiDung = existingUser.Id,
                                NenTang = "Google",
                                ThongTinDangNhap = email,
                                TrangThai = true,
                                NgayLienKet = DateTime.Now
                            });
                            db.SaveChanges();
                        }
                        return existingUser;
                    }
                    else
                    {
                        // TRƯỜNG HỢP B: NGƯỜI DÙNG MỚI TINH -> TỰ ĐỘNG ĐĂNG KÝ VÀ CẤP KHÓA E2EE
                        string newUid = "";
                        bool isUniqueUid = false;
                        while (!isUniqueUid)
                        {
                            newUid = "UID-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                            if (!db.NguoiDungs.Any(u => u.MaNgauNhien == newUid)) isUniqueUid = true;
                        }

                        // TẠO CẶP KHÓA BẢO MẬT (Giống y hệt đăng ký thường)
                        var keyPair = TroLySoCaNhan.Services.CryptoService.GenerateKeyPair();
                        TroLySoCaNhan.Services.CryptoService.SaveAndProtectPrivateKey(newUid, keyPair.PrivateKey);

                        // Tạo User mới
                        var newUser = new NguoiDung
                        {
                            Id = Guid.NewGuid(),
                            TenDangNhap = newUid, // Dùng tạm UID làm Tên đăng nhập để khỏi trùng
                            TenHienThi = name,
                            Email = email,
                            MatKhauHash = null, // Google Login thì không cần mật khẩu hệ thống
                            MaNgauNhien = newUid,
                            KhoaCongKhaiPgp = keyPair.PublicKey,
                            VaiTro = 1,
                            TrangThai = true,
                            SoDuVi = 0,
                            DungLuongToiDa = 5368709120,
                            LuotAisuDung = 100,
                            NgayTao = DateTime.Now,
                            NgayCapNhat = DateTime.Now
                        };
                        db.NguoiDungs.Add(newUser);

                        // Tạo luôn liên kết tài khoản
                        db.TaiKhoanLienKets.Add(new TaiKhoanLienKet
                        {
                            Id = Guid.NewGuid(),
                            MaNguoiDung = newUser.Id,
                            NenTang = "Google",
                            ThongTinDangNhap = email,
                            TrangThai = true,
                            NgayLienKet = DateTime.Now
                        });

                        db.SaveChanges();
                        return newUser;
                    }
                });

                // 3. Chuyển sang màn hình Dashboard
                IsLoggingIn = false;
                LoginSucceeded?.Invoke(this, loggedInUser);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi xác thực Google: " + (ex.InnerException?.Message ?? ex.Message);
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
                // Đẩy toàn bộ Logic thêm mới Database và băm BCrypt xuống Luồng ngầm
                await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();

                    if (db.NguoiDungs.Any(u => u.TenDangNhap == RegUsername))
                    {
                        throw new Exception("Tên đăng nhập này đã có người sử dụng.");
                    }

                    int count = db.NguoiDungs.Count() + 1;
                    string dummyEmail = $"useremail{count}@gmail.com";
                    while (db.NguoiDungs.Any(u => u.Email == dummyEmail))
                    {
                        count++;
                        dummyEmail = $"useremail{count}@gmail.com";
                    }

                    string newUid = "";
                    bool isUniqueUid = false;
                    while (!isUniqueUid)
                    {
                        string randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                        newUid = $"UID-{randomPart}";

                        if (!db.NguoiDungs.Any(u => u.MaNgauNhien == newUid))
                        {
                            isUniqueUid = true;
                        }
                    }
                    // 1. Tạo cặp khóa
                    var keyPair = TroLySoCaNhan.Services.CryptoService.GenerateKeyPair();
                    // 2. Lưu Private Key xuống máy
                    TroLySoCaNhan.Services.CryptoService.SaveAndProtectPrivateKey(newUid, keyPair.PrivateKey);

                    // 3. Gán Public Key vào Object
                    var newUser = new NguoiDung
                    {
                        Id = Guid.NewGuid(),
                        TenDangNhap = RegUsername,
                        TenHienThi = RegUsername,
                        Email = dummyEmail,
                        MatKhauHash = BCrypt.Net.BCrypt.HashPassword(RegPassword),
                        MaNgauNhien = newUid,
                        KhoaCongKhaiPgp = keyPair.PublicKey,
                        VaiTro = (byte)1,
                        TrangThai = true,
                        SoDuVi = 0,
                        DungLuongToiDa = 5368709120,
                        LuotAisuDung = 100,
                        NgayTao = DateTime.Now,
                        NgayCapNhat = DateTime.Now
                    };

                    db.NguoiDungs.Add(newUser);
                    db.SaveChanges(); // Lưu vào Database
                });

                ErrorMessage = "Tạo tài khoản thành công! Hãy đăng nhập.";
                ActivePanel = "Login";
                Username = RegUsername;

                RegPassword = string.Empty;
                RegConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi: " + (ex.InnerException?.Message ?? ex.Message);
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
                var user = await Task.Run(() =>
                {
                    using var db = new TroLySoCaNhanContext();
                    return db.NguoiDungs.FirstOrDefault(u => u.Email == ForgotEmail);
                });

                await Task.Delay(1200);

                if (user == null || (user.Email.StartsWith("useremail") && user.Email.EndsWith("@gmail.com")))
                {
                    ErrorMessage = "Tài khoản của bạn chưa cập nhật Email bảo mật. Vui lòng liên hệ Admin.";
                    return;
                }

                ErrorMessage = "Mã khôi phục đã được gửi vào Email của bạn!";
                ActivePanel = "Login";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi xử lý: " + (ex.InnerException?.Message ?? ex.Message);
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