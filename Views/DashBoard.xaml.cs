using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class DashBoard : FluentWindow
    {
        public DashBoard()
        {
            InitializeComponent();

            // Subscribe các yêu cầu mở Window mới
            if (DataContext is DashboardViewModel vm)
            {
                vm.ProfileRequested += (_, _) =>
                {
                    var win = new ProfileWindow { Owner = this };
                    win.ShowDialog();
                };
                vm.SettingsRequested += (_, _) =>
                {
                    var win = new ProfileWindow(vm.CurrentUser) { Owner = this };
                    win.ShowDialog();
                };
                vm.UpgradeRequested += (_, _) =>
                {
                    var dlg = new UpgradeDialog { Owner = this };
                    dlg.ShowDialog();
                };
            }
        }
    }
}
