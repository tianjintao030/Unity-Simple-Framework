using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target;

    [Header("�ӽǲ���")]
    public float distance = 2.3f;     // ����Ŀ��ľ���
    public float height = 0.4f;       // ������߶�ƫ��
    public float rotateSpeed = 5.0f;  // �����ת�ٶ�
    public float zoomSpeed = 2.0f;    // �����ٶ�
    public float minDistance = 0f;    // ��С����
    public float maxDistance = 10f;   // �������

    private float currentYaw = 0.0f;
    private float currentPitch = 15.0f;  // ��ʼΪƽ�������ӽ�
    public float minPitch = -20f;
    public float maxPitch = 60f;

    void LateUpdate()
    {
        if (target == null) return;

        // ����Ҽ�������ת
        if (Input.GetMouseButton(1))
        {
            currentYaw += Input.GetAxis("Mouse X") * rotateSpeed;
            currentPitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        // �����ֿ��ƾ���
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // �������������
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        // ���������λ��
        Vector3 cameraPos = target.position + Vector3.up * height + offset;
        transform.position = cameraPos;

        // ����Ŀ��
        transform.LookAt(target.position + Vector3.up * height);
    }
}
