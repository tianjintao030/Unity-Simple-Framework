using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.UI;

public class TestWindow : WindowBase
{
    public override UILayer Layer => UILayer.Game;

    public override void OnAwake()
    {
        base.OnAwake();
        Debug.Log("初始化");
    }

    public override void OnShow()
    {
        base.OnShow();
        Debug.Log("显示");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnHide()
    {
        base.OnHide();
        Debug.Log("关闭");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Debug.Log("销毁");
    }

    public void TestGet()
    {
        Debug.Log("Get Window");
    }
}
