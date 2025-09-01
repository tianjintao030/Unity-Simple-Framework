using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Navigation
{
    /// <summary>
    /// A星寻路地形网格
    /// </summary>
    public class AStarGridController : MonoBehaviour
    {
        [Header("网格大小")]
        public Vector2 gridWorldSize = new Vector2(20, 20);
        private int gridSizeX, gridSizeY;
        [Header("格子半径/半边长")]
        public float nodeRadius;
        [Header("障碍物层级")]
        public LayerMask[] unwalkableLayers;
        [Header("网格绘制开关")]
        public bool canDraw;

        /// <summary>
        /// 网格格子二维数组
        /// </summary>
        private AStarNode[,] grid;
        /// <summary>
        /// 格子直径/边长
        /// </summary>
        private float nodeDiameter;

        /// <summary>
        /// 寻路路径
        /// </summary>
        [ReadOnlyInInspector]
        public List<AStarNode> path;

        public void ReSetGrid()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            CreateGrid();
        }

        private void CreateGrid()
        {
            if(gridWorldSize.x <= 0 || gridWorldSize.y <= 0)
            {
                Debug.LogError("网格Size必须为正值");
                return;
            }

            grid = new AStarNode[gridSizeX, gridSizeY];
            // 获取网格左下角位置
            var gridBottomLeft = transform.position 
                            - Vector3.right * gridWorldSize.x / 2 
                            - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    var worldPoint = gridBottomLeft
                                        + Vector3.right * (x * nodeDiameter + nodeRadius)
                                        + Vector3.forward * (y * nodeDiameter + nodeRadius);

                    var walkable = true;
                    if(unwalkableLayers != null && unwalkableLayers.Length > 0)
                    {
                        // 物理检测该格子位置是否有unwalkableLayer来判断walkable
                        foreach (var layer in unwalkableLayers)
                        {
                            if(Physics.CheckSphere(worldPoint, nodeRadius, layer))
                            {
                                walkable = false;
                                break;
                            }
                        }
                    }

                    grid[x, y] = new AStarNode(walkable, worldPoint, x, y);
                }
            }
        }

        public AStarNode NodeFromWorldPosition(Vector3 worldPosition)
        {
            var percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
            var percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return grid[x, y];
        }

        /// <summary>
        /// 获取格子的8个相邻格子
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<AStarNode> GetNeighbours(AStarNode node)
        {
            List<AStarNode> neighbours = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    // 跳过自身
                    if (x == 0 && y == 0) 
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    // 格子位置是否在网格范围内
                    if (checkX >= 0 && checkX < gridSizeX &&
                        checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }
            return neighbours;
        }

        private void OnDrawGizmos()
        {
            if(!canDraw)
            {
                return;
            }

            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (grid != null)
            {
                foreach (var n in grid)
                {
                    Gizmos.color = n.walkable ? Color.green : Color.red;
                    if (path != null && path.Contains(n))
                        Gizmos.color = Color.blue;

                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter * 0.9f));
                }
            }
        }
    }
}

