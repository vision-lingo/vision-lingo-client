using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    private Action onComplete; // 메시지 끝났을 때 콜백

    public void Show(string message, float fadeInTime = 0.8f, float displayTime = 2f, float fadeOutTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, fadeInTime))
           .AppendInterval(displayTime)
           .Append(canvasGroup.DOFade(0, fadeOutTime))
           .OnComplete(() =>
           {
               gameObject.SetActive(false);
               onComplete?.Invoke(); // 다음 메시지 처리
           });
    }
}
