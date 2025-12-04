using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

// TODO: Panel 일시정지 기능 구현
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
    private Coroutine _waitCoroutine;

    private Sequence _sequence;

    public void OnPause()
    {
        _sequence.Pause();
    }   
    public void OnResume()
    {
        _sequence.Play();
    }   


    // 25/10/29 CY: 자동으로 비활성화 되지 않는 UI 호출 시 사용.
    public void Show(string message, Vector2 anchoredPosition, float fadeInTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        _sequence = DOTween.Sequence();
        _sequence.Append(canvasGroup.DOFade(1, fadeInTime))
           .OnComplete(() =>
           {
               //gameObject.SetActive(false);
               onComplete?.Invoke();
               IsCompleted = true;
           });
    }

    /// <summary>
    /// 메시지를 표시하고, 특정 조건이 충족될 때까지 기다린 후 사라집니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="anchoredPosition">UI 위치</param>
    /// <param name="waitUntil">true를 반환할 때까지 대기할 조건 함수</param>
    /// <param name="fadeInTime">나타나는 시간</param>
    /// <param name="fadeOutTime">사라지는 시간</param>
    /// <param name="onComplete">완료 시 호출될 액션</param>
    public void Show(string message, Vector2 anchoredPosition, bool waitUntil, float fadeInTime = 0.8f, float fadeOutTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        _sequence = DOTween.Sequence();
        _sequence.Append(canvasGroup.DOFade(1, fadeInTime))
           // 조건이 충족될 때까지 대기하는 코루틴을 실행합니다.
           .AppendCallback(() => _waitCoroutine = StartCoroutine(WaitUntilCondition()))
           .Append(canvasGroup.DOFade(0, fadeOutTime))
           .OnComplete(()=>OnSequenceComplete(onComplete)).SetAutoKill(false);
    }
    public void Show(string message, Vector2 anchoredPosition, float fadeInTime = 0.8f, float displayTime = 2f, float fadeOutTime = 0.8f, Action onComplete = null)
    {
        this.onComplete = onComplete;
        messageText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

        _sequence = DOTween.Sequence();
        _sequence.Append(canvasGroup.DOFade(1, fadeInTime))
           .AppendInterval(displayTime)
           .Append(canvasGroup.DOFade(0, fadeOutTime))
           .OnComplete(()=>OnSequenceComplete(onComplete)).SetAutoKill(false);
    }

    /// <summary>
    /// 튜토리얼 인터렉션 할 때까지 기다리기
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitUntilCondition()
    {
        _sequence.Pause();
        yield return new WaitUntil(()=>UIPanelFactory.Instance.IsInteract);
        _sequence.Play();
        UIPanelFactory.Instance.IsInteract = false;
    }

    private void OnSequenceComplete(Action act_complete)
    {
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }
        IsCompleted = true;
        _sequence.Kill();
        gameObject.SetActive(false);
        act_complete?.Invoke();
    }
}
