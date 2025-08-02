using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayAnimConfigData_", menuName = "PlayAnim/PlayAnimConfigData")]
public class PlayAnimConfigData : ScriptableObject
{
    public List<PlayAnimStateConfig> configs = new();
}

/// <summary>
/// 动画状态相关配置参数管理项
/// </summary>
[System.Serializable]
public class PlayAnimStateConfig
{
    public string name;
    public AnimationClip clip;
    public float fadeDuration = 0.2f;
    public float speed = 1f;
    public bool isLoop = false;
}
