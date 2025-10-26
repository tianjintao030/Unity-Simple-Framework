using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 取反器
    /// </summary>
    public class InverterNode : BehaviourTreeDecoratorNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if(child == null)
            {
                return State.Failure;
            }

            var result = child.Update();
            switch(result)
            {
                case State.Success:
                    return State.Failure;
                case State.Failure:
                    return State.Success;
                case State.Running:
                    return State.Running;
                default:
                    return State.Invalid;
            }
        }
    }
}

