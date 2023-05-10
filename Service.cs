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
using System.Xml.Serialization;

namespace PortieTalkie
{
    public class Service
    {
        private const string portErrorMsg = "Port number out of range!\nPorts should be from 0 to 65535.";
        /// <summary>
        /// Default constructor is needed for the class to become serializable
        /// </summary>
        public Service()
        {
            Host = "";
            Port = 0;
            IsTCP = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="isTcp"></param>
        /// <exception cref="ArgumentException"></exception>
        public Service(string host, int port, bool isTcp = true)
        {
            Host = host;
            if (port < 0 || port > 65535)
            {
                throw new ArgumentException(portErrorMsg);
            }
            Port = port;
            IsTCP = isTcp;
        }
        public Service(string hostPort, bool isTcp = true)
        {
            string[] splits = hostPort.Split(':');
            Host = splits[0];
            IsTCP = isTcp;
            try
            {
                Port = Convert.ToInt32(splits[1]);
            }
            catch
            {
                throw new ArgumentException("Syntax: [host]:[port]\nExample: 192.168.0.1:7777");
            }
            if (Port < 0 || Port > 65535)
            {
                throw new ArgumentException(portErrorMsg);
            }
        }
        public string Host { get; set; }
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
            return s1.Host == s2.Host && s1.Port == s2.Port && s1.IsTCP == s2.IsTCP;
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
            return (Host, Port, IsTCP).GetHashCode();
        }
        /// <summary>
        /// Defines a way to convert a Service into a string.
        /// </summary>
        /// <returns>"[protocol]://host:host"</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(IsTCP ? "tcp" : "udp");
            sb.Append($"://{Host}:{Port}");
            return sb.ToString();
        }
    }
}
