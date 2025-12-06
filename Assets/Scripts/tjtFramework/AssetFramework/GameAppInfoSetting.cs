using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    [CreateAssetMenu(menuName = "GameAppInfoSetting", fileName = "GameAppInfoSetting")]
    public class GameAppInfoSetting : InstanceScriptableObject<GameAppInfoSetting>
    {
        protected override string ResourcePath => "GameAppInfoSetting";

        [SerializeField]
        public string GameVersion;
    }
}

