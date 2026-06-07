using System.Windows;
using TroLySoCaNhan.Views;

namespace TroLySoCaNhan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Mở cửa sổ Login đầu tiên
            var loginWindow = new Login();
            loginWindow.Show();
        }
    }
}