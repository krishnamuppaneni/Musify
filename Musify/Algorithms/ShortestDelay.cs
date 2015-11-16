using Musify.Models;
using SQLite.Net;
using SQLite.Net.Platform.WindowsPhone8;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musify.Algorithms
{
    public class ShortestDelay
    {

        private List<Device> _cloud = new List<Device>();
        private ReachableDeviceList _reachableNodes = new ReachableDeviceList();
        private List<Device> _Devices = new List<Device>();
        private List<Connection> _connections = new List<Connection>();
        private static List<Device> shortestRoute = new List<Device>();
        private SQLiteConnection db;
        private bool _isGraphConnected = true;

        public ShortestDelay()
        {
            db = new SQLiteConnection(new SQLitePlatformWP8(), DatabaseHelper.DB_PATH);
            FindMinDelayPath(db.FindWithChildren<Device>(1, recursive: true), db.FindWithChildren<Device>(5, recursive: true), 2);
        }

        private void FindMinDelayPath(Device start, Device end, int routeId)
        {
            shortestRoute.Clear();
            _cloud.Clear();
            _reachableNodes.Clear();
            _connections = db.Table<Connection>()
                .Where(c => c.RouteId == 2).ToList();
            Device currentNode = start;
            currentNode.Visited = true;
            start.TotalCost = 0;
            _cloud.Add(currentNode);
            ReachableDevice currentReachableNode;

            while (currentNode != end)
            {
                AddReachableNodes(currentNode);
                UpdateReachableNodesTotalCost(currentNode);

                //if we cannot reach any other node, the graph is not connected
                if (_reachableNodes.ReachableDevices.Count == 0)
                {
                    _isGraphConnected = false;
                    break;
                }

                //get the closest reachable node
                currentReachableNode = _reachableNodes.ReachableDevices[0];
                //remove if from the reachable nodes list
                _reachableNodes.RemoveReachableDevice(currentReachableNode);
                //mark the current node as visited
                currentNode.Visited = true;
                //set the current node to the closest one from the cloud
                currentNode = currentReachableNode.Device;
                //set a pointer to the edge from where we came from
                if (currentNode.Id == end.Id)
                {
                    end.ConnectionCameFrom = currentReachableNode.Connection;
                }
                currentNode.ConnectionCameFrom = currentReachableNode.Connection;
                //mark the edge as visited
                currentReachableNode.Connection.Visited = true;

                _cloud.Add(currentNode);
            }
            currentNode = end;
            while (currentNode.Id != start.Id && currentNode.Connections.Count != 0 && currentNode.ConnectionCameFrom != null)
            {
                currentNode.Visited = true;
                currentNode.ConnectionCameFrom.Visited = true;
                shortestRoute.Add(currentNode);
                currentNode = GetNeighbour(currentNode, currentNode.ConnectionCameFrom);
            }
            if (currentNode != null)
                shortestRoute.Add(currentNode);
        }

        private void AddReachableNodes(Device node)
        {
            Device neighbour;
            ReachableDevice rn;
            foreach (Connection edge in node.Connections)
            {
                neighbour = GetNeighbour(node, edge);
                //make sure we don't add the node we came from
                if (node.ConnectionCameFrom == null || neighbour != GetNeighbour(node, node.ConnectionCameFrom))
                {
                    //make sure we don't add a node already in the cloud
                    if (!_cloud.Contains(neighbour))
                    {
                        //if the node is already reachable
                        if (_reachableNodes.HasNode(neighbour))
                        {
                            //if the Delay from this edge is smaller than the current total cost
                            //amend the reachable node using the current edge
                            if (node.TotalCost + edge.Delay < neighbour.TotalCost)
                            {
                                rn = _reachableNodes.GetReachableNodeFromNode(neighbour);
                                rn.Connection = edge;
                            }
                        }
                        else
                        {
                            rn = new ReachableDevice(neighbour, edge);
                            _reachableNodes.AddReachableDevice(rn);
                        }
                    }
                }
            }
        }

        private Device GetNeighbour(Device node, Connection edge)
        {
            if (edge.FirstDevice == node)
                return edge.SecondDevice;
            else
                return edge.FirstDevice;
        }

        private void UpdateReachableNodesTotalCost(Device node)
        {
            double currentCost = node.TotalCost;
            foreach (ReachableDevice rn in _reachableNodes.ReachableDevices)
            {
                if (currentCost + rn.Connection.Delay < rn.Device.TotalCost || rn.Device.TotalCost == -1)
                    rn.Device.TotalCost = currentCost + rn.Connection.Delay;
            }
            _reachableNodes.SortReachableNodes();
        }
    }
}
