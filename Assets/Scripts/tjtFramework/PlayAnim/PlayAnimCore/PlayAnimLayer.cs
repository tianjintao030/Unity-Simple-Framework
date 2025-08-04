using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// PlayAnim动画层，管理动画队列与过渡播放
/// </summary>
public class PlayAnimLayer : IDisposable
{
    public string Name {  get; private set; }

    /// <summary>
    /// 该层级权重
    /// </summary>
    private float weight;
    public float Weight => weight;

    /// <summary>
    /// 骨骼遮罩
    /// </summary>
    public AvatarMask Mask { get; set; }

    private PlayableGraph graph;

    /// <summary>
    /// 可播放动画混合器
    /// </summary>
    private AnimationMixerPlayable mixer;
    public AnimationMixerPlayable Mixer => mixer;

    /// <summary>
    /// 动画状态队列
    /// </summary>
    private Queue<PlayAnimState> stateQueue = new();

    private PlayAnimState currentState;
    private PlayAnimState nextState;

    /// <summary>
    /// 过渡时间
    /// </summary>
    private float fadeDuration = 0.2f;
    /// <summary>
    /// 已经过过渡时间
    /// </summary>
    private float fadeElapsed = 0f;
    /// <summary>
    /// 是否正在过渡
    /// </summary>
    private bool isFading = false;

    /// <summary>
    /// 当前使用的是mixer的哪个输入端口字段；
    /// 目前只用0，1端口轮次播放；
    /// 比方说当前动画在端口0，进来的新动画就在端口1，过渡完成之后，端口1的动画变成当前动画，再新进来的动画则放在端口0
    /// </summary>
    private int currentInputIndex = 0;

    public PlayAnimLayer(PlayableGraph graph, string name, AvatarMask mask)
    {
        this.graph = graph;
        this.Name = name;
        this.Mask = mask;

        mixer = AnimationMixerPlayable.Create(graph, 2);
        mixer.SetInputWeight(0, 1f);
        mixer.SetInputWeight(1, 0f);
    }

    public void Play(PlayAnimState state, float fadeDuration, float speed = 1f,
        float startTime = 0f, bool isLoop = false)
    {
        // 第一个动画直接播放
        if(currentState == null)
        {
            currentState = state;

            AddMixerInput(state, 0);
            state.Play(startTime, speed, isLoop);

            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);
        }
        else
        {
            nextState = state;
            this.fadeDuration = fadeDuration;
            fadeElapsed = 0f;
            isFading = true;

            AddMixerInput(nextState, 1 - currentInputIndex);
            state.Play(startTime, speed, isLoop);
        }
    }

    public void Update(float deltaTime)
    {
        currentState?.Update(deltaTime);
        nextState?.Update(deltaTime);

        // 动画状态过渡
        if(isFading)
        {
            // 根据时间增量设置两端口的混合权重来达到过渡的效果
            fadeElapsed += deltaTime;
            var t = Mathf.Clamp01(fadeElapsed / fadeDuration);

            mixer.SetInputWeight(currentInputIndex, 1f - t);
            mixer.SetInputWeight(1 - currentInputIndex, t);

            // 过渡完成时
            if(t >= 1f)
            {
                currentState?.Dispose();
                currentState = nextState;
                nextState = null;

                mixer.DisconnectInput(currentInputIndex);
                currentInputIndex = 1 - currentInputIndex;

                isFading = false;
            }
        }

        //设置动画状态队列的依次播放
        if (currentState != null && !isFading && !currentState.loop && IsCurrentStateFinshed())
        {
            if (stateQueue.Count > 0)
            {
                this.Play(stateQueue.Dequeue(), fadeDuration);
            }
        }
    }

    /// <summary>
    /// 设置该层级的权重
    /// </summary>
    /// <param name="weight">归一化权重</param>
    public void SetWeight(float weight)
    {
        this.weight = weight;
    }

    /// <summary>
    /// 将动画混合器绑定输出到指定Animator
    /// </summary>
    /// <returns>AnimationPlayableOutput</returns>
    public AnimationPlayableOutput BindMixerToAnimator(Animator animator)
    {
        var output = AnimationPlayableOutput.Create(graph, Name, animator);
        output.SetSourcePlayable(mixer);
        return output;
    }

    /// <summary>
    /// 在动画状态队列结尾插入动画状态
    /// </summary>
    public void EnQueue(PlayAnimState state)
    {
        stateQueue.Enqueue(state);

        if(currentState == null)
        {
            currentState = state;
            mixer.Play();
        }
    }

    /// <summary>
    /// 结束动画状态队列的执行并清空
    /// </summary>
    public void EndStateQueueExecute()
    {
        stateQueue.Clear();
    }

    /// <summary>
    /// 给动画混合器新增连接的动画状态
    /// </summary>
    /// <param name="state"></param>
    /// <param name="port">端口索引</param>
    private void AddMixerInput(PlayAnimState state, int port)
    {
        if(mixer.GetInputCount() <= port)
        {
            mixer.SetInputCount(port + 1);
        }

        mixer.DisconnectInput(port);
        mixer.ConnectInput(port, state.Playable, 0);
        mixer.SetInputWeight(port, 0f);
    }

    /// <summary>
    /// 当前动画状态是否已播放完毕
    /// </summary>
    /// <returns></returns>
    private bool IsCurrentStateFinshed()
    {
        return currentState != null && currentState.Clip != null &&
            currentState.Playable.GetTime() >= currentState.Clip.length;
    }

    public void Dispose()
    {
        currentState?.Dispose();
        nextState?.Dispose();
        
        stateQueue.Clear();

        if(mixer.IsValid())
        {
            mixer.Destroy();
        }
    }
}
