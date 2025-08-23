using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 选择器
    /// 有一个子节点Success，就返回Success；都失败才失败
    /// </summary>
    public class SelectorNode : SequenceNode
    {
        // 可以直接继承顺序器

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override State OnUpdate()
        {
            if(children.IsNullOrEmpty())
            {
                return State.Failure;
            }

            while(currentChildIndex < children.Count)
            {
                var currentNode = children[currentChildIndex];
                var result = currentNode.Update();

                switch (result)
                {
                    case State.Running:
                        return State.Running;

                    case State.Failure:
                        currentChildIndex++;
                        break;

                    case State.Success:
                        return State.Success;
                }
            }

            return State.Failure;
        }
    }
}

