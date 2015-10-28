using System.Data.Linq.Mapping;

namespace Musify.Models
{
    [Table]
    public class PeerInfo
    {
        public int Order { get; set; }
        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public string DisplayName { get; set; }
    }
}
