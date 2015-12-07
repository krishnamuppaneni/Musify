using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Musify.Algorithms
{
    class KruskalsAlgorithm
    {
        public string output;

        public string Run()
        {
            Adjacency adjacency = new Adjacency(9);

            adjacency.setElementAt(true, 1, 2);
            adjacency.setElementAt(true, 2, 1);
            adjacency.setElementAt(true, 1, 3);
            adjacency.setElementAt(true, 3, 1);
            adjacency.setElementAt(true, 3, 4);
            adjacency.setElementAt(true, 4, 3);
            adjacency.setElementAt(true, 4, 5);
            adjacency.setElementAt(true, 5, 4);
            adjacency.setElementAt(true, 5, 2);
            adjacency.setElementAt(true, 2, 5);
            adjacency.setElementAt(true, 4, 2);
            adjacency.setElementAt(true, 2, 4);
            adjacency.setWeight(1, 2, 2);
            adjacency.setWeight(2, 1, 2);
            adjacency.setWeight(1, 3, 1);
            adjacency.setWeight(3, 1, 1);
            adjacency.setWeight(3, 4, 1);
            adjacency.setWeight(4, 3, 1);
            adjacency.setWeight(4, 5, 1);
            adjacency.setWeight(5, 4, 1);
            adjacency.setWeight(5, 2, 4);
            adjacency.setWeight(2, 5, 4);
            adjacency.setWeight(4, 2, 2);
            adjacency.setWeight(2, 4, 2);

            KruskalsAlgorithm mst = new KruskalsAlgorithm();
            Pair[] A = mst.MSTKruskal(9, adjacency);

            output += "The edges of the minimum spanning tree:\r\n";
            for (int i = 0; i < A.Length; i++)
                if (A[i] != null)
                    output += A[i].ToString() + "\r\n";
            output += "\r\n\n";
            return output;
        }

        int ALength;
        Edge[] edge;

        void quickSort(int p, int r)
        {
            int i = p, j = r, m = (i + j) / 2;
            Edge x = edge[m];

            do
            {
                while (edge[i].Weight < x.Weight)
                    i++;

                while (edge[j].Weight > x.Weight)
                    j--;

                if (i <= j)
                {
                    Edge temp = edge[i];

                    edge[i] = edge[j];
                    edge[j] = temp;
                    i++;
                    j--;
                }
            }
            while (i <= j);

            if (p < j)
                quickSort(p, j);

            if (i < r)
                quickSort(i, r);
        }

        public Pair[] MSTKruskal(int n, Adjacency adjacency)
        {
            bool uFound, vFound;
            int i, j, k, l, m, u, v;
            int ULength, count = 0;
            int[] U = new int[n];
            int[] SLength = new int[n];
            int[,] S = new int[n, n];
            Pair[] A = new Pair[n * n];

            ALength = 0;

            for (v = 0; v < n; v++)
            {
                SLength[v] = 1;
                S[v, 0] = v;
            }

            for (u = 0; u < n - 1; u++)
                for (v = u + 1; v < n; v++)
                    if (adjacency.getElementAt(u, v))
                        count++;

            edge = new Edge[count];

            for (i = 0; i < count; i++)
                edge[i] = new Edge();

            for (i = u = 0; u < n - 1; u++)
            {
                for (v = u + 1; v < n; v++)
                {
                    if (adjacency.getElementAt(u, v))
                    {
                        edge[i].U = u;
                        edge[i].V = v;
                        edge[i++].Weight = adjacency.getWeight(u, v);
                    }
                }
            }

            quickSort(0, count - 1);

            for (i = 0; i < count; i++)
            {
                int jIndex = -1, lIndex = -1;

                u = edge[i].U;
                v = edge[i].V;

                for (uFound = false, j = 0; !uFound && j < n; j++)
                {
                    for (k = 0; !uFound && k < SLength[j]; k++)
                    {
                        uFound = u == S[j, k];
                        if (uFound)
                            jIndex = j;
                    }
                }
                for (vFound = false, l = 0; !vFound && l < n; l++)
                {
                    for (m = 0; !vFound && m < SLength[l]; m++)
                    {
                        vFound = v == S[l, m];
                        if (vFound)
                            lIndex = l;
                    }
                }

                if (jIndex != lIndex)
                {
                    Pair pair = new Pair(u, v);

                    for (j = 0; j < ALength; j++)
                        if (A[j].Equals(pair))
                            break;
                    if (j == ALength)
                        A[ALength++] = pair;

                    ULength = SLength[jIndex];

                    for (u = 0; u < ULength; u++)
                        U[u] = S[jIndex, u];

                    for (u = 0; u < SLength[lIndex]; u++)
                    {
                        v = S[lIndex, u];

                        for (vFound = false, j = 0; j < ULength; j++)
                            vFound = v == U[j];

                        if (!vFound)
                            U[ULength++] = v;
                    }

                    SLength[jIndex] = ULength;

                    for (j = 0; j < ULength; j++)
                        S[jIndex, j] = U[j];
                    SLength[lIndex] = 0;
                }
            }

            return A;
        }
    }

    class Adjacency
    {
        bool[,] matrix;
        int n;
        int[,] weight;

        public Adjacency(int n)
        {
            this.n = n;
            matrix = new bool[n, n];
            weight = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = false;
                    weight[i, j] = 0;
                }
            }
        }

        public bool getElementAt(int i, int j)
        {
            return matrix[i, j];
        }

        public int getWeight(int i, int j)
        {
            return weight[i, j];
        }

        public void setElementAt(bool element, int i, int j)
        {
            matrix[i, j] = element;
        }

        public void setWeight(int i, int j, int weight)
        {
            this.weight[i, j] = weight;
        }
    }

    class Edge
    {
        int u, v, weight;

        public Edge()
        {
            u = v = weight = 0;
        }

        public Edge(int u, int v, int weight)
        {
            this.u = u;
            this.v = v;
            this.weight = weight;
        }

        public int U
        {
            get
            {
                return u;
            }
            set
            {
                u = value;
            }
        }

        public int V
        {
            get
            {
                return v;
            }
            set
            {
                v = value;
            }
        }

        public int Weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }
    }

    class Pair
    {
        int u, v;

        public Pair(int u, int v)
        {
            this.u = u;
            this.v = v;
        }

        public int U
        {
            get
            {
                return u;
            }
            set
            {
                u = value;
            }
        }

        public int V
        {
            get
            {
                return v;
            }
            set
            {
                v = value;
            }
        }

        public override string ToString()
        {
            return "(" + u.ToString() + ", " + v.ToString() + ")";
        }
    }
    //class KruskalsAlgorithm
    //{
    //    class Vertex
    //    {
    //        public Vertex(int index)
    //        {
    //            this.index = index;
    //        }
    //        public int index;

    //        public override bool Equals(object obj)
    //        {
    //            return index == ((Vertex)obj).index;
    //        }
    //        public override int GetHashCode()
    //        {
    //            return index;
    //        }

    //        public override string ToString()
    //        {
    //            return index.ToString();
    //        }
    //    }

    //    class Edge
    //    {
    //        public Edge(int weight, Vertex v1, Vertex v2)
    //        {
    //            this.weight = weight;
    //            this.v1 = v1;
    //            this.v2 = v2;
    //        }
    //        public int weight;
    //        public Vertex v1;
    //        public Vertex v2;

    //        public override bool Equals(object obj)
    //        {
    //            Edge e2 = (Edge)obj;
    //            return (this.v1.Equals(e2.v1) && this.v2.Equals(e2.v2)) ||
    //                (this.v2.Equals(e2.v1) && this.v1.Equals(e2.v2));
    //        }

    //        public override int GetHashCode()
    //        {
    //            return weight;
    //        }

    //        public override string ToString()
    //        {
    //            return "" + v1.index + "-" + v2.index;
    //        }

    //        public static int compareByWeight(Edge e1, Edge e2)
    //        {
    //            if (e1.weight < e2.weight)
    //                return -1;
    //            else if (e1.weight > e2.weight)
    //                return 1;
    //            else
    //                return 0;
    //        }
    //    }

    //    class Tree
    //    {
    //        public List<Vertex> vertices = new List<Vertex>();
    //        public List<Edge> edges = new List<Edge>();
    //        public static Tree Merge(Tree t1, Tree t2, Edge e)
    //        {
    //            Tree newTree = new Tree();
    //            newTree.vertices.AddRange(t1.vertices);
    //            newTree.vertices.AddRange(t2.vertices);
    //            newTree.edges.AddRange(t1.edges);
    //            newTree.edges.AddRange(t2.edges);
    //            newTree.edges.Add(e);
    //            return newTree;
    //        }
    //    }

    //    class Forest
    //    {
    //        public List<Tree> trees = new List<Tree>();
    //    }

    //    static List<Vertex> createVertices(int n)
    //    {
    //        List<Vertex> v = new List<Vertex>();
    //        for (int i = 0; i < n; i++)
    //        {
    //            v.Add(new Vertex(i));
    //        }
    //        return v;
    //    }

    //    static void solve()
    //    {
    //        string[] lines = System.IO.File.ReadAllLines(@"..\..\network.txt");
    //        //create a forest
    //        Forest F = new Forest();

    //        //create a list of vertices
    //        List<Vertex> vertices = createVertices(lines.Length);

    //        //each vertex is a tree in a forest
    //        foreach (Vertex v in vertices)
    //        {
    //            Tree t1 = new Tree();
    //            t1.vertices.Add(v);
    //            F.trees.Add(t1);
    //        }

    //        //create a list of all edges
    //        List<Edge> S = new List<Edge>();

    //        for (int i = 0; i < lines.Length; i++)
    //        {
    //            //find vertex to one end
    //            Vertex v1 = vertices.Find(x => x.index == i);

    //            string[] e = lines[i].Split(',');
    //            for (int j = 0; j < e.Length; j++)
    //            {
    //                if (e[j] != "-")
    //                {
    //                    //find vertex to the other end
    //                    Vertex v2 = vertices.Find(x => x.index == j);

    //                    //create the edge
    //                    Edge edge = new Edge(Convert.ToInt32(e[j]), v1, v2);

    //                    //try to find the edge in the list
    //                    Edge temp = S.Find(x => x.Equals(edge));

    //                    //if not found, add the add to the list
    //                    if (temp == null)
    //                        S.Add(edge);
    //                }
    //            }
    //        }

    //        //sort them by weight. smallest weight first
    //        S.Sort(Edge.compareByWeight);

    //        int sumBeforeMininized = S.Sum(x => x.weight);

    //        int count = 0;
    //        while (S.Count > 0)
    //        {
    //            //get the first edge from the list
    //            Edge e = S[count];
    //            //remove the edge
    //            S.Remove(e);

    //            //find the trees that contains the vertices in edge e
    //            Tree t1 = F.trees.Find(x => x.vertices.Find(y => y.Equals(e.v1)) != null);
    //            Tree t2 = F.trees.Find(x => x.vertices.Find(y => y.Equals(e.v2)) != null);

    //            //if the 2 trees found are the same, ignore
    //            if (t1 == t2)
    //                continue;

    //            //Merge the two tress together by creating a new tree
    //            //then adding all the vertices and edges including
    //            //edge e
    //            Tree tFinal = Tree.Merge(t1, t2, e);

    //            //remove the 2 trees from the forest and add the new merged tree
    //            F.trees.Remove(t1);
    //            F.trees.Remove(t2);
    //            F.trees.Add(tFinal);
    //        }

    //        int sumAfterMininized = F.trees[0].edges.Sum(x => x.weight);
    //        Console.WriteLine(sumBeforeMininized - sumAfterMininized);
    //    }

    //    static void Main(string[] args)
    //    {
    //        solve();
    //        Console.ReadLine();
    //    }
    //}
}
