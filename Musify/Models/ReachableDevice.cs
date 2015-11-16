using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musify.Models
{
    public class ReachableDevice : IComparable<ReachableDevice>
    {
        private Device _n;
        private Connection _e;
        public ReachableDevice(Device n, Connection e)
        {
            this._n = n;
            this._e = e;
        }

        public double TotalCost
        {
            get { return _n.TotalCost; }
        }

        public Device Device
        {
            get { return this._n; }
            set { this._n = value; }
        }

        public Connection Connection
        {
            get { return this._e; }
            set { this._e = value; }
        }

        public int CompareTo(ReachableDevice rd)
        {
            return this.Device.TotalCost.CompareTo(rd.Device.TotalCost);
        }
    }
}
