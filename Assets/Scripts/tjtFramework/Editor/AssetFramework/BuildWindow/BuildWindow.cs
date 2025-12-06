using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.AssetFramework;
using UnityEditor;
using UnityEngine;

public class BuildWindow : OdinMenuEditorWindow
{
    [SerializeField]
    private BuildAssetBundleWindow buildAssetBundleWindow = new BuildAssetBundleWindow();
    [SerializeField]
    private BuildHotPatchWindow hotPatchWindow = new BuildHotPatchWindow();
    //[SerializeField]
    //private BuildBundleSetting settingWindow;

    [MenuItem("AssetFramework/AssetBundleWindow")]
    public static void ShowAssetBundleWindow()
    {
        // 先关闭旧的同类型窗口
        var oldWindows = Resources.FindObjectsOfTypeAll<BuildWindow>();
        if(oldWindows != null && oldWindows.Length > 0)
        {
            foreach (var w in oldWindows)
            {
                w.Close();
            }
        }

        var buildWindow = GetWindow<BuildWindow>();
        buildWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        buildWindow.ForceMenuTreeRebuild();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        buildAssetBundleWindow.Init();
        hotPatchWindow.Init();

        var menuTree = new OdinMenuTree(supportsMultiSelect: false)
        {
            {"Build", null, EditorIcons.TriangleDown },
            {"Build/AssetBundle",  buildAssetBundleWindow, EditorIcons.UnityLogo},
            {"Build/HotPatch",  hotPatchWindow, EditorIcons.UnityLogo},
            {"Setting",  BuildBundleSetting.Instance, EditorIcons.SettingsCog},
        };
        return menuTree;
    }
}
