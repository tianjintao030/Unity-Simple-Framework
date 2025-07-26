using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.UWQ;

public class UWQTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityWebRequestManager.Instance.LoadRes<string>("file://" + Application.streamingAssetsPath + "/test.txt",
            (str) => { Debug.Log($"加载文本{str}"); }, () => { Debug.Log("加载失败"); });

        UnityWebRequestManager.Instance.LoadRes<AssetBundle>("file://" + Application.streamingAssetsPath + "/test",
            (ab) => { Debug.Log($"加载AB包名称{ab}"); }, () => { Debug.Log("加载失败"); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
