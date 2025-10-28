using UnityEngine;

[System.Serializable]
public class MessageGroupData
{
    public string groupName;          // 그룹 이름 (Inspector 용)
    public string[] messages;         // 메시지 문자열 배열
    public GameObject extraContent;   // 손 이미지 등 추가 콘텐츠
    public float extraDuration = 1f;  // extraContent 표시 시간
    public bool isCenter = false;     // 중앙 위치 여부
}
