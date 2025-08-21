using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 行为树修饰节点
    /// </summary>
    public abstract class BehaviourTreeDecoratorNode : BehaviourTreeNode
    {
        //只有一个子节点
        [ReadOnlyInInspector]
        public BehaviourTreeNode child;

        public override BehaviourTreeNode Clone()
        {
            BehaviourTreeDecoratorNode node = Instantiate(this) as BehaviourTreeDecoratorNode;
            node.child = child.Clone();
            return node;
        }
    }
}

