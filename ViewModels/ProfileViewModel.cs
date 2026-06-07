using System;
using System.Threading.Tasks;
using System.Windows;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;
using Wpf.Ui.Appearance;

namespace TroLySoCaNhan.ViewModels
{
    /// <summary>
    /// ViewModel cho màn Hồ sơ & Cài đặt.
    /// Bao gồm 2 tab: "Hồ sơ" (thông tin, đổi mật khẩu) và "Cài đặt" (theme, thông báo).
    /// </summary>
    public class ProfileViewModel : ViewModelBase
    {
        public User User { get; }

        // ==========================================
        // ĐỔI MẬT KHẨU
        // ==========================================
        private string _oldPassword = string.Empty;
        public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }

        private string _newPassword = string.Empty;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _confirmNewPassword = string.Empty;
        public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

        private bool _isChangingPassword;
        public bool IsChangingPassword
        {
            get => _isChangingPassword;
            set
            {
                if (SetProperty(ref _isChangingPassword, value))
                    ChangePasswordCommand.RaiseCanExecuteChanged();
            }
        }

        // ==========================================
        // CÀI ĐẶT
        // ==========================================
        /// <summary>"Light" | "Dark" | "System"</summary>
        private string _selectedTheme = "System";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                    ApplyTheme();
            }
        }

        private bool _notificationsEnabled = true;
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set => SetProperty(ref _notificationsEnabled, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                    OnPropertyChanged(nameof(HasStatus));
            }
        }
        public bool HasStatus => !string.IsNullOrEmpty(_statusMessage);

        private bool _isStatusError;
        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetProperty(ref _isStatusError, value);
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public RelayCommand CopyIdCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CloseCommand { get; }

        public event EventHandler? CloseRequested;

        public ProfileViewModel(User user)
        {
            User = user;

            CopyIdCommand = new RelayCommand(_ => CopyIdToClipboard());
            ChangePasswordCommand = new RelayCommand(async _ => await DoChangePasswordAsync(),
                _ => !IsChangingPassword
                     && !string.IsNullOrWhiteSpace(OldPassword)
                     && !string.IsNullOrWhiteSpace(NewPassword)
                     && NewPassword == ConfirmNewPassword);
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        }

        // ==========================================
        // HANDLERS
        // ==========================================
        private void CopyIdToClipboard()
        {
            try
            {
                Clipboard.SetText(User.Id);
                ShowStatus($"Đã copy mã: {User.Id}", false);
            }
            catch
            {
                ShowStatus("Không thể copy. Hãy chọn và copy thủ công.", true);
            }
        }

        private async Task DoChangePasswordAsync()
        {
            IsChangingPassword = true;
            try
            {
                // TODO: gọi API đổi mật khẩu
                await Task.Delay(1200);
                OldPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                ShowStatus("Đổi mật khẩu thành công.", false);
            }
            finally
            {
                IsChangingPassword = false;
            }
        }

        private void ApplyTheme()
        {
            try
            {
                switch (SelectedTheme)
                {
                    case "Light":
                        ApplicationThemeManager.Apply(ApplicationTheme.Light);
                        break;
                    case "Dark":
                        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                        break;
                    default:
                        // "System" — WPF-UI sẽ theo Windows
                        var sysTheme = SystemParameters.WindowGlassBrush == null
                            ? ApplicationTheme.Light
                            : ApplicationTheme.Dark;
                        ApplicationThemeManager.Apply(sysTheme);
                        break;
                }
            }
            catch
            {
                // Không crash nếu theme service chưa sẵn sàng
            }
        }

        private void ShowStatus(string msg, bool isError)
        {
            StatusMessage = msg;
            IsStatusError = isError;
        }
    }
}
