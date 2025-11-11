using UnityEngine;
using TMPro;

public class ScrollCredits : MonoBehaviour
{
    [Header("References")]
    public RectTransform textTransform;

    [Header("Settings")]
    public float scrollSpeed = 50f;
    public float startYOffset = -500f;  // 시작 위치 아래로
    public float extraOffset = 50f;     // 텍스트 끝 위로 여유

    private Vector3 startPos;
    private float resetHeight;

    void Start()
    {
        if (textTransform == null)
        {
            Debug.LogError("TextTransform이 연결되지 않았습니다!");
            return;
        }

        // 1. TMP 전체 렌더링 높이를 가져오기
        TextMeshProUGUI tmp = textTransform.GetComponent<TextMeshProUGUI>();
        float textHeight = tmp.preferredHeight; // 태그 포함 전체 높이 계산

        // 2. 시작 위치를 화면 아래로 설정
        startPos = textTransform.localPosition + Vector3.up * startYOffset;
        textTransform.localPosition = startPos;

        // 3. 리셋 위치 = 시작 위치 + 전체 텍스트 높이 + 여유 공간
        resetHeight = startPos.y + textHeight + extraOffset;
    }

    void Update()
    {
        if (textTransform == null) return;

        // 위로 스크롤
        textTransform.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

        // 화면 위로 끝까지 올라가면 초기 위치로
        if (textTransform.localPosition.y >= resetHeight)
        {
            textTransform.localPosition = startPos;
        }
    }
}
