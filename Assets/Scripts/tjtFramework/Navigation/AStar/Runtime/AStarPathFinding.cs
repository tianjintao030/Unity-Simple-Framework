using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.Navigation
{
    public static class AStarPathFinding
    {
        /// <summary>
        /// A星寻路，寻路结果会赋值给grid的path字段
        /// </summary>
        /// <param name="grid">寻路网格</param>
        /// <param name="start">起点</param>
        /// <param name="target">终点</param>
        public static void FindPath(AStarGridController grid, Vector3 start, Vector3 target)
        {
            var startNode = grid.NodeFromWorldPosition(start);
            var targetNode = grid.NodeFromWorldPosition(target);

            List<AStarNode> openSet = new();
            HashSet<AStarNode> closeSet = new();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet[0];
                // 找到openSet中寻路消耗最小的点
                foreach(var openNode in openSet)
                {
                    if(openNode.fCost < currentNode.fCost ||
                        openNode.fCost == currentNode.fCost && openNode.hCost < currentNode.hCost)
                    {
                        currentNode = openNode;
                    }
                }

                // 移到closeSet
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);

                // 找到终点时
                if(currentNode == targetNode)
                {
                    RetracePath(grid, startNode, targetNode);
                    return;
                }

                var neighbours = grid.GetNeighbours(currentNode);
                if(neighbours.IsNullOrEmpty())
                {
                    Debug.LogError($"{grid.gameObject.name}中存在格子无邻格的情况,请检查");
                    return;
                }

                foreach(var neighbour in neighbours)
                {
                    // 若为不可行走区域或者已检查过在closeSet中
                    if(!neighbour.walkable || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    var newGCost = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if(!openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newGCost;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        openSet.Add(neighbour);
                    }
                }
            }
        }

        /// <summary>
        /// 从终点回溯路径
        /// </summary>
        private static void RetracePath(AStarGridController grid, AStarNode startNode,AStarNode targetNode)
        {
            List<AStarNode> path = new();
            var currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            grid.path = path;
        }

        private static int GetDistance(AStarNode a,AStarNode b)
        {
            // 用切比雪夫距离Chebyshev Distance
            var dstX = Mathf.Abs(a.gridX - b.gridX);
            var dstY = Mathf.Abs(a.gridY - b.gridY);

            // 若x方向跨度更大，先斜向走dstY，再直走x
            // 若y方向跨度大同理
            if (dstX > dstY)
            {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            else
            {
                return 14 * dstX + 10 * (dstY - dstX);
            }
        }
    }
}

