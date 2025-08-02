using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayAnim动画配置
/// </summary>
public class PlayAnimTransition
{
    public float FadeDuration = 0.25f;
    public float Speed = 1f;
    public bool Loop = false;
    public AvatarMask Mask;
    public bool CanInterrupt = false;

    public PlayAnimTransition(float fade = 0.25f, float speed = 1f, bool loop = false, bool canInterrupt = false)
    {
        FadeDuration = fade;
        Speed = speed;
        Loop = loop;
        CanInterrupt = canInterrupt;
    }
}
