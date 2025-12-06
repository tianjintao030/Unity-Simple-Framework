using System.Collections;
using System.Collections.Generic;
using tjtFramework.AssetFramework;
using tjtFramework.Utiliy;
using UnityEditor;
using UnityEngine;

public class BuildHotPatchWindow : BundleBehaviour
{
    protected override int oneRowCount { get => 5; }

    private int selectedGameVersionIndex;
    private string resourceVersion;

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

        }
    }

    private string[] buildButtonsNames = new string[] { "打包热更", "上传更新" };

    public override void DrawBuildButtons()
    {
        base.DrawBuildButtons();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        for (int i = 0; i < buildButtonsNames.Length; i++)
        {
            if (GUILayout.Button(buildButtonsNames[i], GUILayout.Width(120), GUILayout.Height(32)))
            {
                if (buildButtonsNames[i] == "打包热更")
                {
                    BuildBundle();
                }
                if (buildButtonsNames[i] == "上传更新")
                {
                    CopyBundleToStreamingAssetsPath();
                }
            }

            GUILayout.Space(10);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public override void DrawDetailContent()
    {
        base.DrawDetailContent();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("游戏版本号:", GUILayout.Width(80));
        selectedGameVersionIndex = EditorGUILayout.Popup(selectedGameVersionIndex, GameVersion.versions, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("资源版本号:", GUILayout.Width(80));
        resourceVersion = EditorGUILayout.TextField(resourceVersion, GUILayout.Width(200), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
    }

    public override void BuildBundle()
    {
        base.BuildBundle();

        if (moduleDataList.IsNullOrEmpty())
        {
            return;
        }

        var gameVersion = GameVersion.versions[selectedGameVersionIndex];
        GameAppInfoSetting.Instance.GameVersion = gameVersion;  
        foreach (var moduleData in moduleDataList)
        {
            if (moduleData.isBuild)
            {
                BuildBundleCompiler.BuildAssetBundle(moduleData, BuildType.HotPatch, gameVersion, resourceVersion);
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

            }
        }
    }
}
