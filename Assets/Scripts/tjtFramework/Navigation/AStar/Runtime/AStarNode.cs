using UnityEngine;

namespace tjtFramework.Navigation
{
    /// <summary>
    /// A星寻路格子
    /// </summary>
    public class AStarNode
    {
        /// <summary>
        /// 是否可行走
        /// </summary>
        public bool walkable;

        public Vector3 worldPosition;

        // 在Grid中的坐标
        public int gridX;
        public int gridY;

        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;

        public AStarNode parent;

        public AStarNode(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }

}
