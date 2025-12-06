using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEditor;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    [CreateAssetMenu(menuName = "AssetBudle/BuildBundleConfig", fileName = "BuildBundleConfig")]
    public class BuildBundleConfig : InstanceScriptableObject<BuildBundleConfig>
    {
        protected override string ResourcePath => "BuildBundleConfig";

        /// <summary>
        /// 构建模块配置
        /// </summary>
        public List<BuildModuleData> AssetBundleConfig = new();

        public BuildModuleData GetModuleDataByName(string name)
        {
            if (!AssetBundleConfig.IsNullOrEmpty())
            {
                foreach (var moduleData in AssetBundleConfig)
                {
                    if (moduleData.moduleName == name)
                    {
                        return moduleData;
                    }
                }
            }
            return null;
        }

        public void RemovModuleDataByName(string name)
        {
            for (int i = AssetBundleConfig.Count - 1; i >= 0; i--)
            {
                if (AssetBundleConfig[i].moduleName == name)
                {
                    AssetBundleConfig.RemoveAt(i);
                }
            }
        }

        public void AddAndSaveModuleData(BuildModuleData moduleData)
        {
            AssetBundleConfig.Add(moduleData);
            SaveAssetBundleConfig();
        }

        public void SaveAssetBundleConfig()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}

