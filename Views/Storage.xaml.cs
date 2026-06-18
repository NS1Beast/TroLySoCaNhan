using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class Storage : FluentWindow
    {
        // Constructor rỗng bắt buộc cho WPF Designer
        public Storage()
        {
            InitializeComponent();
        }

        // Constructor thật nhận thông tin User từ Dashboard truyền qua
        public Storage(UserDto currentUser)
        {
            InitializeComponent();
            // Nối giao diện với StorageViewModel thay vì DashboardViewModel
            this.DataContext = new StorageViewModel(currentUser);
        }
    }
}