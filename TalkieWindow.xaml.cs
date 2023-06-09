﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PortieTalkie
{
    /// <summary>
    /// Interaction logic for TalkieWindow.xaml
    /// </summary>
    public partial class TalkieWindow : Window
    {
        private TextBox? lastMsgTextBox;
        bool lastMsgIsReply = false;
        string removedNewLine = string.Empty;
        private TcpClient? tcpClient;
        private UdpClient? udpClient;
        private readonly Service service;
        private Mutex networkMutex = new Mutex();
        private NetworkStream? networkStream;
        public TalkieWindow(Service service)
        {
            InitializeComponent();
            this.service = service;
            Title = service.ToString();
            const int bufferSize = 4096;
            // prep networking when the window is loaded
            Loaded += async (sender, e) =>
            {
                if (service.IsTCP)
                {
                    tcpClient = new TcpClient();
                    addAnnouncement("Connecting...");

                    try
                    {   // Connect!
                        await tcpClient.ConnectAsync(service.Host, service.Port).WaitAsync(TimeSpan.FromSeconds(5)); // 5-second timeoout
                        networkStream = tcpClient.GetStream();

                        addAnnouncement("Connected!");
                        Dispatcher.Invoke(() =>
                        {
                            btnSend.IsEnabled = true;
                        });
                        while (true)
                        {
                            try
                            {
                                // Read
                                networkMutex.WaitOne();
                                byte[] buffer = new byte[bufferSize];
                                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferSize);
                                string messageStr = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                if (bytesRead == 0) // this only happens if remote closed connection gracefully
                                {
                                    addAnnouncement("Connection closed.");
                                    Dispatcher.Invoke(() =>
                                    {
                                        btnSend.IsEnabled = false;
                                        btnReconnect.Visibility = Visibility.Visible;
                                    });
                                    break;
                                }
                                addMessage(messageStr, true);
                            }
                            catch (Exception ex)
                            {
                                addAnnouncement("Connection interrupted.\nError: " + ex.Message, Colors.Red);
                                break;
                            }
                            finally
                            {
                                networkMutex.ReleaseMutex();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        addAnnouncement("Connection failed.");
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                }
                else
                {
                    udpClient = new UdpClient(0);
                    btnSend.IsEnabled = true;
                    while (true)    // Straight to the receive loop, no connection for UDP
                    {
                        try
                        {
                            networkMutex.WaitOne();
                            var result = await udpClient.ReceiveAsync();
                            addMessage(Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length));
                        }
                        catch (Exception ex)
                        {
                            addAnnouncement("Error while trying to receive UDP packet: " + ex.Message, Colors.Red);
                            break;
                        }
                        finally
                        {
                            networkMutex.ReleaseMutex();
                        }
                    }
                }
            };
        }
        public void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var inputStr = talkyInput.Text;
            if (checkBoxSendNewLine.IsChecked == true)  // == true is needed because of possible nullity 
            {
                inputStr += "\r\n";
            }
            var bytes = Encoding.UTF8.GetBytes(inputStr);
            networkMutex.WaitOne();
            Task.Run(async () =>
            {
                if (service.IsTCP && networkStream is not null)
                {
                    try
                    {
                        await networkStream.WriteAsync(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        addAnnouncement("Connection interrupted.\nError: " + ex.Message, Colors.Red);
                    }
                    finally
                    {
                        networkMutex.ReleaseMutex();
                    }
                }
                else if (!service.IsTCP)   // UDP
                {
                    if (udpClient is not null)
                    {
                        try
                        {
                            await udpClient.SendAsync(bytes, bytes.Length, service.Host, service.Port);
                        }
                        catch (Exception ex)
                        {
                            addAnnouncement("Connection interrupted.\nError: " + ex.Message, Colors.Red);
                        }
                        finally
                        {
                            networkMutex.ReleaseMutex();
                        }
                    }
                } // otherwise do nothing, this will only be reached if networkStream is null
            });
            addMessage(inputStr);
            Dispatcher.Invoke(() =>
            {
                talkyInput.Clear();
            });
        }


        private void talkyInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && checkBoxEnterToSend.IsChecked == true && btnSend.IsEnabled)
            {
                btnSend_Click(sender, e);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            tcpClient?.Close();
            udpClient?.Close();
        }

        private void checkBoxEnterToSend_Unchecked(object sender, RoutedEventArgs e)
        {
            talkyInput.AcceptsReturn = true;
        }

        private void checkBoxEnterToSend_Checked(object sender, RoutedEventArgs e)
        {
            talkyInput.AcceptsReturn = false;
        }
        /// <summary>
        /// Add a read-only TextBox containing the message to the main window. 
        /// If the message is a reply from the service and the user hasn't sent 
        /// anything since the last reply, addMessage will append the message 
        /// to the previous TextBox.
        /// </summary>
        /// <param name="message">Message to be added to the TalkieWindow.</param>
        /// <param name="isReply">true if the message is a reply from the service, false if the message is outgoing.</param>
        private void addMessage(string message, bool isReply = false)
        {
            TextBox textBox = lastMsgIsReply && lastMsgTextBox is not null ? lastMsgTextBox : new TextBox();
            // style the textbox here
            textBox.BorderBrush = null;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.IsReadOnly = true;

            // if we removed a new line before, add it back now
            if (lastMsgIsReply)
            {
                message = removedNewLine + message;
                removedNewLine = String.Empty;
            }

            if (isReply)
            {
                textBox.Foreground = Brushes.Green;
                lastMsgTextBox = textBox;
            }


            if (message.EndsWith("\r\n"))
            {
                message = message[..^2];  // get rid of the last new line
                if (isReply)
                {
                    removedNewLine = "\r\n";
                }
            }
            else if (message.EndsWith("\n"))
            {
                message = message[..^1];  // get rid of the last new line
                if (isReply)
                {
                    removedNewLine = "\n";
                }
            }

            textBox.Text += message;
            if (!lastMsgIsReply || !isReply)        // if this is not continued, or an outgoing message, add the textbox to the visual tree
            {
                Dispatcher.Invoke(() =>
                {
                    chatMessages.Children.Add(textBox);
                });
            }
            lastMsgIsReply = isReply;
        }
        /// <summary>
        /// Add a TextBlock containing the announcement to the TalkieWindow.
        /// The default color is Colors.Blue.
        /// </summary>
        /// <param name="announcement">The announcement to be made.</param>
        /// <param name="color">Use the Colors enum.</param>
        private void addAnnouncement(string announcement, Color color = default)
        {
            Dispatcher.Invoke(() =>    // this will make the function thread-safe to call
            {
                TextBlock textBlock = new TextBlock();
                if (color == default)
                {
                    color = Colors.Blue;
                }
                textBlock.Foreground = new SolidColorBrush(color);
                textBlock.Text = announcement;
                chatMessages.Children.Add(textBlock);
            });
        }

        private void btnReconnect_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(Window.LoadedEvent));    // Back to Loaded we go!
            btnReconnect.Visibility = Visibility.Hidden;                 // Gotta cover up the Reconnect button
            lastMsgIsReply = false;
        }
    }
}
