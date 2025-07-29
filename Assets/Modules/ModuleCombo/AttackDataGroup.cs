using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Attack Data Group")]
public class AttackDataGroup : ScriptableObject
{
    public List<AttackData> datas = new();
}

[System.Serializable]
public class AttackData
{
    /// <summary>
    /// 动画重写控制器
    /// </summary>
    public AnimatorOverrideController overrideController;

    /// <summary>
    /// 切换到该动画的过渡时间
    /// </summary>
    public float transitionDuration;

#region 字段
    /// <summary>
    /// 伤害
    /// </summary>
    public float damage;
#endregion
}
