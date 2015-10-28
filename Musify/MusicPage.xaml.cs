using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Musify.Models;
using Musify.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Musify
{
    public partial class MusicPage : PhoneApplicationPage
    {
        StreamSocket _socket;                           // The socket object used to communicate with a peer
        StreamSocket _socket1;
        string _peerName = string.Empty;                // The name of the current peer


        // Error code constants
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // The Bluetooth radio is off
        const uint ERR_MISSING_CAPS = 0x80070005;       // A capability is missing from your WMAppManifest.xml
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // You are currently not advertising your presence using PeerFinder.Start()

        List<MusicInfo> music;
        DataWriter _dataWriter;
        DataReader _dataReader;
        DataWriter _dataWriter1;
        DataReader _dataReader1;

        public MusicPage()
        {
            InitializeComponent();
            SystemTray.SetProgressIndicator(this, new ProgressIndicator());
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (App.DisplayName == string.Empty)
            {
                InputScope inputScope = new InputScope();
                InputScopeName name = new InputScopeName() { NameValue = InputScopeNameValue.PersonalFullName };
                inputScope.Names.Add(name);

                InputPrompt inputPrompt = new InputPrompt();
                inputPrompt.Title = "Set display name";
                inputPrompt.Message = "Please set your display name";
                inputPrompt.InputScope = inputScope;
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
            if (musicList.SelectedItem == null)
                return;
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
            musicList.SelectedItem = null;
        }

        private async void RequestMusic(MusicInfo music)
        {
            try
            {
                StartProgress("finding peer...");
                var peers = await PeerFinder.FindAllPeersAsync();
                if (peers.Count == 0)
                {
                    ShowToastMessage("No peers are available to connect.");
                }
                else
                {
                    List<RouteTable> routes;
                    PeerInformation peer;
                    int routeId;
                    int sourceOrderId;
                    int destinationOrderId;
                    using (DatabaseContext db = new DatabaseContext(DatabaseContext.DBConnectionString))
                    {
                        routes = db.RouteTable.ToList();
                    }
                    routeId = routes.Where(r1 => r1.DisplayName == App.DisplayName && routes.Where(r2 => r2.DisplayName == music.Owner && r1.RouteId == r2.RouteId).Count() > 0)
                            .Select(s => s.RouteId).FirstOrDefault();
                    sourceOrderId = routes.Where(r => r.DisplayName == App.DisplayName && r.RouteId == routeId).FirstOrDefault().Order;
                    destinationOrderId = routes.Where(r => r.DisplayName == music.Owner && r.RouteId == routeId).FirstOrDefault().Order;

                    if (sourceOrderId > destinationOrderId)
                    {
                        peer = peers.Where(p => p.DisplayName == routes
                        .Where(r => r.Order == sourceOrderId - 1).FirstOrDefault().DisplayName)
                        .FirstOrDefault();
                    }
                    else
                    {
                        peer = peers.Where(p => p.DisplayName == routes
                           .Where(r => r.Order == sourceOrderId + 1).FirstOrDefault().DisplayName)
                           .FirstOrDefault();

                    }
                    if (peer != null)
                    {
                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            StartProgress("Getting music from " + _peerName + "...");
                            byte[] musicBytes = await SendMusicRequest(music, peer);
                            using (IsolatedStorageFileStream audio = new IsolatedStorageFileStream(music.Name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, isf))
                            {
                                audio.Write(musicBytes, 0, (Int32)musicBytes.Length);
                                media.SetSource(audio);
                                media.Play();
                                media.MediaEnded += Media_MediaEnded;
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
                else if ((uint)ex.HResult == 0x80072750)

                {
                    MessageBox.Show("The app on peer device is not running.");
                }
            }
            finally
            {
                StopProgress();
                CloseConnection(true);
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as MediaElement).Stop();

        }

        private async Task<byte[]> SendMusicRequest(MusicInfo music, PeerInformation peer)
        {
            await ConnectToPeer(peer);
            if (_socket == null)
            {
                MessageBox.Show(AppResources.Err_NoPeerConnected, AppResources.Err_NoSendTitle, MessageBoxButton.OK);
                return null;
            }
            _dataWriter = new DataWriter(_socket.OutputStream);
            _dataWriter.WriteInt32(music.Name.Length);
            await _dataWriter.StoreAsync();

            _dataWriter.WriteString(music.Name);
            await _dataWriter.StoreAsync();
            return await GetMusic();
        }

        private async Task<byte[]> GetMusic()
        {
            _dataReader = new DataReader(_socket.InputStream);
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            byte[] bytes = new byte[messageLen];
            _dataReader.ReadBytes(bytes);
            //CloseConnection(true);
            return bytes;
        }

        private async Task ConnectToPeer(PeerInformation peer)
        {
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);
                _peerName = peer.DisplayName;
            }
            catch (Exception)
            {
                CloseConnection(true);
            }
        }

        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                this.Dispatcher.BeginInvoke(async () =>
                {
                    // Ask the user if they want to accept the incoming request.
                    var result = MessageBox.Show(String.Format("Accept music request from {0}?", args.PeerInformation.DisplayName)
                                                 , "Music Rquest", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        await ConnectToPeer(args.PeerInformation);
                        ListenForMusicRequest();
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

        private async void ListenForMusicRequest()
        {
            StartProgress("Sending music to "+_peerName+"...");
            try
            {
                string music = await GetMusicRequest();
                SendMusic(music);
            }
            catch (Exception)
            {
                CloseConnection(true);
            }
        }

        private async Task<string> GetMusicRequest()
        {
            _dataReader = new DataReader(_socket.InputStream);
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            string song = _dataReader.ReadString(messageLen);
            return song;
        }

        private async void SendMusic(string music)
        {
            MusicInfo musicInfo;
            using (DatabaseContext db = new DatabaseContext(DatabaseContext.DBConnectionString))
            {
                musicInfo = db.MusicInfo.Where(m => m.Name == music).FirstOrDefault();
            }
            if (musicInfo.Owner == App.DisplayName)
            {
                StorageFolder folder = KnownFolders.MusicLibrary;
                StorageFile file = await folder.GetFileAsync(music);
                _dataWriter = new DataWriter(_socket.OutputStream);

                IRandomAccessStream randomAccessStream = await file.OpenReadAsync();
                Windows.Storage.Streams.Buffer myBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(randomAccessStream.Size));
                IBuffer buffer = await randomAccessStream.ReadAsync(myBuffer, myBuffer.Capacity, InputStreamOptions.None);
                _dataWriter.WriteInt32((int)buffer.Length);
                await _dataWriter.StoreAsync();
                _dataWriter.WriteBytes(buffer.ToArray());
                await _dataWriter.StoreAsync();
                StopProgress();
            }
            else
            {
                byte[] musicBytes = await RequestMusicForPeer(musicInfo);
                _dataWriter = new DataWriter(_socket.OutputStream);
                _dataWriter.WriteInt32(musicBytes.Length);
                await _dataWriter.StoreAsync();
                _dataWriter.WriteBytes(musicBytes);
                await _dataWriter.StoreAsync();
                StopProgress();
            }

        }

        private async Task<byte[]> RequestMusicForPeer(MusicInfo music)
        {
            try
            {
                StartProgress("finding peer...");
                var peers = await PeerFinder.FindAllPeersAsync();

                if (peers.Count == 0)
                {
                    ShowToastMessage("No peers are available to connect.");
                }
                else
                {
                    List<RouteTable> routes;
                    PeerInformation peer;
                    int routeId;
                    int sourceOrderId;
                    int destinationOrderId;
                    using (DatabaseContext db = new DatabaseContext(DatabaseContext.DBConnectionString))
                    {
                        routes = db.RouteTable.ToList();
                    }
                    routeId = routes.Where(r1 => r1.DisplayName == App.DisplayName && routes.Where(r2 => r2.DisplayName == music.Owner && r1.RouteId == r2.RouteId).Count() > 0)
                            .Select(s => s.RouteId).FirstOrDefault();
                    sourceOrderId = routes.Where(r => r.DisplayName == App.DisplayName && r.RouteId == routeId).FirstOrDefault().Order;
                    destinationOrderId = routes.Where(r => r.DisplayName == music.Owner && r.RouteId == routeId).FirstOrDefault().Order;

                    if (sourceOrderId > destinationOrderId)
                    {
                        peer = peers.Where(p => p.DisplayName == routes
                        .Where(r => r.Order == sourceOrderId - 1).FirstOrDefault().DisplayName)
                        .FirstOrDefault();
                    }
                    else
                    {
                        peer = peers.Where(p => p.DisplayName == routes
                           .Where(r => r.Order == sourceOrderId + 1).FirstOrDefault().DisplayName)
                           .FirstOrDefault();

                    }
                    StartProgress("Getting music from "+_peerName+"...");
                    byte[] musicBytes = await SendMusicRequestForPeer(music, peer);
                    return musicBytes;
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
                CloseConnection1();
            }
            return null;
        }

        private async Task<byte[]> SendMusicRequestForPeer(MusicInfo music, PeerInformation peer)
        {
            try
            {
                _socket1 = await PeerFinder.ConnectAsync(peer);
                _peerName = peer.DisplayName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                CloseConnection1();
            }
            if (_socket == null)
            {
                MessageBox.Show(AppResources.Err_NoPeerConnected, AppResources.Err_NoSendTitle, MessageBoxButton.OK);
                return null;
            }
            _dataWriter1 = new DataWriter(_socket1.OutputStream);
            _dataWriter1.WriteInt32(music.Name.Length);
            await _dataWriter1.StoreAsync();

            _dataWriter1.WriteString(music.Name);
            await _dataWriter1.StoreAsync();
            return await GetMusicforPeer();
        }

        private async Task<byte[]> GetMusicforPeer()
        {
            _dataReader1 = new DataReader(_socket1.InputStream);
            await _dataReader1.LoadAsync(4);
            uint messageLen = (uint)_dataReader1.ReadInt32();
            await _dataReader1.LoadAsync(messageLen);
            byte[] bytes = new byte[messageLen];
            _dataReader1.ReadBytes(bytes);
            CloseConnection1();
            return bytes;
        }

        private void CloseConnection1()
        {
            if (_dataReader1 != null)
            {
                _dataReader1.Dispose();
                _dataReader1 = null;
            }

            if (_dataWriter1 != null)
            {
                _dataWriter1.Dispose();
                _dataWriter1 = null;
            }

            if (_socket1 != null)
            {
                _socket1.Dispose();
                _socket1 = null;
            }
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

        private void ShowToastMessage(string message)
        {
            ToastPrompt toast = new ToastPrompt()
            {
                Title = message
            };
            toast.Show();
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