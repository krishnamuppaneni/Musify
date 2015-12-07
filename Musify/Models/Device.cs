using System.Collections.Generic;
using SQLiteNetExtensions.Attributes;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Extensions;
using SQLite.Net.Attributes;
using System;

namespace Musify.Models
{
    [Table("Device")]
    public class Device : IComparable<Device>
    {
        public Device()
        {
            Visited = false;

            Connections = new List<Connection>();
            TotalCost = -1;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string DisplayName { get; set; }

        [Ignore]
        public double TotalCost { get; set; }

        [Ignore]
        public Connection ConnectionCameFrom { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Connection> Connections { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MusicInfo> Musics { get; set; }

        [Ignore]
        public bool Visited { get; set; }

        public void Reset()
        {
            Visited = false;
            ConnectionCameFrom = null;
            TotalCost = -1;
        }

        public int CompareTo(Device device)
        {
            return this.TotalCost.CompareTo(device.TotalCost);
        }
    }
}
