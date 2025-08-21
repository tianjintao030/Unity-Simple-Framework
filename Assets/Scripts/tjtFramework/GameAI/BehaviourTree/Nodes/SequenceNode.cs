using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 顺序器
    /// </summary>
    public class SequenceNode : BehaviourTreeCompositeNode
    {
        /// <summary>
        /// 当前运行子节点的索引
        /// </summary>
        protected int currentChildIndex;

        protected override void OnStart()
        {
            currentChildIndex = 0;
        }

        protected override void OnStop()
        {
            
        }

        // 依次执行childrenNode，有一个失败即失败，一个成功了就继续执行下一个，直到children都执行成功
        protected override State OnUpdate()
        {
            if(children.Count > currentChildIndex)
            {
                var currentNode = children[currentChildIndex];
                switch(currentNode.Update())
                {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        currentChildIndex++;
                        break;
                }
            }

            return currentChildIndex == children.Count ? State.Success : State.Running;
        }
    }
}

