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

            // Nạp dữ liệu thật vào ViewModel
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
                var upgradeWindow = new UpgradeDialog { Owner = this };
                upgradeWindow.ShowDialog();
                this.Show();
            };

            vm.GroupRequested += (_, _) =>
            {
                this.Hide(); // Ẩn Dashboard
                var groupWindow = new Group { Owner = this };
                groupWindow.ShowDialog(); // Mở cửa sổ Nhóm (chặn tương tác các cửa sổ khác)
                this.Show(); // Hiện lại Dashboard khi Group đóng
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