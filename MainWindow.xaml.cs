using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace PortieTalkie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Service> services = new ObservableCollection<Service>(){
            //new Service("192.168.0.1", 27015, false),  // TODO: Delete these 
            //new Service("192.168.0.1", 80, true),
            //new Service("localhost", 1234),
            //new Service("Google.com", 80)
        };
        ObservableCollection<string> comboBoxInputs = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            const string servicesXmlName = "Services.xml";
            Loaded += (sender, e) =>
            {  // load the services from last time
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
                try {
                    using (TextReader reader = new StreamReader(servicesXmlName))
                    {
                        var servicesLoaded = serializer.Deserialize(reader) as ObservableCollection<Service>;
                        if (servicesLoaded != null)
                        {
                            services = servicesLoaded;
                            Dispatcher.Invoke(() =>
                            {
                                listView.DataContext = services;
                            });
                        }
                    }
                } 
                catch (FileNotFoundException)
                {
                    // leave services as is
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        listView.DataContext = services;
                    });
                }
            };
            Closing += (sender, e) =>
            {  // save the services
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
                using (TextWriter writer = new StreamWriter(servicesXmlName)) 
                { 
                    serializer.Serialize(writer, services); 
                }
            };
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
            try
            {
                var selectedService = (Service)listView.SelectedItem;
                var selectedListViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);
                if (selectedService is not null && selectedListViewItem.IsMouseOver)     // maybe nothing is selected yet, so we need to check
                {
                    TalkieWindow talkyWindow = new TalkieWindow(selectedService);
                    talkyWindow.Show();
                }
            }
            catch (IndexOutOfRangeException)
            {   // if nothing is selected yet (or the end of the list was selected and deleted in an instant
                // which is currently not possible), do nothing
                return;
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            bool? isTcp = tcpRadioButton.IsChecked;
            Service service;
            try
            {
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
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void cbHostPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonAdd_Click(sender, e);
            }
        }
        private void listViewItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItem is not null)
            {
                services.Remove((Service)listView.SelectedItem);
            }
        }

        private void listView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var selectedListViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);
                if (selectedListViewItem is not null)
                {
                    if (!selectedListViewItem.IsMouseOver)
                    {
                        e.Handled = true;
                    }
                }
                else  // no IndexOutOfRangeException thrown for the -1 index but selectedListViewItem is null.
                {     // ContainerFromIndex probably just returns null when the ListView is empty without checking index
                    e.Handled = true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // When nothing is selected, don't open the context menu either
                e.Handled = true;
            }
        }
    }
}
