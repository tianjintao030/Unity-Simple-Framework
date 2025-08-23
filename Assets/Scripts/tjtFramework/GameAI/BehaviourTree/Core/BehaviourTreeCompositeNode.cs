using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 行为树组合节点
    /// </summary>
    public abstract class BehaviourTreeCompositeNode : BehaviourTreeNode
    {
        // 多个子节点
        // 用双向链表构建子节点列表
        [ReadOnlyInInspector]
        public List<BehaviourTreeNode> children = new();

        public override BehaviourTreeNode Clone()
        {
            BehaviourTreeCompositeNode node = Instantiate(this) as BehaviourTreeCompositeNode;
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }

        public virtual void ResetChildren()
        {
            if(!children.IsNullOrEmpty())
            {
                children.ForEach((c) => c.Reset());
            }
        }
    }
}

