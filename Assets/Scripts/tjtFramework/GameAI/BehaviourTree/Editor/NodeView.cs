using System.Collections;
using System.Collections.Generic;
using tjtFramework.GameAI.BehaviourTree;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public BehaviourTreeNode node;
    public Port input;
    public Port output;

    public Action<NodeView> OnNodeSelectd;

    public NodeView(BehaviourTreeNode node) 
    { 
        this.node = node;
        if(node is not BehaviourTreeRootNode)
        {
            this.title = node.name;
        }
        else
        {
            this.title = "RootNode";
        }
        
        this.viewDataKey = node.guid;

        style.left = node.position.x;
        style.top = node.position.y;

        CreateInputPorts();
        CreateOutputPorts();
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;
    }

    private void CreateInputPorts()
    { 
        // 根节点无输入
        if(node is BehaviourTreeRootNode)
        {
            return;
        }

        input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));

        if(input != null)
        {
            input.portName = "";
            inputContainer.Add(input);
        }
    }

    private void CreateOutputPorts()
    {
        // actionNode无需输出
        if(node is BehaviourTreeActionNode)
        {
            return;
        }
        if (node is BehaviourTreeRootNode)
        {
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
        }
        if (node is BehaviourTreeDecoratorNode)
        {
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
        }
        if (node is BehaviourTreeCompositeNode)
        {
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }

        if (output != null)
        {
            output.portName = "";
            outputContainer.Add(output);
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if(OnNodeSelectd != null)
        {
            OnNodeSelectd.Invoke(this);
        }
    }
}
