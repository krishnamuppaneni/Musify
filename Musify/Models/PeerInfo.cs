using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
