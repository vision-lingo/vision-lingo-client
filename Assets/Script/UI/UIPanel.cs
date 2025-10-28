using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    private Action onComplete;

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
           });
    }
}
