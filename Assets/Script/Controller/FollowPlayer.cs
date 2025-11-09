using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // XR Origin 또는 Camera
    public float distance = 2.0f; // 앞쪽 거리
    public float height = 1.5f;   // 눈높이 조절
    public float followSpeed = 5f;

    void Update()
    {
        if (player == null) return;

        // 플레이어 앞 위치 계산
        Vector3 targetPosition = player.position + player.forward * distance;
        targetPosition.y = player.position.y + height;

        // 부드럽게 따라가기
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 항상 플레이어를 바라보게
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        transform.Rotate(0, 180, 0); // 텍스트가 뒤집혀 있으면 반대로 돌려주기
    }
}
