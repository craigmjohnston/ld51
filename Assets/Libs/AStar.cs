// taken from: https://github.com/davecusatis/A-Star-Sharp/blob/master/Astar.cs
namespace AStarSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Node
    {
        // Change this depending on what the desired size is for each element in the grid
        public static int NODE_SIZE = 32;
        public Node Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + NODE_SIZE / 2, Position.Y + NODE_SIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        List<List<Node>> Grid;
        int GridRows
        {
            get
            {
               return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End)
        {
            Node start = new Node(new Vector2((int)(Start.X/Node.NODE_SIZE), (int) (Start.Y/Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.X / Node.NODE_SIZE), (int)(End.Y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            PriorityQueue<Node, float> OpenList = new PriorityQueue<Node,float>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Enqueue(start, start.F);

            while(OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList.Dequeue();
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach(Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        bool isFound = false;
                        foreach (var oLNode in OpenList.UnorderedItems)
                        {
                            if (oLNode == n)
                            {
                                isFound = true;
                            }
                        }
                        if (!isFound)
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Enqueue(n, n.F);
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if(!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null) ;
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if(row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if(row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if(col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if(col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }

    // probably super slow but .NET Core (or whatever I'm using) doesn't seem like it has PriorityQueue built in
    public class PriorityQueue<TValue, TKey>
    {
        private SortedDictionary<TKey, List<TValue>> sortedKeys = new ();
        private int count = 0;

        public int Count => this.count;

        public IEnumerable<TValue> UnorderedItems => this.sortedKeys.Values.SelectMany(v => v);

        public void Enqueue(TValue value, TKey key)
        {
            if (!this.sortedKeys.TryGetValue(key, out List<TValue> list))
            {
                list = new List<TValue>();
                this.sortedKeys.Add(key, list);
            }

            list.Add(value);
            count += 1;
        }

        public TValue Dequeue()
        {
            var kvp = this.sortedKeys.First();
            var list = kvp.Value;
            var output = list.First();
            if (list.Count == 1)
            {
                this.sortedKeys.Remove(kvp.Key);
            }
            else
            {
                list.RemoveAt(0);
            }

            count -= 1;

            return output;
        }
    }
}