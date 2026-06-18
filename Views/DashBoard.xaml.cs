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

            // Xử lý mở các cửa sổ
            vm.ProfileRequested += (_, _) => { new ProfileWindow(vm.CurrentUser) { Owner = this }.ShowDialog(); };
            vm.SettingsRequested += (_, _) => { new ProfileWindow(vm.CurrentUser) { Owner = this }.ShowDialog(); };
            vm.UpgradeRequested += (_, _) => { new UpgradeDialog { Owner = this }.ShowDialog(); };
            vm.GroupRequested += (_, _) => { new Group { Owner = this }.ShowDialog(); };
            vm.StorageRequested += (_, _) => { new Storage { Owner = this }.ShowDialog(); };
        }
    }
}