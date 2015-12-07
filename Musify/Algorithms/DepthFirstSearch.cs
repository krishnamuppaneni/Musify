using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgosProject
{
    class DepthFirstSearch
    {
        public class Device_DFS
        {
            public Device_DFS(string name)
            {
                this.name = name;
            }

            public string name { get; set; }
            public List<Device_DFS> Friends
            {
                get
                {
                    return DFS_DeviceList;
                }
            }

            public void DFS_AddDevice(Device_DFS p)
            {
                DFS_DeviceList.Add(p);
            }

            List<Device_DFS> DFS_DeviceList = new List<Device_DFS>();

            public override string ToString()
            {
                return name;
            }
        }

        public class DepthFirstAlgorithm
        {
            public Device_DFS BuildFriendGraph()
            {
                Device_DFS Aaron = new Device_DFS("Aaron");
                Device_DFS Betty = new Device_DFS("Betty");
                Device_DFS Brian = new Device_DFS("Brian");
                Aaron.DFS_AddDevice(Betty);
                Aaron.DFS_AddDevice(Brian);

                Device_DFS Catherine = new Device_DFS("Catherine");
                Device_DFS Carson = new Device_DFS("Carson");
                Device_DFS Darian = new Device_DFS("Darian");
                Device_DFS Derek = new Device_DFS("Derek");
                Betty.DFS_AddDevice(Catherine);
                Betty.DFS_AddDevice(Darian);
                Brian.DFS_AddDevice(Carson);
                Brian.DFS_AddDevice(Derek);

                return Aaron;
            }

            public Device_DFS Search(Device_DFS root, string nameToSearchFor)
            {
                if (nameToSearchFor == root.name)
                    return root;

                Device_DFS deviceFound = null;
                for (int i = 0; i < root.Friends.Count; i++)
                {
                    deviceFound = Search(root.Friends[i], nameToSearchFor);
                    if (deviceFound != null)
                        break;
                }
                return deviceFound;
            }

            public void Traverse(Device_DFS root)
            {
                Console.WriteLine(root.name);
                for (int i = 0; i < root.Friends.Count; i++)
                {
                    Traverse(root.Friends[i]);
                }
            }
        }

        static void Main(string[] args)
        {
            DepthFirstAlgorithm b = new DepthFirstAlgorithm();
            Device_DFS root = b.BuildFriendGraph();
            Console.WriteLine("Traverse\n------");
            b.Traverse(root);

            Console.WriteLine("\nSearch\n------");
            Device_DFS p = b.Search(root, "Catherine");
            Console.WriteLine(p == null ? "Device_DFS not found" : p.name);
            //p = b.Search(root, "Alex");
            //Console.WriteLine(p == null ? "Device_DFS not found" : p.name);
            Console.ReadLine();
        }
    }
}
