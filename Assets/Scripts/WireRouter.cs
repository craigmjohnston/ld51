namespace Oatsbarley.LD51
{
    using System.Collections.Generic;
    using System.Linq;
    using AStarSharp;
    using UnityEngine;

    public class WireRouter : MonoBehaviour
    {
        private static WireRouter instance;
        public static WireRouter Instance => WireRouter.instance;

        private List<List<Node>> grid;

        private void Awake()
        {
            WireRouter.instance = this;

            this.grid = new List<List<Node>>();
            for (int i = 0; i < 1000; i++)
            {
                var sublist = new List<Node>();
                grid.Add(sublist);

                for (int j = 0; j < 1000; j++)
                {
                    sublist.Add(new Node(new System.Numerics.Vector2(i, j), true, 1f));
                }
            }
        }

        public Vector2[] Route(Vector2 from, Vector2 to)
        {
            var astar = new Astar(this.grid);
            var pathNodes = astar.FindPath(
                new System.Numerics.Vector2(from.x * 10, from.y * 10),
                new System.Numerics.Vector2(to.x * 10, to.y * 10));

            var output = pathNodes.Select(n => new Vector2(n.Position.X / 10f, n.Position.Y / 10f));

            return output.ToArray();
        }

        public void CommitRoute(Vector2[] route)
        {
            foreach (var position in route)
            {
                grid[(int)position.x * 10][(int)position.y * 10].Weight = 10f;
            }
        }
    }
}