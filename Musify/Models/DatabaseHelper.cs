using SQLite.Net;
using SQLite.Net.Platform.WindowsPhone8;
using System;
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
                if (!CheckFileExists("Database.sqlite").Result)
                {
                    //using (dbConn = new SQLiteConnection(new SQLitePlatformWP8(), DB_PATH))
                    //{
                    //    dbConn.CreateTable<Device>();
                    //    dbConn.CreateTable<Connection>();
                    //    dbConn.CreateTable<MusicInfo>();
                    //}
                    StorageFile databaseFile = await Package.Current.InstalledLocation.GetFileAsync("Database.sqlite");
                    await databaseFile.CopyAsync(ApplicationData.Current.LocalFolder);
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
