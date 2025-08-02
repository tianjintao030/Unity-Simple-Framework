using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

//结构示例：
//PlayAnimPlayer
//│
//├── PlayAnimLayer[0] (Base Layer)
//│   └── AnimationMixerPlayable
//│       └── PlayAnimState (当前动画)
//│       └── PlayAnimState (过渡动画)
//│
//├── PlayAnimLayer[1] (Upper Body Layer)
//│   └── AnimationMixerPlayable + AvatarMask
//│
//└── Output → AnimationPlayableOutput → Animator

/// <summary>
/// PlayAnim控制器，挂载在物体上统一管理动画层
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayAnimPlayer : MonoBehaviour
{
    [SerializeField]
    private PlayAnimConfigData config;

    private Animator animator;

    /// <summary>
    /// 可播放图
    /// </summary>
    private PlayableGraph graph;
    /// <summary>
    /// 动画层级混合器
    /// </summary>
    private AnimationLayerMixerPlayable layerMixer;

    /// <summary>
    /// 动画输出端
    /// </summary>
    private AnimationPlayableOutput output;

    /// <summary>
    /// 管理的动画状态层级列表
    /// </summary>
    private List<PlayAnimLayer> layers = new();

    /// <summary>
    /// 动画状态层级索引管理字典,
    /// key：层级名，value：索引
    /// </summary>
    private Dictionary<string, int> layerIndexMap = new();

    /// <summary>
    /// 动画层级的数量
    /// </summary>
    private int layerCount = 0;

    /// <summary>
    /// PlayAnim控制器初始化
    /// </summary>
    public void Init(PlayAnimConfigData config, int layerCount)
    {
        this.config = config;
        this.layerCount = layerCount;

        animator = GetComponentInChildren<Animator>();
        graph = PlayableGraph.Create($"{this.gameObject.name}PlayableGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        layerMixer = AnimationLayerMixerPlayable.Create(graph, layerCount);

        output = AnimationPlayableOutput.Create(graph, $"{this.gameObject.name}PlayableGraphOutput", animator);
        output.SetSourcePlayable(layerMixer);

        graph.Play();
    }

    /// <summary>
    /// 创建动画状态层级
    /// </summary>
    /// <returns>PlayAnimLayer</returns>
    public PlayAnimLayer CreateLayer(string name, AvatarMask mask = null, float weight = 1f)
    {
        var index = layers.Count;
        if(index >= layerCount)
        {
            Debug.LogError($"{graph.GetEditorName()}最大层级数为{layerCount},现在要加的层级数{index}超限了");
            return null;
        }

        var layer = new PlayAnimLayer(graph, name, mask);
        layers.Add(layer);
        layerIndexMap.Add(name, index);

        layerMixer.ConnectInput(index, layer.Mixer, 0);
        layerMixer.SetInputWeight(index, weight);
        if(mask != null)
        {
            layerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
        }

        return layer;
    }

    /// <summary>
    /// 获取动画状态配置信息
    /// </summary>
    public PlayAnimStateConfig GetStateConfigByName(string name)
    {
        if(config != null && !config.configs.IsNullOrEmpty())
        {
            foreach(var configItem in config.configs)
            {
                if(configItem.name == name)
                {
                    return configItem;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 播放动画状态
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="state"></param>
    /// <param name="fadeDuration"></param>
    public void PlayOnLayer(string layerName, string stateName, float startTime = 0f)
    {
        if(layerIndexMap.TryGetValue(layerName, out var index))
        {
            var config = GetStateConfigByName(stateName);
            var state = new PlayAnimState(graph, config.clip);

            layers[index].Play(state, config.fadeDuration, config.speed, startTime, config.isLoop);
        }
        else
        {
            Debug.LogError($"{graph.GetEditorName()}未找到层级{layerName}");
        }
    }

    /// <summary>
    /// 将动画状态加入层级的队列中
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="state"></param>
    public void QueueInLayer(string layerName, PlayAnimState state)
    {
        if (layerIndexMap.TryGetValue(layerName, out var index))
        {
            layers[index].EnQueue(state);
        }
    }

    /// <summary>
    /// 设置层级权重
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="weight"></param>
    public void SetLayerWeight(string layerName, float weight)
    {
        if (layerIndexMap.TryGetValue(layerName, out var index))
        {
            layerMixer.SetInputWeight(index, weight);
        }
    }

    private void Update()
    {
        if(!layers.IsNullOrEmpty())
        {
            foreach(var layer in layers)
            {
                layer.Update(Time.deltaTime);
            }
        }
    }

    private void OnDestroy()
    {
        if(!layers.IsNullOrEmpty())
        {
            foreach (var layer in layers)
            {
                layer.Dispose();
            }
        }
        layers.Clear();
        layerIndexMap.Clear();

        if(graph.IsValid())
        {
            graph.Destroy();
        }
    }
}
