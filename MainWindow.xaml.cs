using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
        ObservableCollection<Service> services = new ObservableCollection<Service>(){};
        ObservableCollection<string> comboBoxInputs = new ObservableCollection<string>();
        List<TalkieWindow> talkieWindows = new List<TalkieWindow>();
        public MainWindow()
        {
            InitializeComponent();

            const string servicesXmlName = "Services.xml";
            Loaded += (sender, e) =>
            {  // load the services from last time
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
                ReadServices(servicesXmlName);
            };
            Closing += (sender, e) =>
            {  // save the services
                if (MessageBoxResult.OK == MessageBox.Show("Quit PortieTalkie?", "", MessageBoxButton.OKCancel, MessageBoxImage.Question))
                {
                    WriteServices(servicesXmlName);
                    foreach (var talkieWindow in talkieWindows)
                    {
                        talkieWindow.Close();
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            };
            cbHostPort.ItemsSource = comboBoxInputs;
        }


        ////////////////////////////////// ListView events //////////////////////////////////
        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var selectedService = (Service)listView.SelectedItem;
                var selectedListViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);
                if (selectedService is not null && selectedListViewItem.IsMouseOver)     // maybe nothing is selected yet, so we need to check
                {
                    TalkieWindow talkyWindow = new TalkieWindow(selectedService);
                    talkieWindows.Add(talkyWindow);
                    talkyWindow.Show();
                }
            }
            catch (IndexOutOfRangeException)
            {   // if nothing is selected yet (or the end of the list was selected and deleted in an instant
                // which is currently not possible), do nothing
                return;
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
        ////////////////////////////////// Menu click events //////////////////////////////////
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();    // Do not use Environment.Exit() so that the Closing event can trigger
        }
        ////////////////////////////////// Application Commands //////////////////////////////////
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Open");
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Save");
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("SaveAs");
        }

        ////////////////////////////////// Button click events //////////////////////////////////
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
        ////////////////////////////////// ComboBox events //////////////////////////////////
        private void cbHostPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonAdd_Click(sender, e);
            }
        }
        ////////////////////////////////// XML interactions //////////////////////////////////
        private void ReadServices(string XmlPathName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
            try
            {
                using (TextReader reader = new StreamReader(XmlPathName))
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
        }
        private void WriteServices(string XmlPathName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
            try
            {
                using (TextWriter writer = new StreamWriter(XmlPathName))
                {
                    serializer.Serialize(writer, services);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show($"Could not save list of services to {XmlPathName}:" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
