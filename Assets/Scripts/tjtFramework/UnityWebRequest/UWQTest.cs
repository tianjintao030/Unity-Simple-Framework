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
            (str) => { Debug.Log($"�����ı�{str}"); }, () => { Debug.Log("����ʧ��"); });

        UnityWebRequestManager.Instance.LoadRes<AssetBundle>("file://" + Application.streamingAssetsPath + "/test",
            (ab) => { Debug.Log($"����AB������{ab}"); }, () => { Debug.Log("����ʧ��"); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
