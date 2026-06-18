using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class Group : FluentWindow
    {
        public Group()
        {
            InitializeComponent();
        }

        public Group(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new GroupViewModel(currentUser);
        }
    }
}