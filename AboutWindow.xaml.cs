using System.Reflection;
using System;
using System.Text;
using System.Windows;

namespace PortieTalkie
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            var attribute = Assembly.GetEntryAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute));
            StringBuilder sb = new StringBuilder();
            sb.Append($"Version: {version.Major}.{version.Minor}.{version.Build}\n");
            sb.Append($"{((AssemblyCopyrightAttribute)attribute).Copyright}");
            sb.Append("\n\n\n");
            sb.Append("WalkieTalkie is intended to be a lightweight tool for testing internet applications in a straightforward and visual manner, but it is a great tool for learning how certain servers behave as well!");
            sb.Append("\n\nPoke around and start sending packets like texts!");
            textBoxInfo.Text = sb.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
