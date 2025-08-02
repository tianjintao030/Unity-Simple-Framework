using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayAnim动画事件
/// </summary>
public class PlayAnimEvent
{
    /// <summary>
    /// 事件触发的归一化时间
    /// </summary>
    private float normalizedTime;

    /// <summary>
    ///  事件回调
    /// </summary>
    private Action callback;

    /// <summary>
    /// 是否可多次触发
    /// </summary>
    private bool loop;

    /// <summary>
    /// 是否已经触发过
    /// </summary>
    private bool triggered;

    public PlayAnimEvent(float normalizedTime, Action callback, bool loop = false)
    {
        this.normalizedTime = Mathf.Clamp01(normalizedTime);
        this.callback = callback;
        this.loop = loop;
        triggered = false;
    }

    public void TryTrigger(float normalizedTime)
    {
        if (!loop && triggered)
        {
            return;
        }
        if (this.normalizedTime >= normalizedTime)
        {
            callback?.Invoke();
            if (!loop)
            {
                triggered = true;
            }
        }
    }

    public void Reset() => triggered = false;
}
