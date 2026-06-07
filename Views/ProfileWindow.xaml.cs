using System.Windows;
using TroLySoCaNhan.Models;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class ProfileWindow : FluentWindow
    {
        public ProfileWindow() : this(new User { Id = "TL-GUEST", UserName = "guest", DisplayName = "Khách", Email = "", Plan = "Miễn phí" })
        {
        }

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
