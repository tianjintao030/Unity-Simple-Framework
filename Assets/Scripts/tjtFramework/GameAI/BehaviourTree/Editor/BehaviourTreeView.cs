using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using tjtFramework.GameAI.BehaviourTree;
using System;
using System.Linq;

public class BehaviourTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }

    private BehaviourTree tree;

    public Action<NodeView> OnNodeSelectd;

    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/tjtFramework/GameAI/BehaviourTree/Editor/BehaviourTreeEditor.uss");
        styleSheets.Add(styleSheet);
    }

    internal void PopulateView(BehaviourTree tree)
    {
        this.tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if(tree.rootNode == null)
        {
            tree.rootNode = tree.CreateNode(typeof(BehaviourTreeRootNode)) as BehaviourTreeRootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        // 创建NodeView
        tree.nodes.ForEach(n => CreateNodeView(n));

        // 创建Edge
        tree.nodes.ForEach(n =>
        {
            var children = tree.GetChildren(n);
            if (children.Count > 0)
            {
                children.ForEach(c =>
                {
                    var parentView = FindNodeViewByNode(n);
                    var childView = FindNodeViewByNode(c);
                    var edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            }
        });
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                var nodeView = elem as NodeView;
                if (nodeView != null)
                {
                    tree.DeleteNode(nodeView.node);
                }

                var edge = elem as Edge;
                if(edge != null)
                {
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    tree.RemoveChild(parentView.node, childView.node);
                }
            });
        }

        if(graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                var parentView = edge.output.node as NodeView;
                var childView = edge.input.node as NodeView;
                tree.AddChild(parentView.node, childView.node);
            });
        }

        return graphViewChange;
    }

    private NodeView FindNodeViewByNode(BehaviourTreeNode node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node
            ).ToList();
    }

    /// <summary>
    /// 创建右键菜单项
    /// </summary>
    /// <param name="evt"></param>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        // 检查 tree 是否为空
        if (tree == null)
        {
            evt.menu.AppendAction("Please open a BehaviourTree asset first", null);
            return;
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<BehaviourTreeActionNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<BehaviourTreeDecoratorNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<BehaviourTreeCompositeNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
    }

    private void CreateNode(System.Type type)
    {
        var node = tree.CreateNode(type);
        CreateNodeView(node);
    }

    private void CreateNodeView(BehaviourTreeNode node)
    {
        var nodeView = new NodeView(node);
        nodeView.OnNodeSelectd = OnNodeSelectd;
        AddElement(nodeView);
    }
}
