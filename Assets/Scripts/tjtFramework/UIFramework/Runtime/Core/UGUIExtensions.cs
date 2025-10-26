using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UGUIExtensions
{
    // 使用将Scale设为(0,0,0)的方法来控制UGUI组件的显隐，避免使用SetActive造成UI网格重绘

    public static void SetVisible(this GameObject go, bool visible)
    {
        go.transform.localScale = visible ? Vector3.one : Vector3.zero;
    }

    public static void SetVisible(this Transform trans, bool visible)
    { 
       trans.localScale = visible ? Vector3.one : Vector3.zero;
    }
}
