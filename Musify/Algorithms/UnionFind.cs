using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Musify.Algorithms
{
    class UnionFind
    {
        public string output;

        public string Run()
        {
            try
            {
                DisjointSets sets = new DisjointSets(10);
                PrintElementSets(sets);
                sets.Union(sets.FindSet(5), sets.FindSet(3));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(1), sets.FindSet(3));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(6), sets.FindSet(7));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(8), sets.FindSet(9));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(6), sets.FindSet(9));
                PrintElementSets(sets);
                sets.AddElements(3);
                PrintElementSets(sets);
                sets.Union(sets.FindSet(11), sets.FindSet(12));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(9), sets.FindSet(10));
                PrintElementSets(sets);
                sets.Union(sets.FindSet(7), sets.FindSet(11));
                PrintElementSets(sets);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.StackTrace);
            }
            return output;
        }

        public void PrintElementSets(DisjointSets sets)
        {
            for (int i = 0; i < sets.ElementCount; ++i)
                output += sets.FindSet(i).ToString() + "  ";
            output += "\n";
        }
    }

    public class DisjointSets
    {
        /// Create an empty DisjointSets data structure
        public DisjointSets()
            : this(0)
        {
        }
        /// Create a DisjointSets data structure with a specified number of elements (with element id's from 0 to count-1)
        public DisjointSets(int count)
        {
            m_elementCount = 0;
            m_setCount = 0;
            m_nodes = new List<Node>();
            AddElements(count);
        }
        /// Find the set identifier that an element currently belongs to.
        /// Note: some internal data is modified for optimization even though this method is consant.
        public int FindSet(int elementId)
        {
            if (elementId >= m_elementCount)
                throw new ArgumentOutOfRangeException("elementId");

            Node curNode;
            // Find the root element that represents the set which `elementId` belongs to
            curNode = m_nodes[elementId];
            while (curNode.Parent != null)
                curNode = curNode.Parent;
            Node root = curNode;

            // Walk to the root, updating the parents of `elementId`. Make those elements the direct
            // children of `root`. This optimizes the tree for future FindSet invokations.
            curNode = m_nodes[elementId];
            while (curNode != root)
            {
                Node next = curNode.Parent;
                curNode.Parent = root;
                curNode = next;
            }
            return root.Index;
        }

        /// Combine two sets into one. All elements in those two sets will share the same set id that can be gotten using FindSet.
        public void Union(int setId1, int setId2)
        {
            if (setId1 >= m_elementCount)
                throw new ArgumentOutOfRangeException("setId1");
            if (setId2 >= m_elementCount)
                throw new ArgumentOutOfRangeException("setId2");

            if (setId1 == setId2)
                return; // already unioned

            Node set1 = m_nodes[setId1];
            Node set2 = m_nodes[setId2];

            // Determine which node representing a set has a higher rank. The node with the higher rank is
            // likely to have a bigger subtree so in order to better balance the tree representing the
            // union, the node with the higher rank is made the parent of the one with the lower rank and
            // not the other way around.
            if (set1.Rank > set2.Rank)
                set2.Parent = set1;
            else if (set1.Rank < set2.Rank)
                set1.Parent = set2;
            else // set1.Rank == set2.Rank
            {
                set2.Parent = set1;
                ++set1.Rank; // update rank
            }
            // Since two sets have fused into one, there is now one less set so update the set count.
            --m_setCount;
        }

        /// Add a specified number of elements to the DisjointSets data structure. The element id's of the new elements are numbered
        /// consequitively starting with the first never-before-used elementId.
        public void AddElements(int addCount)
        {
            if (addCount < 0)
                throw new ArgumentOutOfRangeException("addCount");

            // insert and initialize the specified number of element nodes to the end of the `m_nodes` array
            for (int i = m_elementCount; i < m_elementCount + addCount; ++i)
            {
                Node newNode = new Node();
                newNode.Parent = null;
                newNode.Index = i;
                newNode.Rank = 0;
                m_nodes.Add(newNode);
            }
            // update element and set counts
            m_elementCount += addCount;
            m_setCount += addCount;
        }
        /// Returns the number of elements currently in the DisjointSets data structure.
        public int ElementCount
        {
            get { return m_elementCount; }
        }
        /// Returns the number of sets currently in the DisjointSets data structure.
        public int SetCount
        {
            get { return m_setCount; }
        }
        /// Internal Node data structure used for representing an element.
        private class Node
        {
            public string deviceName;
            public string deviceId;
            //public string totalCost;

            /// This roughly represent the max height of the node in its subtree.
            public int Rank;
            /// The index of the element the node represents.
            public int Index;
            /// The parent node of the node.
            public Node Parent;
        }
        /// The number of elements currently in the DisjointSets data structure.
        private int m_elementCount;
        /// The number of sets currently in the DisjointSets data structure.
        private int m_setCount;
        /// The list of nodes representing the elements.
        private List<Node> m_nodes;
    }
}
