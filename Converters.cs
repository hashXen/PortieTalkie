using System.Globalization;
using System.Windows.Data;
using System;

namespace PortieTalkie
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTCP = (bool)value;
            return isTCP ? "TCP" : "UDP";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {   // so far not necessary, but implemented just in case
            string str = (string)value;
            return str == "TCP";
        }
    }
}