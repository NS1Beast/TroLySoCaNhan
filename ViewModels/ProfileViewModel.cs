using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.MVVM;
using Wpf.Ui.Appearance;

namespace TroLySoCaNhan.ViewModels
{
    public class UserDto : ViewModelBase
    {
        public Guid DbId { get; set; }
        private string _id = string.Empty;
        public string Id { get => _id; set => SetProperty(ref _id, value); }
        private string _userName = string.Empty;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }
        private string _displayName = string.Empty;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        private string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        private string _plan = "Cơ bản";
        public string Plan { get => _plan; set => SetProperty(ref _plan, value); }
        private string _shareCode = string.Empty;
        public string ShareCode { get => _shareCode; set => SetProperty(ref _shareCode, value); }
    }

    public class ProfileViewModel : ViewModelBase
    {
        public UserDto User { get; }

        private bool _isEmailEditable = true;
        public bool IsEmailEditable { get => _isEmailEditable; set => SetProperty(ref _isEmailEditable, value); }

        private bool _isPasswordSectionVisible = true;
        public bool IsPasswordSectionVisible { get => _isPasswordSectionVisible; set => SetProperty(ref _isPasswordSectionVisible, value); }

        private string _googleWarningMessage = string.Empty;
        public string GoogleWarningMessage { get => _googleWarningMessage; set => SetProperty(ref _googleWarningMessage, value); }

        private string _oldPassword = string.Empty;
        public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }

        private string _newPassword = string.Empty;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _confirmNewPassword = string.Empty;
        public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

        private string _copyMessage = string.Empty;
        public string CopyMessage { get => _copyMessage; set => SetProperty(ref _copyMessage, value); }

        private bool _isChangingPassword;
        public bool IsChangingPassword { get => _isChangingPassword; set { if (SetProperty(ref _isChangingPassword, value)) ChangePasswordCommand.RaiseCanExecuteChanged(); } }

        private bool _isUpdatingProfile;
        public bool IsUpdatingProfile { get => _isUpdatingProfile; set { if (SetProperty(ref _isUpdatingProfile, value)) UpdateProfileCommand.RaiseCanExecuteChanged(); } }

        private string _selectedTheme = "Light";
        public string SelectedTheme { get => _selectedTheme; set => SetProperty(ref _selectedTheme, value); }

        public List<string> Languages { get; } = new List<string> { "Tiếng Việt (vi-VN)", "English (en-US)" };
        private string _selectedLanguage = "Tiếng Việt (vi-VN)";
        public string SelectedLanguage { get => _selectedLanguage; set => SetProperty(ref _selectedLanguage, value); }

        private bool _notificationsEnabled = true;
        public bool NotificationsEnabled { get => _notificationsEnabled; set => SetProperty(ref _notificationsEnabled, value); }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; set { if (SetProperty(ref _statusMessage, value)) OnPropertyChanged(nameof(HasStatus)); } }
        public bool HasStatus => !string.IsNullOrEmpty(_statusMessage);

        private bool _isStatusError;
        public bool IsStatusError { get => _isStatusError; set => SetProperty(ref _isStatusError, value); }

        // ==========================================
        // KHAI BÁO COMMAND
        // ==========================================
        public RelayCommand CopyIdCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand UpdateProfileCommand { get; }
        public RelayCommand ChangeThemeCommand { get; } // Command đổi theme mới

        public ProfileViewModel(UserDto user)
        {
            User = user;

            if (User.Email.StartsWith("useremail") && User.Email.EndsWith("@gmail.com"))
            {
                User.Email = "";
            }

            using (var db = new TroLySoCaNhanContext())
            {
                bool isGoogleAccount = db.TaiKhoanLienKets.Any(tk => tk.MaNguoiDung == user.DbId && tk.NenTang == "Google");
                if (isGoogleAccount)
                {
                    IsEmailEditable = false;
                    IsPasswordSectionVisible = false;
                    GoogleWarningMessage = "Tài khoản liên kết Google: Không thể thay đổi Email và Mật khẩu hệ thống.";
                }
            }

            CopyIdCommand = new RelayCommand(async _ => await CopyIdToClipboardAsync());
            UpdateProfileCommand = new RelayCommand(async _ => await DoUpdateProfileAsync(), _ => !IsUpdatingProfile);
            ChangePasswordCommand = new RelayCommand(async _ => await DoChangePasswordAsync(), _ => !IsChangingPassword && !string.IsNullOrWhiteSpace(OldPassword) && !string.IsNullOrWhiteSpace(NewPassword) && NewPassword == ConfirmNewPassword);

            // Lệnh đổi Theme
            ChangeThemeCommand = new RelayCommand(param =>
            {
                if (param is string theme)
                {
                    SelectedTheme = theme;
                    ApplyTheme();
                }
            });
        }

        private async Task CopyIdToClipboardAsync()
        {
            try
            {
                Clipboard.SetText(User.ShareCode);
                CopyMessage = "✓ Đã copy vào bộ nhớ tạm";
                await Task.Delay(2500);
                CopyMessage = string.Empty;
            }
            catch { CopyMessage = "Lỗi! Không thể copy."; }
        }

        private async Task DoUpdateProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(User.UserName)) { ShowStatus("Tên đăng nhập không được để trống!", true); return; }

            IsUpdatingProfile = true;
            try
            {
                using var db = new TroLySoCaNhanContext();
                var dbUser = db.NguoiDungs.FirstOrDefault(u => u.Id == User.DbId);
                if (dbUser != null)
                {
                    if (db.NguoiDungs.Any(u => u.TenDangNhap == User.UserName && u.Id != User.DbId))
                    {
                        ShowStatus("Tên đăng nhập (Username) này đã có người sử dụng!", true);
                        return;
                    }

                    if (IsEmailEditable && !string.IsNullOrWhiteSpace(User.Email))
                    {
                        if (db.NguoiDungs.Any(u => u.Email == User.Email && u.Id != User.DbId))
                        {
                            ShowStatus("Email này đã tồn tại trong hệ thống!", true);
                            return;
                        }
                    }

                    dbUser.TenDangNhap = User.UserName;
                    dbUser.TenHienThi = User.DisplayName;

                    if (IsEmailEditable && !string.IsNullOrWhiteSpace(User.Email))
                    {
                        dbUser.Email = User.Email;
                    }

                    dbUser.NgayCapNhat = DateTime.Now;
                    await db.SaveChangesAsync();
                    ShowStatus("Cập nhật thông tin thành công!", false);
                }
            }
            catch (Exception ex) { ShowStatus("Lỗi cập nhật: " + ex.Message, true); }
            finally { IsUpdatingProfile = false; }
        }

        private async Task DoChangePasswordAsync()
        {
            IsChangingPassword = true;
            try
            {
                using var db = new TroLySoCaNhanContext();
                var dbUser = db.NguoiDungs.FirstOrDefault(u => u.Id == User.DbId);
                if (dbUser == null) { ShowStatus("Lỗi: Không tìm thấy tài khoản.", true); return; }

                if (string.IsNullOrEmpty(dbUser.MatKhauHash) || !BCrypt.Net.BCrypt.Verify(OldPassword, dbUser.MatKhauHash))
                {
                    ShowStatus("Mật khẩu hiện tại không đúng.", true);
                    return;
                }

                dbUser.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                dbUser.NgayCapNhat = DateTime.Now;
                await db.SaveChangesAsync();

                OldPassword = string.Empty; NewPassword = string.Empty; ConfirmNewPassword = string.Empty;
                ShowStatus("Đổi mật khẩu thành công!", false);
            }
            catch (Exception ex) { ShowStatus("Lỗi đổi mật khẩu: " + ex.Message, true); }
            finally { IsChangingPassword = false; }
        }

        private void ApplyTheme()
        {
            try
            {
                // Gọi thư viện Wpf.Ui đổi Theme toàn cục
                if (SelectedTheme == "Dark")
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                else if (SelectedTheme == "Light")
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                else
                    ApplicationThemeManager.ApplySystemTheme();
            }
            catch (Exception ex) { ShowStatus("Lỗi đổi Theme: " + ex.Message, true); }
        }

        private void ShowStatus(string msg, bool isError) { StatusMessage = msg; IsStatusError = isError; }
    }
}