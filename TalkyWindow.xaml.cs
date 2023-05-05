using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PortyTalky
{
    /// <summary>
    /// Interaction logic for TalkyWindow.xaml
    /// </summary>
    public partial class TalkyWindow : Window
    {
        TextBox? lastMsgTextBox;  // in case more data are still trickling in, needs to point to the last received message 
        TcpClient? tcpClient;
        UdpClient? udpClient;
        public TalkyWindow(Service service)
        {
            InitializeComponent();
            Title = service.ToString();
            
            // prep networking
            if (service.IsTCP)
            { 
                tcpClient = new TcpClient();
                addAnnouncement("Connecting...");
                try 
                { 
                    tcpClient.Connect(service.IP, service.Port);   // it's ok to block since window would be useless otherwise
                    var networkStream = tcpClient.GetStream();
                    addAnnouncement("Connected!");
                    Thread receiveThread = new Thread(() => {
                        while (true)
                        {
                            try
                            {
                                // READ
                                
                            }
                            catch
                            {
                                addAnnouncement("Connection interrupted.");
                                break;
                            }
                        }
                    });
                } 
                catch (Exception ex)
                {
                    addAnnouncement("Connection failed.");
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


            }
            else  // UDP
            {

            }
        }

        public void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            chatMessages.Children.Add(new TextBox() { Text="Haha"});
        }
        private void addMessage(string message, bool isReply = false)
        {
            TextBox textBox = new TextBox();
            // style the textbox here
            textBox.BorderBrush = null;
            if (isReply)
            {
                textBox.Text = message;
                lastMsgTextBox = textBox;
            }
            else
            {
                textBox.Text = message;
            }
            Dispatcher.Invoke(() =>    // this will make the function thread-safe to call (right?)
            {
                chatMessages.Children.Add(textBox);
            });
        }
        private void addAnnouncement(string announcement) 
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = announcement;
            Dispatcher.Invoke(() =>    // this will make the function thread-safe to call (right?)
            {
                chatMessages.Children.Add(textBlock);
            });
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (tcpClient is not null)
            {
                tcpClient.Close();
            }
            if (udpClient is not null)
            {
                udpClient.Close();
            }
        }
    }
}
