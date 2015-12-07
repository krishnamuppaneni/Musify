using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgosProject
{
    class BreadthFirstSearch
    {
        public class BFS_Device
        {
            public string deviceName { get; set; }
            public string deviceId { get; set; }
            public string totalCost { get; set; }

            public BFS_Device(string deviceName, string deviceId, string totalCost)
            {
                this.deviceName = deviceName;
                this.deviceId = deviceId;
                this.totalCost = totalCost;
            }

            public List<BFS_Device> BFS_Children
            {
                get{return BFS_DeviceList;}
            }

            public void BFS_AddDevice(BFS_Device p)
            {
                BFS_DeviceList.Add(p);
            }

            List<BFS_Device> BFS_DeviceList = new List<BFS_Device>();

            public override string ToString()
            {
                return deviceName;
            }
        }

        public class BreadthFirstAlgorithm
        {
            public BFS_Device BFS_BuildDeviceGraph()
            {
                BFS_Device Darshan = new BFS_Device("Darshan","1","100");  //TO BE RETRIEVED FroM THE DB
                BFS_Device Krishna = new BFS_Device("Krishna", "2", "50");
                BFS_Device Lakshman = new BFS_Device("Lakshman", "3", "80");
                BFS_Device Nanda = new BFS_Device("Nanda", "4", "70");
                BFS_Device Aakash = new BFS_Device("Aakash", "5", "10");

                Krishna.BFS_AddDevice(Darshan);
                Krishna.BFS_AddDevice(Lakshman);
                Lakshman.BFS_AddDevice(Aakash);
                Lakshman.BFS_AddDevice(Nanda);

                return Krishna;
            }

            public BFS_Device Search(BFS_Device root, string nameToSearchFor)
            {
                Queue<BFS_Device> Q = new Queue<BFS_Device>();
                HashSet<BFS_Device> S = new HashSet<BFS_Device>();
                Q.Enqueue(root);
                S.Add(root);
                while (Q.Count > 0)
                {
                    BFS_Device p = Q.Dequeue();
                    if (p.deviceName == nameToSearchFor)
                        return p;
                    foreach (BFS_Device childDevice in p.BFS_Children)
                    {
                        if (!S.Contains(childDevice))
                        {
                            Q.Enqueue(childDevice);
                            S.Add(childDevice);
                        }
                    }
                }
                return null;
            }

            public void Traverse(BFS_Device root)
            {
                Queue<BFS_Device> traverseOrder = new Queue<BFS_Device>();
                Queue<BFS_Device> Q = new Queue<BFS_Device>();
                HashSet<BFS_Device> S = new HashSet<BFS_Device>();
                Q.Enqueue(root);
                S.Add(root);
                while (Q.Count > 0)
                {
                    BFS_Device p = Q.Dequeue();
                    traverseOrder.Enqueue(p);
                    foreach (BFS_Device childDevice in p.BFS_Children)
                    {
                        if (!S.Contains(childDevice))
                        {
                            Q.Enqueue(childDevice);
                            S.Add(childDevice);
                        }
                    }
                }
                while (traverseOrder.Count > 0)
                {
                    BFS_Device p = traverseOrder.Dequeue();
                    Console.WriteLine(p);
                }
            }
        }

        static void Main(string[] args)
        {
            BreadthFirstAlgorithm BFS = new BreadthFirstAlgorithm();
            BFS_Device root = BFS.BFS_BuildDeviceGraph(); //Get Graph and Root
            Console.WriteLine("Traverse\n------");
            BFS.Traverse(root);
            Console.WriteLine("\nSearch\n------");
            BFS_Device p = BFS.Search(root, "Darshan");
            Console.WriteLine(p == null ? "Device not found" : p.deviceName);
            //p = BFS.Search(root, "Derek");
            //Console.WriteLine(p == null ? "Device not found" : p.deviceName);

            Console.ReadLine();
        }
    }
}
