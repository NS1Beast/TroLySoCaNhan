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

            // Subscribe sự kiện đăng nhập thành công từ ViewModel
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginSucceeded += OnLoginSucceeded;
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(LoginViewModel.ActivePanel))
                        SlideToPanel(vm.ActivePanel);
                };
            }
        }


        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TroLySoCaNhan.ViewModels.LoginViewModel vm)
            {
                // Ép kiểu sender về dạng ui:PasswordBox của thư viện Wpf.Ui
                if (sender is Wpf.Ui.Controls.PasswordBox passBox)
                {
                    vm.Password = passBox.Password;
                }
            }
        }


        // ==========================================
        // 1. HIỆU ỨNG PARALLAX (DI CHUỘT) — chỉ là hiệu ứng thị giác, KHÔNG phải business logic
        // ==========================================
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            double centerX = this.ActualWidth / 2;
            double centerY = this.ActualHeight / 2;
            double offsetX = mousePos.X - centerX;
            transBranding.X = -offsetX * 0.015;
            transCard.X = offsetX * 0.01;
        }

        // ==========================================
        // 2. HOẠT ẢNH CHUYỂN PANEL — thuần túy UI animation, điều khiển bởi ViewModel
        // ==========================================
        private void SlideToPanel(string panel)
        {
            switch (panel)
            {
                case "Login":
                    SlidePanel(transLogin, 0);
                    SlidePanel(transRegister, 420);
                    SlidePanel(transForgot, 420);
                    break;
                case "Register":
                    SlidePanel(transLogin, -450);
                    SlidePanel(transRegister, 0);
                    SlidePanel(transForgot, 420);
                    break;
                case "Forgot":
                    SlidePanel(transLogin, -450);
                    SlidePanel(transRegister, 420);
                    SlidePanel(transForgot, 0);
                    break;
            }
        }

        private void SlidePanel(System.Windows.Media.TranslateTransform target, double toValue)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
            };
            target.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        // ==========================================
        // 3. CHUYỂN SANG DASHBOARD KHI LOGIN THÀNH CÔNG
        // ==========================================
        private void OnLoginSucceeded(object? sender, System.EventArgs e)
        {
            var dashboard = new DashBoard();
            dashboard.Show();
            this.Close();
        }
    }
}
