using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Tile
{
    public class PathFinder
    {
        public List<PositionBehavior> GetPath(PositionBehavior start, PositionBehavior target, List<PositionBehavior> visited)
        {
            if(start == target)
            {
                return null;
            }

            List<PositionBehavior> bestPath = null;

            foreach (var node in start.GetAdjacentNodes())
            {
                var currentPath = visited.ToList();
                if (target == node)
                {
                    currentPath.Add(node);
                    if (PathDistance(start,bestPath) > PathDistance(start,currentPath))
                    {
                        bestPath = currentPath;
                    }
                    return bestPath;
                }
                else
                {
                    if (!currentPath.Contains(node) && node != start && CloserNode(node,start,target))
                    {
                        currentPath.Add(node);
                        var path = GetPath(node, target, currentPath);
                        if(path != null && (PathDistance(start, bestPath) > PathDistance(start, path)))
                        {
                            bestPath = path;
                        }
                    }
                }
            }

            return bestPath;
        }

        public int GetPathhStaminaCost(List<PositionBehavior> path)
        {
            return path.Sum(p => p.GetNodeCost());
        }

        private float PathDistance(PositionBehavior start, List<PositionBehavior> path)
        {
            if(path == null)
            {
                return float.MaxValue;
            }

            var completedPath = new List<PositionBehavior>() { start }.Concat(path).ToList();
            var distance = 0f;

            for(int i=0; i< completedPath.Count()-1;i++) 
            {
                distance += NodeDistance(completedPath[i],completedPath[i + 1]);
            }
            return distance;
        }

        private bool CloserNode(PositionBehavior nodeA, PositionBehavior nodeB, PositionBehavior target)
        {
            return NodeDistance(nodeA, target) <= NodeDistance(nodeB, target);
        }

        private float NodeDistance(PositionBehavior node1, PositionBehavior node2)
        {
            return (node2.transform.position - node1.transform.position).magnitude;
        }
    }
}
