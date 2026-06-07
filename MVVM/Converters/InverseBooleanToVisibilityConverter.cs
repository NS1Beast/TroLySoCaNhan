using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TroLySoCaNhan.MVVM.Converters
{
    /// <summary>
    /// Đảo ngược BooleanToVisibility:
    /// true -> Collapsed, false -> Visible.
    /// Dùng để ẩn ProgressRing khi IsLoading = false.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            return false;
        }
    }
}
