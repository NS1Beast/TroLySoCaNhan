using System.Windows;
using System.Windows.Controls;

namespace TroLySoCaNhan.MVVM.Converters
{
    /// <summary>
    /// Cho phép bind PasswordBox.Password hai chiều mà KHÔNG vi phạm MVVM.
    /// Cách dùng:
    ///   &lt;ui:PasswordBox helpers:PasswordBoxHelper.BoundPassword="{Binding Password}" /&gt;
    ///   &lt;ui:PasswordBox helpers:PasswordBoxHelper.BoundPassword="{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" /&gt;
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox pb) return;
            // Tránh vòng lặp khi gán từ UI
            if (pb.Password == (e.NewValue as string)) return;
            pb.Password = e.NewValue as string ?? string.Empty;
        }
    }
}
