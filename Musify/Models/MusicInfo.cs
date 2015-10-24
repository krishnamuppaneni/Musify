using Microsoft.Phone.Data.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musify.Models
{
    [Table]
    public class MusicInfo
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL IDENTITY", CanBeNull = false, AutoSync = AutoSync.OnInsert)]      
        public int Id { get; set; }
        [Column]
        public string Name { get; set; }     
        [Column] 
        public string Owner { get; set; }
    }
}
