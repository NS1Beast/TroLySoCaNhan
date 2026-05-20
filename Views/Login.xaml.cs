using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class Login : FluentWindow
    {
        public Login()
        {
            InitializeComponent();
        }

        // ==========================================
        // 1. HIỆU ỨNG PARALLAX (DI CHUỘT)
        // ==========================================
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            double centerX = this.ActualWidth / 2;
            double centerY = this.ActualHeight / 2;

            double offsetX = mousePos.X - centerX;

            // Chữ (Branding) di chuyển ngược chiều chuột một chút
            transBranding.X = -offsetX * 0.015;

            // Form đăng nhập di chuyển cùng chiều chuột
            transCard.X = offsetX * 0.01;
        }

        // ==========================================
        // 2. HOẠT ẢNH CHUYỂN PANEL (SLIDE ANIMATION)
        // ==========================================
        private void SlidePanel(System.Windows.Media.TranslateTransform target, double toValue)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut } // Gia tốc mượt
            };
            target.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        private void LinkToRegister_Click(object sender, RoutedEventArgs e)
        {
            SlidePanel(transLogin, -450); // Đẩy Login sang trái cho khuất hẳn
            SlidePanel(transRegister, 0); // Kéo Register vào giữa
        }

        private void LinkToLogin_Click(object sender, RoutedEventArgs e)
        {
            SlidePanel(transRegister, 450); // Đẩy Register về bên phải
            SlidePanel(transLogin, 0);      // Kéo Login về giữa
        }

        private void LinkToForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            SlidePanel(transLogin, -450); // Đẩy Login sang trái
            SlidePanel(transForgot, 0);   // Kéo Forgot vào giữa
        }

        private void LinkToLoginFromForgot_Click(object sender, RoutedEventArgs e)
        {
            SlidePanel(transForgot, 450); // Đẩy Forgot về bên phải
            SlidePanel(transLogin, 0);    // Kéo Login về giữa
        }

        // ==========================================
        // 3. XỬ LÝ NÚT BẤM (GIẢ LẬP LOADING)
        // ==========================================
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Bật hiệu ứng Loading xoay xoay
            txtLoginBtn.Visibility = Visibility.Collapsed;
            ringLoading.Visibility = Visibility.Visible;
            btnLogin.IsEnabled = false;

            // TODO: Gọi API / Database hoặc SQL Server ở đây
            await Task.Delay(1500); // Giả lập delay mạng

            // Tắt hiệu ứng Loading
            txtLoginBtn.Visibility = Visibility.Visible;
            ringLoading.Visibility = Visibility.Collapsed;
            btnLogin.IsEnabled = true;

            // MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            txtRegBtnContent.Visibility = Visibility.Collapsed;
            ringRegLoading.Visibility = Visibility.Visible;
            btnRegister.IsEnabled = false;

            await Task.Delay(1500);

            txtRegBtnContent.Visibility = Visibility.Visible;
            ringRegLoading.Visibility = Visibility.Collapsed;
            btnRegister.IsEnabled = true;
        }

        private async void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            txtForgotBtnContent.Visibility = Visibility.Collapsed;
            ringForgotLoading.Visibility = Visibility.Visible;
            btnForgotPassword.IsEnabled = false;

            await Task.Delay(1500);

            txtForgotBtnContent.Visibility = Visibility.Visible;
            ringForgotLoading.Visibility = Visibility.Collapsed;
            btnForgotPassword.IsEnabled = true;
        }
    }
}