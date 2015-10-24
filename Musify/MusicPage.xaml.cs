using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Musify.Models;
using Windows.Storage;
using Coding4Fun.Toolkit.Controls;
using System.Collections.ObjectModel;
using Windows.Networking.Sockets;
using Windows.Networking.Proximity;
using Microsoft.Phone.Tasks;
using Musify.Resources;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Musify
{
    public partial class MusicPage : PhoneApplicationPage
    {
        ObservableCollection<PeerAppInfo> _peerApps;    // A local copy of peer app information
        StreamSocket _socket;                           // The socket object used to communicate with a peer
        string _peerName = string.Empty;                // The name of the current peer


        // Error code constants
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // The Bluetooth radio is off
        const uint ERR_MISSING_CAPS = 0x80070005;       // A capability is missing from your WMAppManifest.xml
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // You are currently not advertising your presence using PeerFinder.Start()

        List<MusicInfo> music;

        public MusicPage()
        {
            InitializeComponent();
            SystemTray.SetProgressIndicator(this, new ProgressIndicator());
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (App.DisplayName == string.Empty)
            {
                InputPrompt inputPrompt = new InputPrompt();
                inputPrompt.Title = "Set display name";
                inputPrompt.Message = "Please set your display name";

                inputPrompt.Completed += (sender, o) =>
                {
                    App.DisplayName = o.Result;
                };
                inputPrompt.Show();
            }
            using (DatabaseContext db = new DatabaseContext(DatabaseContext.DBConnectionString))
            {
                music = db.MusicInfo.ToList();
                musicList.DataContext = music;
            }
            // Maintain a list of peers and bind that list to the UI
            _peerApps = new ObservableCollection<PeerAppInfo>();

            // Register for incoming connection requests
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;

            // Start advertising ourselves so that our peers can find us
            PeerFinder.DisplayName = App.DisplayName;
            PeerFinder.Start();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;

            // Cleanup before we leave
            CloseConnection(false);

            base.OnNavigatingFrom(e);
        }


        private async void musicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MusicInfo music = (MusicInfo)e.AddedItems[0];
            StorageFolder folder = KnownFolders.MusicLibrary;            
            if (music.Owner == App.DisplayName)
            {
                StorageFile file = await folder.GetFileAsync(music.Name);
                media.Source = new Uri(file.Path);
                media.Play();
            }
            else
            {
                RequestMusic(music);
            }
        }

        private async void RequestMusic(MusicInfo music)
        {
            try
            {
                StartProgress("finding peer...");
                var peers = await PeerFinder.FindAllPeersAsync();

                // By clearing the backing data, we are effectively clearing the ListBox
                _peerApps.Clear();

                if (peers.Count == 0)
                {
                    //tbPeerList.Text = AppResources.Msg_NoPeers;
                }
                else
                {
                    PeerInformation peer = peers.Where(p => p.DisplayName == music.Owner).FirstOrDefault();
                    if (peer != null)
                    {
                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            byte[] musicBytes = await SendMusicRequest(music, peer);
                            using (IsolatedStorageFileStream audio = new IsolatedStorageFileStream(music.Name, FileMode.Create, isf))
                            {
                                audio.Write(musicBytes, 0, (Int32)musicBytes.Length);
                                media.SetSource(audio);
                                media.Play();
                            }                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == ERR_BLUETOOTH_OFF)
                {
                    var result = MessageBox.Show(AppResources.Err_BluetoothOff, AppResources.Err_BluetoothOffCaption, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        ShowBluetoothControlPanel();
                    }
                }
                else if ((uint)ex.HResult == ERR_MISSING_CAPS)
                {
                    MessageBox.Show(AppResources.Err_MissingCaps);
                }
                else if ((uint)ex.HResult == ERR_NOT_ADVERTISING)
                {
                    MessageBox.Show(AppResources.Err_NotAdvertising);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                StopProgress();
            }
        }

        private async Task<byte[]> SendMusicRequest(MusicInfo music, PeerInformation peer)
        {
            //ConnectToPeer(peer);
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);

                _peerName = peer.DisplayName;
                //UpdateChatBox(AppResources.Msg_ChatStarted, true);

                // Since this is a chat, messages can be incoming and outgoing. 
                // Listen for incoming messages.
                //ListenForIncomingMessage();

            }
            catch (Exception ex)
            {
                // In this sample, we handle each exception by displaying it and
                // closing any outstanding connection. An exception can occur here if, for example, 
                // the connection was refused, the connection timeout etc.
                MessageBox.Show(ex.Message);
                CloseConnection(false);
            }
            if (_socket == null)
            {
                MessageBox.Show(AppResources.Err_NoPeerConnected, AppResources.Err_NoSendTitle, MessageBoxButton.OK);
                return null;
            }

            if (_dataWriter == null)
                _dataWriter = new DataWriter(_socket.OutputStream);

            // Each message is sent in two blocks.
            // The first is the size of the message.
            // The second is the message itself.
            _dataWriter.WriteInt32(music.Name.Length);
            await _dataWriter.StoreAsync();

            _dataWriter.WriteString(music.Name);
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();
            return await GetMusic();
        }

        private async Task<byte[]> GetMusic()
        {
            if (_dataReader == null)
                _dataReader = new DataReader(_socket.InputStream);
            // Each message is sent in two blocks.
            // The first is the size of the message.
            // The second if the message itself.
            //var len = await GetMessageSize();

            await _dataReader.LoadAsync(4);           
            uint messageLen = (uint)_dataReader.ReadInt32();         
            await _dataReader.LoadAsync(messageLen);
            byte[] bytes = new byte[messageLen];
            _dataReader.ReadBytes(bytes);          
           // SoundEffect sound;
           // sound = new SoundEffect(buffer.ToArray(), 44100, AudioChannels.Stereo);
           // FrameworkDispatcher.Update();
           // sound.Play();
            return bytes;
        }

        private async Task ConnectToPeer(PeerInformation peer)
        {
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);
                _peerName = peer.DisplayName;
                //UpdateChatBox(AppResources.Msg_ChatStarted, true);

                // Since this is a chat, messages can be incoming and outgoing. 
                // Listen for incoming messages.
                //ListenForIncomingMessage();
            }
            catch (Exception ex)
            {
                // In this sample, we handle each exception by displaying it and
                // closing any outstanding connection. An exception can occur here if, for example, 
                // the connection was refused, the connection timeout etc.
                MessageBox.Show(ex.Message);
                CloseConnection(false);
            }
        }

        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                this.Dispatcher.BeginInvoke(async () =>
                {
                    // Ask the user if they want to accept the incoming request.
                    var result = MessageBox.Show(String.Format(AppResources.Msg_ChatPrompt, args.PeerInformation.DisplayName)
                                                 , AppResources.Msg_ChatPromptTitle, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        await ConnectToPeer(args.PeerInformation);
                        ListenForIncomingMessage();
                    }
                    else
                    {
                        // Currently no method to tell the sender that the connection was rejected.
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                CloseConnection(true);
            }
        }

        private DataReader _dataReader;
        private async void ListenForIncomingMessage()
        {
            try
            {
                string music = await GetMusicRequest();
                SendMusic(music);

                // Add to chat
                //UpdateChatBox(message, true);

                // Start listening for the next message.
                //ListenForIncomingMessage();
            }
            catch (Exception)
            {
                UpdateChatBox(AppResources.Msg_ChatEnded, true);
                CloseConnection(true);
            }
        }

        private async void SendMusic(string music)
        {
            StorageFolder folder = KnownFolders.MusicLibrary;
            StorageFile file = await folder.GetFileAsync(music);
            if (_dataWriter == null)
                _dataWriter = new DataWriter(_socket.OutputStream);

            // Each message is sent in two blocks.
            // The first is the size of the message.
            // The second is the message itself.
            IRandomAccessStream randomAccessStream = await file.OpenReadAsync();
            Windows.Storage.Streams.Buffer myBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(randomAccessStream.Size));
            IBuffer buffer = await randomAccessStream.ReadAsync(myBuffer, myBuffer.Capacity, InputStreamOptions.None);
            _dataWriter.WriteInt32((int)buffer.Length);
            await _dataWriter.StoreAsync();
            _dataWriter.WriteBytes(buffer.ToArray());
            await _dataWriter.StoreAsync();
        }

        private void CloseConnection(bool continueAdvertise)
        {
            if (_dataReader != null)
            {
                _dataReader.Dispose();
                _dataReader = null;
            }

            if (_dataWriter != null)
            {
                _dataWriter.Dispose();
                _dataWriter = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (continueAdvertise)
            {
                // Since there is no connection, let's advertise ourselves again, so that peers can find us.
                PeerFinder.Start();
            }
            else
            {
                PeerFinder.Stop();
            }
        }

        private async Task<string> GetMusicRequest()
        {
            if (_dataReader == null)
                _dataReader = new DataReader(_socket.InputStream);

            // Each message is sent in two blocks.
            // The first is the size of the message.
            // The second if the message itself.
            //var len = await GetMessageSize();
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            return _dataReader.ReadString(messageLen);
        }

        private void ConnectToSelected_Tap_1(object sender, GestureEventArgs e)
        {
            //if (PeerList.SelectedItem == null)
            //{
            //    MessageBox.Show(AppResources.Err_NoPeer, AppResources.Err_NoConnectTitle, MessageBoxButton.OK);
            //    return;
            //}

            //// Connect to the selected peer.
            //PeerAppInfo pdi = PeerList.SelectedItem as PeerAppInfo;
            //PeerInformation peer = pdi.PeerInfo;

            //ConnectToPeer(peer);
        }

        DataWriter _dataWriter;
        private void SendMessage_Tap_1(object sender, GestureEventArgs e)
        {
            //SendMessage(txtMessage.Text);
        }

        private void UpdateChatBox(string message, bool isIncoming)
        {
            if (isIncoming)
            {
                message = (String.IsNullOrEmpty(_peerName)) ? String.Format(AppResources.Format_IncomingMessageNoName, message) : String.Format(AppResources.Format_IncomingMessageWithName, _peerName, message);
            }
            else
            {
                message = String.Format(AppResources.Format_OutgoingMessage, message);
            }

            this.Dispatcher.BeginInvoke(() =>
            {
                //tbChat.Text = message + tbChat.Text;
                //txtMessage.Text = (isIncoming) ? txtMessage.Text : string.Empty;
            });
        }


        private void StartProgress(string message)
        {
            SystemTray.ProgressIndicator.Text = message;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
        }

        private void StopProgress()
        {
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.IsVisible = false;
                SystemTray.ProgressIndicator.IsIndeterminate = false;
            }
        }

        private void ShowBluetoothControlPanel()
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            connectionSettingsTask.Show();
        }
    }

    /// <summary>
    ///  Class to hold all peer information
    /// </summary>
    public class PeerAppInfo
    {
        internal PeerAppInfo(PeerInformation peerInformation)
        {
            this.PeerInfo = peerInformation;
            this.DisplayName = this.PeerInfo.DisplayName;
        }

        public string DisplayName { get; private set; }
        public PeerInformation PeerInfo { get; private set; }
    }
}