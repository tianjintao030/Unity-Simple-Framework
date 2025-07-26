using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.PublicMono;

public class MonoSample : MonoBehaviour
{
    private Coroutine coroutine;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            UpdateBegin();
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            UpdateEnd();
        }
    }

    void UpdateBegin()
    {
        MonoManager.Instance.AddUpdateListener(Test1);

        coroutine = MonoManager.Instance.StartCoroutine(Test2());
    }

    void UpdateEnd()
    {
        MonoManager.Instance.RemoveUpdateListener(Test1);

        MonoManager.Instance.StopCoroutine(coroutine);
    }

    void Test1()
    {
        Debug.Log("MonoSample Test Update");
    }

    IEnumerator Test2()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("MonoSample Test IEnumerator");
    }
}
