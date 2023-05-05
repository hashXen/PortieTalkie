using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
namespace PortyTalky
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Service> services = new ObservableCollection<Service>(){
            new Service("192.168.0.1", 27015, false),  // TODO: Delete these 
            new Service("192.168.0.1", 80, true)
        };
        ObservableCollection<string> comboBoxInputs = new ObservableCollection<string>();
        
        public MainWindow()
        {
            InitializeComponent();
            listView.DataContext = services;   // for databinding
            cbHostPort.ItemsSource = comboBoxInputs;
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        private void MenuExit_Click(object sender, RoutedEventArgs e) 
        {
            Environment.Exit(0);
        }
        private void MenuAdd_Click(object sender, RoutedEventArgs e)
        {
            services.Add(new Service("0.0.0.0", 80));
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedService = (Service)listView.SelectedItem;
            if (selectedService != null)     // maybe nothing is selected yet, so we need to check
            {
                MessageBox.Show($"Selected {selectedService.IP}:{selectedService.Port}");
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            bool? isTcp = tcpRadioButton.IsChecked;
            Service service;
            if (isTcp.HasValue) 
            {
                service = new Service(cbHostPort.Text, (bool)isTcp);
            }
            else
            {
                service = new Service(cbHostPort.Text);
            }
            if (!services.Contains(service))
            {
                services.Add(service);
                comboBoxInputs.Add(cbHostPort.Text);
            }
            
        }

        private void cbHostPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonAdd_Click(sender, e);
            }
        }
    }
}
