using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework.GameResources
{
    [CreateAssetMenu(menuName = "AssetBudle/ResourcesLoadSetting", fileName = "ResourcesLoadSetting")]
    public class ResourcesLoadSetting : InstanceScriptableObject<ResourcesLoadSetting>
    {
        protected override string ResourcePath => "ResourcesLoadSetting";

        [SerializeField]
        public bool isEditorModel;
    }
}

