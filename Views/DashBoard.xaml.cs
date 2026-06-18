using System.Windows;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class DashBoard : FluentWindow
    {
        public DashBoard(NguoiDung userDb)
        {
            InitializeComponent();
            var vm = new DashboardViewModel(userDb);
            this.DataContext = vm;

            vm.ProfileRequested += (_, _) =>
            {
                var profileWindow = new ProfileWindow(vm.CurrentUser) { Owner = this };
                profileWindow.ShowDialog();
            };

            vm.SettingsRequested += (_, _) =>
            {
                this.Hide();
                var profileWindow = new ProfileWindow(vm.CurrentUser) { Owner = this };
                profileWindow.ShowDialog();
                this.Show();
            };

            vm.UpgradeRequested += (_, _) =>
            {
                this.Hide();
                var upgradeWindow = new UpgradeDialog(vm.CurrentUser) { Owner = this };
                upgradeWindow.ShowDialog();
                this.Show();
            };

            vm.GroupRequested += (_, _) =>
            {
                this.Hide(); 
                var groupWindow = new Group(vm.CurrentUser) { Owner = this };
                groupWindow.ShowDialog(); 
                this.Show(); 
            };

            vm.StorageRequested += (_, _) =>
            {
                this.Hide();
                var storageWindow = new Storage(vm.CurrentUser) { Owner = this }; 
                storageWindow.ShowDialog();
                this.Show();
            };
        }
    }
}