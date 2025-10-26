using System.Collections;
using System.Collections.Generic;
using tjtFramework.Singleton;
using UnityEngine;

namespace tjtFramework.UI
{
    [CreateAssetMenu(fileName = "UISetting", menuName = "UISetting")]
    public class UISetting : ScriptableObject
    {
        private static UISetting instance;
        public static UISetting Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = Resources.Load<UISetting>("UISetting");
                }
                return instance;
            }
        }
    }

    public enum UILayer
    {
        Game,
        Fixed,
        Normal,
        TopBar,
        Upper,
        PopUp
    }
}

