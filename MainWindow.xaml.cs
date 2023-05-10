using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Collections.Specialized;

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
        const string servicesXmlName = "Services.xml";
        string currentXmlFile = servicesXmlName;
        bool newUntitledFile = false;      // for Save behavior
        bool madeChanges = false;          // ditto
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {  // load the services from last time
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Service>));
                ReadServices(servicesXmlName);
            };
            Closing += (sender, e) =>
            {  // save the services
                if (MessageBoxResult.OK == MessageBox.Show("Quit PortieTalkie? The current services will be saved to the default list for your next launch.", 
                    "", MessageBoxButton.OKCancel, MessageBoxImage.Question))
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

        private void Services_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    madeChanges = true;
                    updateTitle();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // User got a new file, don't need to do anything (for now)
                    break;
                default:
                    break;
            }
        }
        ////////////////////////////////// Helpers //////////////////////////////////
        private void updateTitle()
        {
            if (newUntitledFile)
            {
                Title = "PortieTalkie : UntitledList" + (madeChanges ? " *" : "");
            } 
            else
            {
                string xmlFileName = Path.GetFileName(currentXmlFile);
                Title = "PortieTalkie : " + xmlFileName + (madeChanges ? " *" : "");
                if (xmlFileName == "Services.xml")
                {
                    Title += "[Default list]";
                }
            }
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

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (madeChanges)
            {
                var result = MessageBox.Show("You will any changes you've made since your last save. Continue?",
                    "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            services.Clear();
            newUntitledFile = true;
            updateTitle();
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (madeChanges)
            {
                var result = MessageBox.Show("You will any changes you've made since your last save. Continue?",
                    "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Cancel) 
                {
                    return;
                }
            }
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                currentXmlFile = openFileDialog.FileName;
                ReadServices(currentXmlFile);
                updateTitle();
                // no need to toggle asterisk because the title is fresh
            }

        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!newUntitledFile)
            {
                WriteServices(currentXmlFile);
                madeChanges = false;
                updateTitle();
            } 
            else
            {
                SaveAs_Executed(sender, e);
            }
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            
            if (saveFileDialog.ShowDialog() == true)
            {
                currentXmlFile = saveFileDialog.FileName;
                WriteServices(currentXmlFile);
                newUntitledFile = false;
                madeChanges = false;
                updateTitle();
            }
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
            cbHostPort.Text = string.Empty;
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
                        services.CollectionChanged += Services_CollectionChanged;
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
