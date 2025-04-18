using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace SimpleFileManager.WPFApp.Converter;

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTime date = (DateTime)value;
        
        string result = date.ToString("yyyy/MM/dd HH:mm");
        //0001/01/01 00:00
        if ("0001/01/01 00:00" == result)
        {
            result = "";
        }
        return result;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}