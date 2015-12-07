using SQLite.Net.Attributes;
using SQLite.Net.Platform.WindowsPhone8;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musify.Models
{
    [Table("Connection")]
    public class Connection : IComparable<Connection>
    {
        private double _delay;
        private Device _firstDevice;
        private Device _secondDevice;

        private bool visited;

        public Connection()
        {

        }

        public Connection(Device firstDevice, Device secondDevice)
        {
            this._firstDevice = firstDevice;
            this._secondDevice = secondDevice;

            visited = false;
        }

        public Connection(Device firstDevice, Device secondDevice, double delay)
            : this(firstDevice, secondDevice)
        {
            this._delay = delay;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(RouteTable))]
        public int RouteId { get; set; }

        [ManyToOne(foreignKey: "FirstDeviceId", CascadeOperations = CascadeOperation.All)]
        [Column("FirstDeviceId")]
        public Device FirstDevice
        {
            get { return this._firstDevice; }
            set { this._firstDevice = value; }
        }

        [ForeignKey(typeof(Device))]
        public int FirstDeviceId
        {
            get;
            set;
        }

        [ManyToOne(foreignKey: "SecondDeviceId", CascadeOperations = CascadeOperation.All)]
        [Column("SecondDeviceId")]
        public Device SecondDevice
        {
            get { return this._secondDevice; }
            set
            {
                this._secondDevice = value;
            }
        }

        [ForeignKey(typeof(Device))]
        public int SecondDeviceId
        {
            get; set;
        }

        [Column("Delay")]
        public double Delay
        {
            get { return this._delay; }
            set { this._delay = value; }
        }

        public void Reset()
        {
            visited = false;
        }

        public int CompareTo(Connection otherConnection)
        {
            return this.Delay.CompareTo(otherConnection.Delay);
        }

        [Ignore]
        public bool Visited
        {
            get { return visited; }
            set { this.visited = value; }
        }
    }
}
