using System.Collections.Specialized;
using System.Windows;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class OcrWindow : FluentWindow
    {
        public OcrWindow(UserDto currentUser)
        {
            InitializeComponent();

            // Khởi tạo ViewModel và truyền User xuống
            var vm = new OcrViewModel(currentUser);
            DataContext = vm;

            // ĐĂNG KÝ SỰ KIỆN: Tự động cuộn xuống đáy khi có dòng Log mới
            vm.LogMessages.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Tìm ScrollViewer bên trong ListBox (LogListBox) và lệnh cho nó cuộn xuống
                        var border = System.Windows.Media.VisualTreeHelper.GetChild(LogListBox, 0) as System.Windows.Controls.Decorator;
                        var scrollViewer = border?.Child as System.Windows.Controls.ScrollViewer;
                        scrollViewer?.ScrollToBottom();
                    });
                }
            };
        }
    }
}