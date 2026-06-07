using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class UpgradeDialog : FluentWindow
    {
        public UpgradeDialog()
        {
            InitializeComponent();

            if (DataContext is UpgradeViewModel vm)
            {
                vm.CloseRequested += (_, _) => this.Close();
            }
        }
    }
}
