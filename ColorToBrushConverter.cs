using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Clock;

[ValueConversion(typeof(Color), typeof(Brush))]
public class ColorToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type type, object parameter, CultureInfo culture)
        => value is not Color color ? Brushes.Red : new SolidColorBrush(color);

    public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}