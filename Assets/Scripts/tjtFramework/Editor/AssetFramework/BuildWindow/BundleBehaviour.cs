using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEditor;
using UnityEngine;
using tjtFramework.AssetFramework;

public class BundleBehaviour
{
    /// <summary>
    /// 模块配置
    /// </summary>
    protected List<BuildModuleData> moduleDataList = new();
    /// <summary>
    /// 模块配置行列表
    /// </summary>
    protected List<List<BuildModuleData>> rowModuleDataList = new();

    protected virtual int oneRowCount { get;}

    public virtual void Init()
    {
        // 获取模块配置列表
        moduleDataList = BuildBundleConfig.Instance.AssetBundleConfig;

        for(int i = 0; i < moduleDataList.Count; i++)
        {
            //计算当前行索引
            var rowIndex = Mathf.FloorToInt(i / oneRowCount);

            if(rowModuleDataList.Count < rowIndex + 1)
            {
                //为当前行创建行数据列表
                rowModuleDataList.Add(new List<BuildModuleData>());
            }

            //给当前行列表添加模块配置
            rowModuleDataList[rowIndex].Add(moduleDataList[i]);
        }
    }

    [OnInspectorGUI]
    public virtual void OnGUI()
    {
        if (rowModuleDataList.IsNullOrEmpty() || rowModuleDataList.IsNullOrEmpty())
        {
            DrawAddModuleButton();
            return;
        }

        GUIContent content = EditorGUIUtility.IconContent("SceneAsset Icon".Trim(), "module");
        content.tooltip = "单机选中/取消\n双击打开配置";

        for(int i = 0;i < rowModuleDataList.Count;i++)
        {
            GUILayout.BeginHorizontal();

            for (int j = 0; j < rowModuleDataList[i].Count;j++)
            {
                var moduleData = rowModuleDataList[i][j];

                //绘制模块按钮
                if(GUILayout.Button(content, GUILayout.Width(110),GUILayout.Height(110)))
                {
                    moduleData.isBuild = !moduleData.isBuild;

                    if(Time.realtimeSinceStartup - moduleData.lastClickTime < 0.18f)
                    {
                        BundleModuleConfigWindow.ShowWindow(moduleData.moduleName);
                    }

                    moduleData.lastClickTime = Time.realtimeSinceStartup;
                }

                // 获取按钮Rect
                var buttonRect = GUILayoutUtility.GetLastRect();

                // 绘制name文本
                var labelRect = new Rect(
                    buttonRect.x,
                    buttonRect.yMax - 20,
                    buttonRect.width,
                    20
                );
                GUI.Label(labelRect, moduleData.moduleName, new GUIStyle { alignment = TextAnchor.MiddleCenter });

                // 绘制按钮选中效果
                if(moduleData.isBuild)
                {
                    // 绘制高亮
                    var highlightStyle = UnityEditorUtility.GetGUIStyle("LightmapEditorSelectedHighlight");
                    highlightStyle.contentOffset = Vector2.zero;
                    highlightStyle.alignment = TextAnchor.MiddleCenter;
                    var highlightRect = new Rect(
                        buttonRect.x,
                        buttonRect.y,
                        buttonRect.width - 8,
                        buttonRect.height - 8
                    );
                    GUI.Toggle(highlightRect, true, EditorGUIUtility.IconContent("Collab"), highlightStyle);

                    // 绘制对勾
                    var checkIcon = EditorGUIUtility.IconContent("d_FilterSelectedOnly");
                    float iconSize = 20f; 
                    var iconRect = new Rect(
                        buttonRect.xMax - iconSize,
                        buttonRect.y - 4,               
                        iconSize,
                        iconSize
                    );
                    GUI.Label(iconRect, checkIcon);
                }

            }

            // 在最后位置绘制添加模块按钮
            if (i == rowModuleDataList.Count - 1)
            {
                DrawAddModuleButton();
            }

            GUILayout.EndHorizontal();
        }

        // 没有模块数据时绘制一个添加模块按钮
        if(rowModuleDataList.IsNullOrEmpty())
        {
            DrawAddModuleButton();
        }

        GUILayout.Space(10);

        DrawDetailContent();

        GUILayout.Space(10);

        DrawBuildButtons();
    }

    public virtual void DrawAddModuleButton()
    {

    }

    public virtual void DrawBuildButtons()
    {

    }

    public virtual void DrawDetailContent()
    {

    }

    public virtual void BuildBundle()
    {

    }
}
