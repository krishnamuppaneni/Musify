using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Musify.Algorithms
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
                Device_DFS Krishna = new Device_DFS("Krishna");
                Device_DFS Lakshman = new Device_DFS("Lakshman");
                Device_DFS Aakash = new Device_DFS("Aakash");
                Krishna.DFS_AddDevice(Lakshman);
                Krishna.DFS_AddDevice(Aakash);

                Device_DFS Darshan = new Device_DFS("Darshan");
                Device_DFS Nandha = new Device_DFS("Nandha");

                Lakshman.DFS_AddDevice(Darshan);
                Lakshman.DFS_AddDevice(Nandha);

                return Krishna;
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

        string output;
        public string Run()
        {
            DepthFirstAlgorithm b = new DepthFirstAlgorithm();
            Device_DFS root = b.BuildFriendGraph();
            output += "\n\nDepth First Search";
            output += "\n\nTraverse\n";
            b.Traverse(root);

            output += "\nSearch\n";
            Device_DFS p = b.Search(root, "Nandha");
            output += p == null ? "Device_DFS not found" : "Device "+ p.name + " found";
            return output;
        }
    }
}
