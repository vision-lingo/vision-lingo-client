using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MessageGroupData
{
    public string groupName;          // 그룹 이름 (Inspector 용)
    public string[] messages;         // 메시지 문자열 배열
    //public GameObject extraContent;   // 손 이미지 등 추가 콘텐츠
    //public float extraDuration = 1f;  // extraContent 표시 시간
    // 25/10/29 CY: ExtraContent를 클래스로 따로 뺐습니다.
    public ExtraContent[] extraContent;
    public bool isCenter = false;     // 중앙 위치 여부
}

[System.Serializable]
public class ExtraContent
{
    public GameObject extraContent;   // 손 이미지 등 추가 콘텐츠
    public float extraDuration = 1f;  // extraContent 표시 시간(쓰이지 않음.)
    public Vector3 pos;
    public Vector3 rot;
    public UnityEvent act_Start;
}