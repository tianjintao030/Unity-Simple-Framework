using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerAttack : MonoBehaviour
{
    private CharacterController controller;

    public AttackDataGroup attackDataGroup;

    /// <summary>
    /// 上次玩家输入攻击操作的时间
    /// </summary>
    private float lastClickedTime;
    /// <summary>
    /// 上次结束连击的时间
    /// </summary>
    private float lastComboEnd;

    /// <summary>
    /// 当前连击的索引
    /// </summary>
    private int comboIndex;

    /// <summary>
    /// 攻击操作之间的最大允许间隔（超过此值视为新连击）
    /// </summary>
    public float comboMaxInterval = 0.5f;
    /// <summary>
    /// 攻击操作之间的最小有效间隔（防止点击过快）
    /// </summary>
    public float comboMinInterval = 0.2f;
    /// <summary>
    /// 动画播放接近尾部后，延迟多长时间重置连击
    /// </summary>
    public float comboResetDelay = 1f;

    private Animator animator;

    /// <summary>
    /// 攻击动画所在层级
    /// </summary>
    private int attackAnimatorLayer;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Attack();
        }
        ExitAttack();
    }

    private void Attack()
    {
        // 如果距离上次点击超过 0.5 秒且当前连击还未完成
        if (Time.time - lastClickedTime >= comboMaxInterval && comboIndex < attackDataGroup.datas.Count)
        {
            // 防止连击被取消
            CancelInvoke("EndCombo");

            // 与上次点击间隔大于0.2秒，防止误触或过快点击
            if (Time.time - lastClickedTime >= comboMinInterval)
            {
                animator.runtimeAnimatorController = attackDataGroup.datas[comboIndex].overrideController;
                animator.CrossFade("Attack", attackDataGroup.datas[comboIndex].transitionDuration, attackAnimatorLayer, 0);

                lastClickedTime = Time.time;
                comboIndex++;

                if(comboIndex > attackDataGroup.datas.Count)
                {
                    comboIndex = 0;
                }
            }
        }
    }

    private void ExitAttack()
    {
        var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(attackAnimatorLayer);
        if(animatorStateInfo.IsTag("Attack") && animatorStateInfo.normalizedTime >= 0.9f)
        {
            // 留 1 秒窗口调用连击结束方法
            Invoke("EndCombo", comboResetDelay);
        }
    }

    private void EndCombo()
    {
        comboIndex = 0;
        lastComboEnd = Time.time;
    }
}
