using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundleWindow : BundleBehaviour
{

    protected override int oneRowCount { get => 5; }

    public override void Init()
    {
        base.Init();
    }

    public override void DrawAddModuleButton()
    {
        base.DrawAddModuleButton();

        var addButtonContent = EditorGUIUtility.IconContent("CollabCreate Icon", "");
        if (GUILayout.Button(addButtonContent, GUILayout.Width(110), GUILayout.Height(110)))
        {
            BundleModuleConfigWindow.ShowWindow("newModule");
        }
    }

    private string[] buildButtonsNames = new string[]{"打包资源","内嵌资源"};

    public override void DrawBuildButtons()
    {
        base.DrawBuildButtons();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace(); 

        for (int i = 0; i < buildButtonsNames.Length; i++)
        {
            if (GUILayout.Button(buildButtonsNames[i], GUILayout.Width(120), GUILayout.Height(32)))
            {
                if (buildButtonsNames[i] == "打包资源")
                {
                    BuildBundle();
                }
                if (buildButtonsNames[i] == "内嵌资源")
                {
                    CopyBundleToStreamingAssetsPath();
                }
            }

            GUILayout.Space(10);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public override void BuildBundle()
    {
        base.BuildBundle();

        if(moduleDataList.IsNullOrEmpty())
        {
            return;
        }

        foreach(var moduleData in moduleDataList)
        {
            if(moduleData.isBuild)
            {
                BuildBundleCompiler.BuildAssetBundle(moduleData, BuildType.AssetBundle, "null", "null");
            }
        }
    }

    public void CopyBundleToStreamingAssetsPath()
    {
        if (moduleDataList.IsNullOrEmpty())
        {
            return;
        }

        foreach (var moduleData in moduleDataList)
        {
            if (moduleData.isBuild)
            {
                BuildBundleCompiler.CopyAssetBundlesToStreamingAssets(moduleData);
            }
        }
    }
}
