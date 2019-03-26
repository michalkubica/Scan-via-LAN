using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Scan_via_LAN_server
{
    public sealed partial class ServerPanel : Page
    {
        public ServerPanel() 
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(800, 400);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            StartServer();
        }

        private static string ServerPort = "1337";

        private async void StartServer()
        {
            try
            {
                var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

                // The ConnectionReceived event is raised when connections are received.
                serverDatagramSocket.MessageReceived += ServerDatagramSocket_MessageReceived;

                this.listbox_Logger.Items.Add("server is about to bind...");

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await serverDatagramSocket.BindServiceNameAsync(ServerPort);

                this.listbox_Logger.Items.Add(string.Format("server is bound to port number {0}", ServerPort));
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.listbox_Logger.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void ServerDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            string request;
            using (DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listbox_Logger.Items.Add(request));

            // Echo the request back as the response.
            using (Stream outputStream = (await sender.GetOutputStreamAsync(args.RemoteAddress, "1337")).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.listbox_Logger.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

            sender.Dispose();

            // await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.listbox_Logger.Items.Add("server closed its socket"));
        }
    }
}
