using System;
using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class ProfileWindow : FluentWindow
    {
        private ProfileViewModel? _viewModel;

        // Constructor rỗng, giữ lại cho WPF Designer
        public ProfileWindow()
        {
            InitializeComponent();
        }

        // Constructor nhận UserDto
        public ProfileWindow(UserDto currentUser)
        {
            InitializeComponent();

            _viewModel = new ProfileViewModel(currentUser);
            _viewModel.LogoutRequested += ProfileViewModel_LogoutRequested;

            DataContext = _viewModel;
        }

        private void ProfileViewModel_LogoutRequested(object? sender, EventArgs e)
        {
            Window? dashboardWindow = Owner;
            var loginWindow = new Login();

            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            // Đóng Profile
            Close();

            // Đóng Dashboard nếu Profile được mở từ Dashboard
            dashboardWindow?.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LogoutRequested -= ProfileViewModel_LogoutRequested;
                _viewModel = null;
            }

            base.OnClosed(e);
        }

        private void txtOldPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
            {
                vm.OldPassword = txtOldPassword.Password;
                vm.ChangePasswordCommand.RaiseCanExecuteChanged();
            }
        }

        private void txtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
            {
                vm.NewPassword = txtNewPassword.Password;
                vm.ChangePasswordCommand.RaiseCanExecuteChanged();
            }
        }

        private void txtConfirmNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
            {
                vm.ConfirmNewPassword = txtConfirmNewPassword.Password;
                vm.ChangePasswordCommand.RaiseCanExecuteChanged();
            }
        }
    }
}