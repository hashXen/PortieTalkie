using System;
using System.CodeDom;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PortyTalky
{
    public class Service
    {
        public Service(string ip, int port, bool isTcp = true) { IP = ip; Port = port; IsTCP = isTcp; }
        public Service(string ipPort, bool isTcp = true)
        {
            string[] splits = ipPort.Split(':'); 
            IP = splits[0];
            try
            {
                Port = Convert.ToInt32(splits[1]);
            }
            catch
            {
                MessageBox.Show("Syntax: [host]:[port]\nExample: 192.168.0.1:7777");
                return;
            }
            IsTCP = isTcp;
        }

        public string IP { get; set; }
        public int Port { get; set; }
        public bool IsTCP { get; set; } // true for TCP, false for UDP
        public static bool operator ==(Service s1, Service s2)
        {
            if (ReferenceEquals(s1, null) && ReferenceEquals(s2, null))
            {
                return true;
            }
            if (ReferenceEquals(s1, null) || ReferenceEquals(s2, null))
            {   // considering null Services as not equal
                return false;
            }
            return s1.IP == s2.IP && s1.Port == s2.Port && s1.IsTCP == s2.IsTCP;
        }

        public static bool operator !=(Service s1, Service s2)
        {
            return !(s1 == s2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Service s)
            {
                return this == s;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (IP, Port, IsTCP).GetHashCode();
        }
    }

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
