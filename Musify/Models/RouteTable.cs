using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace Musify.Models
{
    public class RouteTable
    {
        [PrimaryKey, AutoIncrement]
        public int RouteId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Connection> Connections { get; set; }
    }
}
