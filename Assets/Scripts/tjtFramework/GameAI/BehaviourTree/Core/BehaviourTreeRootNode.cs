using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    public class BehaviourTreeRootNode : BehaviourTreeNode
    {
        [ReadOnlyInInspector]
        public BehaviourTreeNode child;

        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate()
        {
            return child.Update();
        }

        public override BehaviourTreeNode Clone()
        {
            BehaviourTreeRootNode rootNode = Instantiate(this) as BehaviourTreeRootNode;
            rootNode.child = child.Clone();
            return rootNode;
        }
    }
}

