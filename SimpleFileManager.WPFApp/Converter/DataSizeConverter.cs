using System.Globalization;
using System.Windows.Data;


namespace SimpleFileManager.WPFApp.Converter;

public class DateSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        long length = (long)value;
        long x = System.Convert.ToInt64(Math.Ceiling((double)length/1024));
        return length < 0 ? "" : string.Format("{0:#,0} KB", x);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}