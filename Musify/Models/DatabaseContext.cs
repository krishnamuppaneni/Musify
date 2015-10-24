using System.Data.Linq;

namespace Musify.Models
{
    public class DatabaseContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/Database.sdf";

        // Pass the connection string to the base class.
        public DatabaseContext(string connectionString)
            : base(connectionString)
        { }

        public Table<MusicInfo> MusicInfo;
        public Table<PeerInfo> PeerInfo;
    }
}
