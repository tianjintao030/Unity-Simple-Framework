using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using tjtFramework.AssetFramework;

public class BundleModuleConfigWindow : OdinEditorWindow
{
    [Required, LabelText("资源模块名称")]
    public string moduleName;

    [ReadOnly, HideLabel, TabGroup("预制体模式")]
    public string prefabTabel = "文件夹下所有预制体打进一个AssetBundle包";
    [FolderPath, TabGroup("预制体模式"), LabelText("预制体路径配置")]
    public string[] prefabPathArray = new string[] {};

    [ReadOnly, HideLabel, TabGroup("子文件夹模式")]
    public string rootFolderSubBundleTabel = "文件夹下各个子文件夹各打成一个Assetbundle包";
    [FolderPath, TabGroup("子文件夹模式"), LabelText("子文件夹路径配置")]
    public string[] rootFolderSubBundlePathArray = new string[] {};

    [ReadOnly, HideLabel, TabGroup("单个包模式")]
    public string signBudleTabel = "指定文件夹打成单个AssetBundle包";
    [TabGroup("单个包模式"), LabelText("单文件夹路径配置")]
    public BundleFileInfo[] signBudlePathArray = new BundleFileInfo[] {};

    public static void ShowWindow(string moduleName)
    {
        var window = GetWindowWithRect<BundleModuleConfigWindow>(new Rect(0, 0, 600, 600));
        window.Show();
        window.moduleName = moduleName;

        // 获取数据
        var moduleData = BuildBundleConfig.Instance.GetModuleDataByName(moduleName);
        if (moduleData != null)
        {
            window.prefabPathArray = moduleData.prefabPathArray;
            window.rootFolderSubBundlePathArray = moduleData.rootFolderSubBundlePathArray;
            window.signBudlePathArray = moduleData.signBudlePathArray;
        }
    }

    [OnInspectorGUI]
    public void DrawButton()
    {
        GUILayout.BeginVertical();
        if(GUILayout.Button("删除Moudle", GUILayout.Width(600), GUILayout.Height(25)))
        {
            DeleteModuleConfig();
        }

        if (GUILayout.Button("保存Moudle", GUILayout.Width(600), GUILayout.Height(25)))
        {
            SaveModuleConfig();
        }
        GUILayout.EndVertical();
    }

    private void DeleteModuleConfig()
    {
        BuildBundleConfig.Instance.RemovModuleDataByName(moduleName);
        EditorUtility.DisplayDialog($"删除{moduleName}", "删除配置成功", "OK");
        Close();
        BuildWindow.ShowAssetBundleWindow();
    }

    private void SaveModuleConfig()
    {
        if(!string.IsNullOrEmpty(moduleName))
        {
            var moduleData = BuildBundleConfig.Instance.GetModuleDataByName(moduleName);
            if(moduleData == null)
            {
                moduleData = new BuildModuleData();
                moduleData.moduleName = moduleName;
                moduleData.prefabPathArray = prefabPathArray;
                moduleData.rootFolderSubBundlePathArray = rootFolderSubBundlePathArray;
                moduleData.signBudlePathArray = signBudlePathArray;

                BuildBundleConfig.Instance.AddAndSaveModuleData(moduleData);
            }
            else
            {
                moduleData.moduleName = moduleName;
                moduleData.prefabPathArray = prefabPathArray;
                moduleData.rootFolderSubBundlePathArray = rootFolderSubBundlePathArray;
                moduleData.signBudlePathArray = signBudlePathArray;
            }

            EditorUtility.DisplayDialog($"保存{moduleName}", "保存配置成功", "OK");
            Close();
            BuildWindow.ShowAssetBundleWindow();
        }
        else
        {
            EditorUtility.DisplayDialog($"保存{moduleName}", "保存配置失败，因为moduleName为空", "OK");
        }
    }
}
