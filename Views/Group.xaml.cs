using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class Group : FluentWindow
    {
        public Group()
        {
            InitializeComponent();
            DataContext = new GroupViewModel();
        }
    }
}