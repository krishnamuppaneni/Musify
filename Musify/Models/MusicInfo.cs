using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Musify.Models
{
    [Table("MusicInfo")]
    public class MusicInfo
    {
        [PrimaryKey]      
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(typeof(Device))]
        public int OwnerId { get; set; }

        [OneToOne]
        public Device Owner { get; set; }
    }
}
