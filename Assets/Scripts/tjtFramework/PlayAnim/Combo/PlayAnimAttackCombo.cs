using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.Input;

public class PlayAnimAttackCombo : MonoBehaviour
{
    [SerializeField]
    private PlayAnimConfigData config;
    [SerializeField]
    private PlayAnimPlayer player;

    private string layerName = "combo";
    private KeyCode comboKey = KeyCode.Mouse0;

    [SerializeField]
    private string idleName = "idle";
    [SerializeField]
    private List<string> attackNames = new();

    [SerializeField]
    private int currentComboIndex = 0;
    [SerializeField]
    private bool isPlayingState = false;


    void Start()
    {
        currentComboIndex = 0;

        player.Init(config, 1);
        player.CreateLayer(layerName);
        player.PlayOnLayer(layerName, idleName);
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            InputBuffer.Instance.EnqueueInputBuffer(InputType.attack, 5f);
        }

        if(!isPlayingState && InputBuffer.Instance.IsHaveInputBuffer(InputType.attack))
        {
            InputBuffer.Instance.ConsumeInputBuffer(InputType.attack, () =>
            {
                PlayCombo();
            });
        }
    }

    private void PlayCombo()
    {
        if(currentComboIndex >= attackNames.Count)
        {
            Debug.Log("到达连击最大数");
            EndCombo();
            return;
        }

        Debug.Log($"执行连击{attackNames[currentComboIndex]}");

        var stateName = attackNames[currentComboIndex];
        var state = player.PlayOnLayer(layerName, stateName);

        isPlayingState = true;

        state.AddEvent(0.95f, () =>
        {
            isPlayingState = false;

            if(!InputBuffer.Instance.IsHaveInputBuffer(InputType.attack))
            {
                Debug.Log("未接受到下一次攻击输入");
                EndCombo();
            }
        });

        currentComboIndex++;
    }

    private void EndCombo()
    {
        isPlayingState = false;
        currentComboIndex = 0;
        player.PlayOnLayer(layerName, idleName);

        InputBuffer.Instance.ClearAllInputBuffer();

        Debug.Log("结束连击");
    }
}
