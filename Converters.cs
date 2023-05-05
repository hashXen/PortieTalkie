using System.Globalization;
using System.Windows.Data;
using System;

namespace PortyTalky
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
    public class SubtractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return Binding.DoNothing;
            }

            double input = double.Parse(value.ToString());
            double subtractValue = double.Parse(parameter.ToString());

            return input - subtractValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}