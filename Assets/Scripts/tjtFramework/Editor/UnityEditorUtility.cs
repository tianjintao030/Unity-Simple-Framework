using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEditorUtility
{
    public static GUIStyle GetGUIStyle(string styleName)
    {
        foreach(var style in GUI.skin.customStyles)
        {
            if(string.Equals(styleName.ToLower(), style.name.ToLower()))
            {
                return style;
            }
        }

        return null;
    }
}
