using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// PlayAnim动画状态
/// </summary>
public class PlayAnimState : IDisposable
{
    public string Name {  get; private set; }

    /// <summary>
    /// 动画片段
    /// </summary>
    public AnimationClip Clip { get; private set; }

    /// <summary>
    /// 动画可播放Playable
    /// </summary>
    public AnimationClipPlayable Playable { get; private set; }

    public float speed = 1f;
    public bool loop = false;
    //public AvatarMask Mask;

    public Action OnEnd;
    public Action<float> OnUpdate;

    private float elapsedTime;
    private bool isPlaying;

    private List<PlayAnimEvent> animEvents = new();

    public PlayAnimState(PlayableGraph graph, AnimationClip clip)
    {
        Clip = clip;
        Playable = AnimationClipPlayable.Create(graph, clip);
        Playable.SetApplyFootIK(false);
    }

    /// <summary>
    /// 播放该状态动画
    /// </summary>
    /// <param name="startTime">动画从什么时间开始播放（0-1 归一化）</param>
    /// <param name="speed">动画播放速度</param>
    /// <param name="loop">动画是否循环播放</param>
    public void Play(float startTime = 0f, float speed = 1f, bool loop = false)
    {
        this.speed = speed;
        this.loop = loop;
        elapsedTime = 0;
        isPlaying = true;

        foreach (var e in animEvents)
        {
            e.Reset();
        }

        Playable.SetTime(Clip.length * startTime);
        Playable.SetSpeed(speed);
        Playable.SetDuration(Clip.length);
        Clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Default;
        Playable.Play();
    }

    public void Update(float deltaTime)
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime += deltaTime * speed;
        // 强制循环时间控制
        if (loop)
        {
            if (Clip.length > 0f)
            {
                elapsedTime %= Clip.length;
            }
        }

        // 强制推进 Playable 时间
        Playable.SetTime(elapsedTime);
        Playable.SetDone(false); // 非常重要：防止 Playables 把自己标记为完成

        // normalizedTime 归一化
        float normalizedTime = (Clip.length > 0f) ? elapsedTime / Clip.length : 0f;
        if (!loop)
        {
            normalizedTime = Mathf.Clamp01(normalizedTime);
        }

        // 动画事件触发
        foreach (var e in animEvents)
        {
            e.TryTrigger(normalizedTime);
        }

        OnUpdate?.Invoke(normalizedTime);

        // 非循环情况下触发结束
        if (!loop && elapsedTime >= Clip.length)
        {
            isPlaying = false;
            OnEnd?.Invoke();
        }
    }

    public void AddEvent(float normalizedTime, Action callback, bool loop = false)
    {
        animEvents.Add(new PlayAnimEvent(normalizedTime, callback, loop));
    }

    public void Dispose()
    {
        if (Playable.IsValid())
        {
            Playable.Destroy();
        }

        OnEnd = null;
        OnUpdate = null;
        animEvents.Clear();
        isPlaying = false;
    }
}
