using System.Collections;
using System.Collections.Generic;
using tjtFramework.GameAI.BehaviourTree;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEditor;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public BehaviourTreeNode node;
    public Port input;
    public Port output;
    private VisualElement inputPlaceholder;
    private VisualElement outputPlaceholder;

    public Action<NodeView> OnNodeSelectd;

    public NodeView(BehaviourTreeNode node)
    { 
        this.node = node;

        // 清除默认布局 (Node 自带的 inputContainer/outputContainer 我们不用)
        titleContainer.RemoveFromHierarchy();
        inputContainer.RemoveFromHierarchy();
        outputContainer.RemoveFromHierarchy();

        // 加载 UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/tjtFramework/GameAI/BehaviourTree/Editor/NodeView/NodeView.uxml");
        var nodeRoot = visualTree.CloneTree();

        // 找到 class="node-root" 的元素
        var rootElement = nodeRoot.Q<VisualElement>(className: "node-root");

        // 加载 USS
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/tjtFramework/GameAI/BehaviourTree/Editor/NodeView/NodeView.uss");
        rootElement.styleSheets.Add(styleSheet);

        // 把模板加到 mainContainer
        mainContainer.Add(rootElement);

        // 设置标题
        var titleLabel = rootElement.Q<Label>("title-label"); 
        if (titleLabel != null)
            titleLabel.text = node is BehaviourTreeRootNode ? "RootNode" : node.name;

        // 设置备注
        var descriptionLabel = rootElement.Q<Label>("desc-label");
        if (descriptionLabel != null)
            descriptionLabel.text = node.description;

        inputPlaceholder = rootElement.Q<VisualElement>("custom-input");
        outputPlaceholder = rootElement.Q<VisualElement>("custom-output");

        if (rootElement != null)
        {
            if (node is BehaviourTreeRootNode) rootElement.AddToClassList("root");
            else if (node is BehaviourTreeCompositeNode) rootElement.AddToClassList("composite");
            else if (node is BehaviourTreeActionNode) rootElement.AddToClassList("action");
            else if (node is BehaviourTreeDecoratorNode) rootElement.AddToClassList("decorator");
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
            inputPlaceholder.Add(input);
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
            outputPlaceholder.Add(output);
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
