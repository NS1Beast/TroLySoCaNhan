using System.Windows;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class ProfileWindow : FluentWindow
    {
        // Constructor rỗng để tránh lỗi:
        // 'ProfileWindow' does not contain a constructor that takes 0 arguments
        public ProfileWindow()
        {
            InitializeComponent();
        }

        // Nhận UserDto từ DashboardViewModel rồi chuyển sang Models.User
        public ProfileWindow(UserDto currentUser) : this(new User
        {
            Id = currentUser.Id,
            UserName = currentUser.UserName,
            DisplayName = currentUser.DisplayName,
            Email = currentUser.Email,
            Plan = currentUser.Plan
        })
        {
        }

        // Constructor chính dùng Models.User
        public ProfileWindow(User user)
        {
            InitializeComponent();
            DataContext = new ProfileViewModel(user);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}