using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.UI;

public class UITest : MonoBehaviour
{
    void Start()
    {
        UISystem.Instance.Init();
        UISystem.Instance.PopUpWindow<TestWindow>();

        var testWindow = UISystem.Instance.GetVisibleWindow<TestWindow>();
        testWindow.TestGet();
    }
}
