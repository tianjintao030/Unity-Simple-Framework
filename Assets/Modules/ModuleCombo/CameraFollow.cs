using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("视角参数")]
    public float distance = 2.3f;     // 距离目标的距离
    public float height = 0.4f;       // 摄像机高度偏移
    public float rotateSpeed = 5.0f;  // 鼠标旋转速度
    public float zoomSpeed = 2.0f;    // 缩放速度
    public float minDistance = 0f;    // 最小缩放
    public float maxDistance = 10f;   // 最大缩放

    private float currentYaw = 0.0f;
    private float currentPitch = 15.0f;  // 初始为平视稍上视角
    public float minPitch = -20f;
    public float maxPitch = 60f;

    void LateUpdate()
    {
        if (target == null) return;

        // 鼠标右键控制旋转
        if (Input.GetMouseButton(1))
        {
            currentYaw += Input.GetAxis("Mouse X") * rotateSpeed;
            currentPitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        // 鼠标滚轮控制距离
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // 计算摄像机方向
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        // 设置摄像机位置
        Vector3 cameraPos = target.position + Vector3.up * height + offset;
        transform.position = cameraPos;

        // 看向目标
        transform.LookAt(target.position + Vector3.up * height);
    }
}
