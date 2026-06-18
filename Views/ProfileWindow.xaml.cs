using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class ProfileWindow : FluentWindow
    {
        // Constructor rỗng (Bắt buộc cho WPF Designer)
        public ProfileWindow()
        {
            InitializeComponent();
        }

        // Constructor nhận UserDto (Truyền thẳng vào ViewModel)
        public ProfileWindow(UserDto currentUser)
        {
            InitializeComponent();
            this.DataContext = new ProfileViewModel(currentUser);
        }
    }
}