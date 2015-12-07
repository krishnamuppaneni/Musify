using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Musify.Models
{
    [Table("MusicInfo")]
    public class MusicInfo
    {
        [PrimaryKey, AutoIncrement]      
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(typeof(Device))]
        public int OwnerId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public Device Owner { get; set; }
    }
}
