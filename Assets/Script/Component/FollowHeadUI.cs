using UnityEngine;

public class FollowHeadUI_FullLock : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("보통 Main Camera. 비워두면 Start()에서 자동으로 찾음")]
    public Camera HeadCamera;

    [Header("Settings")]
    [Tooltip("UI가 눈앞에 위치할 거리 (m)")]
    public float Distance = 1.2f;

    [Tooltip("UI의 높이 오프셋 (m)")]
    public float HeightOffset = 0f;

    [Tooltip("부드럽게 따라오게 하려면 값 ↑ (0이면 즉시 이동)")]
    public float FollowSpeed = 20f;

    [Header("Options")]
    [Tooltip("첫 프레임에서 즉시 시야 중앙 위치로 이동할지 여부")]
    public bool LockAtStart = true;

    [Tooltip("true면 Lerp/Slerp로 부드럽게 따라감")]
    public bool SmoothFollow = false;

    private bool _initialized = false;

    void Start()
    {
        if (!HeadCamera)
            HeadCamera = Camera.main;

        if (LockAtStart && HeadCamera)
        {
            // 첫 프레임 즉시 시야 중앙 위치로 이동
            transform.position = HeadCamera.transform.position
                               + HeadCamera.transform.forward * Distance
                               + HeadCamera.transform.up * HeightOffset;

            transform.rotation = HeadCamera.transform.rotation;
            _initialized = true;
        }
    }

    void LateUpdate()
    {
        if (!HeadCamera) return;

        Vector3 targetPos = HeadCamera.transform.position
                          + HeadCamera.transform.forward * Distance
                          + HeadCamera.transform.up * HeightOffset;

        Quaternion targetRot = HeadCamera.transform.rotation;

        if (SmoothFollow && FollowSpeed > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * FollowSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * FollowSpeed);
        }
        else
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }
    }
}
