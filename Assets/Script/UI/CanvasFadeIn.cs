using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 시작 시 투명하게 설정
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        // 시작 지연
        if (startDelay > 0)
        {
            float timer = 0f;
            while (timer < startDelay)
            {
                if (!MainSystem.Instance.IsPause)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            if (!MainSystem.Instance.IsPause)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
                canvasGroup.alpha = fadeCurve.Evaluate(normalizedTime);
            }
            yield return null;
        }

        // 최종적으로 완전히 보이게 설정
        canvasGroup.alpha = 1f;
    }

    // 외부에서 페이드인을 다시 시작하고 싶을 때 사용
    public void RestartFadeIn()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
        StartCoroutine(FadeInCoroutine());
    }
}
