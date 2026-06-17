using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class Login : FluentWindow
    {
        public Login()
        {
            InitializeComponent();

            // Lắng nghe sự thay đổi của DataContext để tránh memory leak
            this.DataContextChanged += Login_DataContextChanged;

            // Xử lý trường hợp DataContext đã được gán sẵn từ file XAML
            if (this.DataContext is LoginViewModel vm)
            {
                HookViewModel(vm);
            }
        }

        // ==========================================
        // 1. QUẢN LÝ SỰ KIỆN VIEWMODEL (CHỐNG MEMORY LEAK)
        // ==========================================
        private void Login_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Gỡ bỏ event cũ
            if (e.OldValue is LoginViewModel oldVm)
            {
                UnhookViewModel(oldVm);
            }

            // Gắn event mới
            if (e.NewValue is LoginViewModel newVm)
            {
                HookViewModel(newVm);
            }
        }

        private void HookViewModel(LoginViewModel vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;
            vm.LoginSucceeded += OnLoginSucceeded; // Đăng ký sự kiện nhảy sang Dashboard
        }

        private void UnhookViewModel(LoginViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.LoginSucceeded -= OnLoginSucceeded;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Nếu ViewModel ra lệnh đổi Form (thuộc tính ActivePanel thay đổi)
            if (e.PropertyName == nameof(LoginViewModel.ActivePanel))
            {
                if (DataContext is LoginViewModel vm)
                {
                    AnimatePanels(vm.ActivePanel);
                }
            }
        }

        // ==========================================
        // 2. HOẠT ẢNH CHUYỂN PANEL (SLIDE & FADE)
        // ==========================================
        private void AnimatePanels(string activePanel)
        {
            // Cài đặt thời gian và hiệu ứng nội suy (Ease) mượt mà
            TimeSpan duration = TimeSpan.FromMilliseconds(400);
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            // Các hoạt ảnh trượt qua lại (Translate X)
            var slideOutLeft = new DoubleAnimation(-450, duration) { EasingFunction = ease };
            var slideOutRight = new DoubleAnimation(450, duration) { EasingFunction = ease };
            var slideInCenter = new DoubleAnimation(0, duration) { EasingFunction = ease };

            // Các hoạt ảnh mờ dần / rõ dần (Opacity)
            var fadeOut = new DoubleAnimation(0, duration);
            var fadeIn = new DoubleAnimation(1, duration);

            // Bật/tắt HitTestVisible để user không click nhầm vào form đang bị ẩn
            pnlLogin.IsHitTestVisible = (activePanel == "Login");
            pnlRegister.IsHitTestVisible = (activePanel == "Register");
            pnlForgotPassword.IsHitTestVisible = (activePanel == "Forgot");

            // Xử lý chạy Animation tùy theo Form được chọn
            if (activePanel == "Login")
            {
                // Form Login trượt từ trái vào giữa
                transLogin.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideInCenter);
                pnlLogin.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                // Form Đăng ký trượt về bên phải
                transRegister.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutRight);
                pnlRegister.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                // Form Quên MK trượt về bên phải
                transForgot.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutRight);
                pnlForgotPassword.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            else if (activePanel == "Register")
            {
                // Form Login trượt sang trái
                transLogin.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutLeft);
                pnlLogin.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                // Form Đăng ký trượt từ phải vào giữa
                transRegister.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideInCenter);
                pnlRegister.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                transForgot.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutRight);
                pnlForgotPassword.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            else if (activePanel == "Forgot")
            {
                // Form Login trượt sang trái
                transLogin.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutLeft);
                pnlLogin.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                transRegister.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOutRight);
                pnlRegister.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                // Form Quên MK trượt từ phải vào giữa
                transForgot.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideInCenter);
                pnlForgotPassword.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        // ==========================================
        // 3. CÁC HÀM XỬ LÝ SỰ KIỆN GIAO DIỆN KHÁC
        // ==========================================

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm)
            {
                // Ép kiểu sender về dạng ui:PasswordBox của thư viện Wpf.Ui
                if (sender is PasswordBox passBox)
                {
                    vm.Password = passBox.Password;
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // Hiệu ứng Parallax (Di chuyển các khối nhẹ theo chuột)
            Point mousePos = e.GetPosition(this);
            double centerX = this.ActualWidth / 2;
            double centerY = this.ActualHeight / 2;
            double offsetX = mousePos.X - centerX;
            transBranding.X = -offsetX * 0.015;
            transCard.X = offsetX * 0.01;
        }

        // ==========================================
        // 4. CHUYỂN SANG DASHBOARD KHI LOGIN THÀNH CÔNG
        // ==========================================
        private void OnLoginSucceeded(object? sender, System.EventArgs e)
        {
            var dashboard = new DashBoard();
            dashboard.Show();
            this.Close();
        }
        private void txtRegPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm && sender is PasswordBox passBox)
            {
                vm.RegPassword = passBox.Password;
            }
        }

        // Sự kiện lấy mật khẩu xác nhận lúc Đăng ký
        private void txtRegConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm && sender is PasswordBox passBox)
            {
                vm.RegConfirmPassword = passBox.Password;
            }
        }
    }
}