using System;
using System.Globalization;
using System.Windows.Data;

namespace TroLySoCaNhan.MVVM.Converters
{
    /// <summary>
    /// So sánh giá trị binding với ConverterParameter (string).
    /// Dùng cho RadioButton.IsChecked: IsChecked="{Binding SelectedTheme, Converter={StaticResource StringEquals}, ConverterParameter=Light}".
    /// </summary>
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Khi user click vào RadioButton (value=true), set lại giá trị binding thành parameter.
            if (value is bool b && b && parameter is not null)
                return parameter.ToString() ?? string.Empty;
            return Binding.DoNothing;
        }
    }
}
