using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TroLySoCaNhan.MVVM.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null;

            if (Invert)
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}