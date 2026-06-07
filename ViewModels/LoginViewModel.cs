using System;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using TroLySoCaNhan.MVVM;
using Wpf.Ui.Appearance;

namespace TroLySoCaNhan.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Login/Register/Forgot Password.
    /// Mọi thao tác click/typing đều qua Binding, code-behind chỉ còn hiệu ứng Parallax.
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        // ==========================================
        // INPUT
        // ==========================================
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    // BẮT BUỘC PHẢI CÓ DÒNG NÀY để đánh thức nút Login
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    // BẮT BUỘC PHẢI CÓ DÒNG NÀY để đánh thức nút Login
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _regFullName = string.Empty;
        public string RegFullName { get => _regFullName; set => SetProperty(ref _regFullName, value); }

        private string _regEmail = string.Empty;
        public string RegEmail { get => _regEmail; set => SetProperty(ref _regEmail, value); }

        private string _regPassword = string.Empty;
        public string RegPassword { get => _regPassword; set => SetProperty(ref _regPassword, value); }

        private string _regConfirmPassword = string.Empty;
        public string RegConfirmPassword { get => _regConfirmPassword; set => SetProperty(ref _regConfirmPassword, value); }

        private string _forgotEmail = string.Empty;
        public string ForgotEmail { get => _forgotEmail; set => SetProperty(ref _forgotEmail, value); }

        // Chế độ đăng nhập admin - không cần tên/mật khẩu
        private bool isAdminModeEnabled;
        public bool AdminModeEnabled
        {
            get => isAdminModeEnabled;
            set => SetProperty(ref isAdminModeEnabled, value);
        }

        /// <summary>Chế độ Guest/Quản trị viên: B .</summary>
        //private RelayCommand ShowAdminLoginCommand;
        //public bool RememberMe { get => _rememberMe; set => SetProperty(ref _rememberMe, value); }

        // ==========================================
        // TRẠNG THÁI UI
        // ==========================================
        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                if (SetProperty(ref _isLoggingIn, value))
                {
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isRegistering;
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                if (SetProperty(ref _isRegistering, value))
                {
                    RegisterCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isSendingForgot;
        public bool IsSendingForgot
        {
            get => _isSendingForgot;
            set
            {
                if (SetProperty(ref _isSendingForgot, value))
                {
                    ForgotPasswordCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>Chế độ đăng nhập admin - lưu mật khẩu.</summary>
        private bool _rememberMe = false;

        public bool RememberMe { get => _rememberMe; set => SetProperty(ref _rememberMe, value); }

        /// <summary>Panel hiện tại đang hiển thị: "Login" / "Register" / "Forgot".</summary>
        private string _activePanel = "Login";
        public string ActivePanel
        {
            get => _activePanel;
            set
            {
                if (SetProperty(ref _activePanel, value))
                    OnPropertyChanged(nameof(IsLoginPanel));
            }
        }
        public bool IsLoginPanel => _activePanel == "Login";

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public RelayCommand LoginCommand { get; }
        public RelayCommand ShowAdminLoginCommand { get; }
        public RelayCommand RegisterCommand { get; }
        public RelayCommand ForgotPasswordCommand { get; }
        public RelayCommand ShowRegisterCommand { get; }
        public RelayCommand ShowLoginCommand { get; }
        public RelayCommand ShowForgotCommand { get; }

        /// <summary>Sự kiện đăng nhập thành công — code-behind sẽ subscribe để đóng cửa sổ Login và mở Dashboard.</summary>
        public event EventHandler? LoginSucceeded;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await DoLoginAsync(), _ => !IsLoggingIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
            RegisterCommand = new RelayCommand(async _ => await DoRegisterAsync(), _ => !IsRegistering
                && !string.IsNullOrWhiteSpace(RegFullName)
                && !string.IsNullOrWhiteSpace(RegEmail)
                && !string.IsNullOrWhiteSpace(RegPassword)
                && RegPassword == RegConfirmPassword);
            ForgotPasswordCommand = new RelayCommand(async _ => await DoForgotAsync(), _ => !IsSendingForgot && !string.IsNullOrWhiteSpace(ForgotEmail));

            ShowRegisterCommand = new RelayCommand(_ => ActivePanel = "Register");
            ShowLoginCommand = new RelayCommand(_ => ActivePanel = "Login");
            ShowForgotCommand = new RelayCommand(_ => ActivePanel = "Forgot");

            ShowAdminLoginCommand = new RelayCommand(async _ => await DoAdminLoginAsync());
        }

        // ==========================================
        // HANDLERS (Bật chế độ admin nếu chưa vào) - bypass login check!
        // ==========================================

        private async Task DoAdminLoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;

            try
            {
                // Bật trạng thái Admin
                AdminModeEnabled = true;

                // Mô phỏng thời gian chờ API
                await Task.Delay(1000);

                // Gửi sự kiện báo đăng nhập thành công cho View biết để mở cửa sổ mới
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsLoggingIn = false;
            }
        }


        private async Task DoLoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;
            try
            {
                // TODO: gọi API đăng nhập thật ở đây
                await Task.Delay(1500);
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private async Task DoRegisterAsync()
        {
            ErrorMessage = string.Empty;
            IsRegistering = true;
            try
            {
                // TODO: gọi API đăng ký thật ở đây
                await Task.Delay(1500);
                ActivePanel = "Login"; // Quay về màn login
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private async Task DoForgotAsync()
        {
            ErrorMessage = string.Empty;
            IsSendingForgot = true;
            try
            {
                // TODO: gọi API quên mật khẩu thật ở đây
                await Task.Delay(1500);
                ActivePanel = "Login";
            }
            finally
            {
                IsSendingForgot = false;
            }
        }
    }
}
