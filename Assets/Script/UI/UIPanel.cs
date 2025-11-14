using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    /// <summary>
    /// 팝업 지속시간이 끝나면 true
    /// 이 boolean 값으로 컴플리트 판정 해야함. gameObject.activeself는 불안정함.
    /// </summary>
    public bool IsCompleted {get; private set;} = false;

    private Action onComplete;
    

    // 25/10/29 CY: 자동으로 비활성화 되지 않는 UI 호출 시 사용.
    public void Show(string message, Vector2 anchoredPosition, float fadeInTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, fadeInTime))
           .OnComplete(() =>
           {
               //gameObject.SetActive(false);
               onComplete?.Invoke();
               IsCompleted = true;
           });
    }

    public void Show(string message, Vector2 anchoredPosition, float fadeInTime = 0.8f, float displayTime = 2f, float fadeOutTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, fadeInTime))
           .AppendInterval(displayTime)
           .Append(canvasGroup.DOFade(0, fadeOutTime))
           .OnComplete(() =>
           {
               gameObject.SetActive(false);
               onComplete?.Invoke();
               IsCompleted = true;
           });
    }
}
