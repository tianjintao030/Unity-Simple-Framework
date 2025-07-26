using System;

namespace tjtFramework.UI
{
    /// <summary>
    /// UI层级特性描述，用于指定UI层级
    /// </summary>
    public class UILayerAttribute : Attribute
    {
        public UIConst.UIPanelLayer layer { get; }

        /// <summary>
        /// 指定面板层级
        /// </summary>
        /// <param name="layer"></param>
        public UILayerAttribute(UIConst.UIPanelLayer layer)
        {
            this.layer = layer;
        }
    }
}


