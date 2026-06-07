using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TroLySoCaNhan.MVVM.Converters
{
    /// <summary>
    /// Chuyển chuỗi sang Visibility: rỗng/null = Collapsed, còn lại = Visible.
    /// Dùng cho placeholder/error message.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool hasText = !string.IsNullOrWhiteSpace(value as string);
            bool invert = parameter as string == "Inverted";
            if (invert) return hasText ? Visibility.Collapsed : Visibility.Visible;
            return hasText ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
