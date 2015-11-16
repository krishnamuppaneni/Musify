using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musify.Models
{
    public class ReachableDeviceList
    {
        private List<ReachableDevice> _rdList;
        private List<Device> _device;
        private Dictionary<Device, ReachableDevice> _rdDictionary;

        public ReachableDeviceList()
        {
            _rdList = new List<ReachableDevice>();
            _device = new List<Device>();
            _rdDictionary = new Dictionary<Device, ReachableDevice>();
        }

        public void AddReachableDevice(ReachableDevice rd)
        {
            if (_device.Contains(rd.Device))
            {
                ReachableDevice oldRN = GetReachableNodeFromNode(rd.Device);
                if (rd.Connection.Delay < oldRN.Connection.Delay)
                    oldRN.Connection = rd.Connection;
            }
            else
            {
                _rdList.Add(rd);
                _device.Add(rd.Device);
                _rdDictionary.Add(rd.Device, rd);
            }
        }

        public virtual List<ReachableDevice> ReachableDevices
        {
            get { return this._rdList; }
        }

        public void RemoveReachableDevice(ReachableDevice rd)
        {
            _rdList.Remove(rd);
            _device.Remove(rd.Device);
        }

        public bool HasNode(Device d)
        {
            return _device.Contains(d);
        }

        public ReachableDevice GetReachableNodeFromNode(Device d)
        {
            if (_rdDictionary.ContainsKey(d))
            {
                return _rdDictionary[d];
            }
            else
            {
                return null;
            }
        }

        public void SortReachableNodes()
        {
            _rdList.Sort();
        }

        public void Clear()
        {
            this._rdList.Clear();
            this._device.Clear();
            this._rdDictionary.Clear();
        }
    }
}
