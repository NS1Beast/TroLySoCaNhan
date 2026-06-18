using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class UpgradeDialog : FluentWindow
    {
        public UpgradeDialog(UserDto currentUser)
        {
            InitializeComponent();
            var vm = new UpgradeViewModel(currentUser);
            this.DataContext = vm;

            vm.CloseRequested += (s, e) =>
            {
                this.DialogResult = true;
                this.Close();
            };
        }
    }
}