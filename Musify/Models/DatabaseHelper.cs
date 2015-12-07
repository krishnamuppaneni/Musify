using SQLite.Net;
using SQLite.Net.Platform.WindowsPhone8;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Musify.Models
{
    public class DatabaseHelper
    {
        SQLiteConnection dbConn;
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Database.sqlite"));

        public async Task<bool> CreateDatabase()
        {
            try
            {
                if (!await CheckFileExists("Database.sqlite"))
                {
                    using (dbConn = new SQLiteConnection(new SQLitePlatformWP8(), DB_PATH))
                    {
                        dbConn.CreateTable<Device>();
                        dbConn.CreateTable<Connection>();
                        dbConn.CreateTable<RouteTable>();
                        dbConn.CreateTable<MusicInfo>();
                        List<MusicInfo> music = new List<MusicInfo>()
                        {
                             new MusicInfo() { Name="Hallowen.mp3", OwnerId=1 },
                             new MusicInfo() { Name="Mouret.mp3", OwnerId=2 },
                             new MusicInfo() { Name="Mozart.mp3", OwnerId=3 },
                             new MusicInfo() { Name="Fireworks.mp3", OwnerId=4 },
                             new MusicInfo() { Name="Vivaldi.mp3", OwnerId=5 }
                        };
                        List<Device> devices = new List<Device>()
                        {
                            new Device() { Id=1, DisplayName="Krishna"},
                            new Device() { Id=2, DisplayName="Lakshman" },
                            new Device() { Id=3, DisplayName="Aakash" },
                            new Device() { Id=4, DisplayName="Darshan" },
                            new Device() { Id=5, DisplayName="Nandha" }
                        };

                        List<Connection> connections1 = new List<Connection>()
                        {
                             new Connection() { FirstDeviceId=1, SecondDeviceId=2, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=1, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=3, Delay=4 },
                             new Connection() { FirstDeviceId=3, SecondDeviceId=2, Delay=4 },
                             new Connection() { FirstDeviceId=1, SecondDeviceId=3, Delay=1 },
                             new Connection() { FirstDeviceId=3, SecondDeviceId=1, Delay=1 },
                             new Connection() { FirstDeviceId=3, SecondDeviceId=4, Delay=1 },
                             new Connection() { FirstDeviceId=4, SecondDeviceId=3, Delay=1 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=4, Delay=2 },
                             new Connection() { FirstDeviceId=4, SecondDeviceId=2, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=5, Delay=4 },
                             new Connection() { FirstDeviceId=5, SecondDeviceId=2, Delay=4 },
                             new Connection() { FirstDeviceId=4, SecondDeviceId=5, Delay=1 },
                             new Connection() { FirstDeviceId=5, SecondDeviceId=4, Delay=1 },
                        };
                        List<Connection> connections2 = new List<Connection>()
                        {
                             new Connection() { FirstDeviceId=1, SecondDeviceId=2, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=1, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=4, Delay=2 },
                             new Connection() { FirstDeviceId=4, SecondDeviceId=2, Delay=2 },
                             new Connection() { FirstDeviceId=2, SecondDeviceId=5, Delay=4 },
                             new Connection() { FirstDeviceId=5, SecondDeviceId=2, Delay=4 },
                             new Connection() { FirstDeviceId=4, SecondDeviceId=5, Delay=1 },
                             new Connection() { FirstDeviceId=5, SecondDeviceId=4, Delay=1 },
                        };
                        List<RouteTable> routes = new List<RouteTable>()
                        {
                             new RouteTable() {
                                  Connections = connections1,
                                  StartTime = DateTime.ParseExact("8:00 AM" ,"h:mm tt",CultureInfo.InvariantCulture),
                                  EndTime = DateTime.ParseExact("11:59 AM" ,"h:mm tt",CultureInfo.InvariantCulture) },
                              new RouteTable()
                              {
                                  Connections = connections2,
                                  StartTime = DateTime.ParseExact("12:00 PM" ,"h:mm tt",CultureInfo.InvariantCulture),
                                  EndTime = DateTime.ParseExact("3:59 PM" ,"h:mm tt",CultureInfo.InvariantCulture) },
                        };
                        dbConn.InsertAllWithChildren(devices);
                        dbConn.InsertAllWithChildren(routes);
                        dbConn.InsertAll(music);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                var store = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
