using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Tile
{
    public static class PathFinder
    {
        public static List<PositionBehavior> GetPath(PositionBehavior start, PositionBehavior target)
        {
            if (start == target)
            {
                return new List<PositionBehavior>();
            }
            return GetPath(start, target, new List<PositionBehavior>());
        }

        public static Dictionary<PositionBehavior, int> GetAllNodeWithinRange(int minRange, int maxRange, PositionBehavior start, IList<PositionBehavior> positions)
        {
            var result = new Dictionary<PositionBehavior, int>();

            var otherNodes = positions.ToList();

            foreach (var other in otherNodes)
            {
                var path = GetPath(start, other);
                if (path != null && path.Count >= minRange && path.Count <= maxRange)
                {
                    result.Add(other, path.Count);
                }
            }
            return result;
        }

        private static List<PositionBehavior> GetPath(PositionBehavior start, PositionBehavior target, List<PositionBehavior> visited)
        {
            List<PositionBehavior> bestPath = null;

            foreach (var node in start.GetAdjacentNodes())
            {
                var currentPath = visited.ToList();
                if (target == node)
                {
                    currentPath.Add(node);
                    if (PathDistance(start, bestPath) > PathDistance(start, currentPath))
                    {
                        bestPath = currentPath;
                    }
                    return bestPath;
                }
                else
                {
                    if (!currentPath.Contains(node) && node != start && CloserNode(node, start, target))
                    {
                        currentPath.Add(node);
                        var path = GetPath(node, target, currentPath);
                        if (path != null && (PathDistance(start, bestPath) > PathDistance(start, path)))
                        {
                            bestPath = path;
                        }
                    }
                }
            }

            return bestPath;
        }

        public static int GetPathStaminaCost(List<PositionBehavior> path, bool firstMovement, bool isFrozen)
        {
            var cost = 0;
            foreach(var node in path)
            {
                cost += node.GetNodeCost() - (firstMovement ? 1 : 0) + (isFrozen ? 1 : 0);
                firstMovement = false;
            }
            return cost;
        }

        private static float PathDistance(PositionBehavior start, List<PositionBehavior> path)
        {
            if (path == null)
            {
                return float.MaxValue;
            }

            var completedPath = new List<PositionBehavior>() { start }.Concat(path).ToList();
            var distance = 0f;

            for (int i = 0; i < completedPath.Count() - 1; i++)
            {
                distance += NodeWorldDistance(completedPath[i], completedPath[i + 1]);
            }
            return distance;
        }

        private static bool CloserNode(PositionBehavior nodeA, PositionBehavior nodeB, PositionBehavior target)
        {
            return NodeWorldDistance(nodeA, target) <= NodeWorldDistance(nodeB, target);
        }

        private static float NodeWorldDistance(PositionBehavior node1, PositionBehavior node2)
        {
            return (node2.transform.position - node1.transform.position).magnitude;
        }
    }
}
