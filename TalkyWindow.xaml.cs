﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PortyTalky
{
    /// <summary>
    /// Interaction logic for TalkyWindow.xaml
    /// </summary>
    public partial class TalkyWindow : Window
    {
        private TextBox? lastMsgTextBox;  // in case more data are still trickling in, needs to point to the last received message 
        private TcpClient? tcpClient;
        private UdpClient? udpClient;
        private Service service;
        private Mutex networkMutex = new Mutex();
        private NetworkStream? networkStream;
        public TalkyWindow(Service service)
        {
            InitializeComponent();
            this.service = service;
            Title = service.ToString();

            // prep networking when loaded
            Loaded += async (sender, e) =>
            {
                if (service.IsTCP)
                {
                    tcpClient = new TcpClient();
                    addAnnouncement("Connecting...");
                    try
                    {
                        await tcpClient.ConnectAsync(service.IP, service.Port);   // can't be blocking, need to fix this
                        networkStream = tcpClient.GetStream();
                        addAnnouncement("Connected!");
                        while (true)
                        {
                            try
                            {
                                // READ
                                networkMutex.WaitOne();
                                byte[] buffer = new byte[4096];
                                var bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                networkMutex.ReleaseMutex();
                                addMessage(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                            }
                            catch (Exception ex)
                            {
                                addAnnouncement("Connection interrupted.\nError: " + ex.Message);
                                break;
                            }
                        }
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
            };
        }

        public void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (service.IsTCP && networkStream is not null)
            {
                networkMutex.WaitOne();
                var inputStr = talkyInput.Text;
                var bytes = Encoding.UTF8.GetBytes(inputStr);

                Task.Run(async () =>
                {
                    await networkStream.WriteAsync(bytes, 0, bytes.Length);
                    networkMutex.ReleaseMutex();
                });
                addMessage(inputStr);
                talkyInput.Clear();
            }
            else if (!service.IsTCP)   // UDP
            {

            } // otherwise do nothing, this will only be reached if networkStream is null
        }
        private void addMessage(string message, bool isReply = false)
        {
            TextBox textBox = new TextBox();
            // style the textbox here
            textBox.BorderBrush = null;
            if (message.EndsWith("\r\n"))
            {
                message = message[..^2];  // get rid of the last new line
            }
            else if (message.EndsWith("\n"))
            {
                message = message[..^1];  // get rid of the last new line
            }
            if (isReply)
            {
                textBox.Text = message;
                lastMsgTextBox = textBox;
            }
            else
            {
                textBox.Text = message;
            }
            chatMessages.Children.Add(textBox);
        }
        private void addAnnouncement(string announcement)
        {
            Dispatcher.Invoke(() =>    // this will make the function thread-safe to call (right?)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                textBlock.Text = announcement;
                chatMessages.Children.Add(textBlock);
            });
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            tcpClient?.Close();
            udpClient?.Close();
        }
    }
}
