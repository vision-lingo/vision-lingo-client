using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class StageController : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("StageSpawner 컴포넌트 (필수)")]
    public StageSpawner spawner;

    [Tooltip("보통 Main Camera. 비워두면 Start()에서 Camera.main")]
    public Camera HeadCamera;

    [Tooltip("안내/피드백용 UI 패널 (FollowHeadUI_FullLock 붙은 오브젝트)")]
    public GameObject UIPanel;

    [Tooltip("UI 패널 내부의 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI UIText;

    [Header("Intro/Outro UI (훈련 시작/안내/종료)")]
    [Tooltip("인트로/아웃트로 전용 UI 패널 (새 위치)")]
    public GameObject IntroPanel;
    
    [Tooltip("인트로/아웃트로용 TextMeshProUGUI")]
    public TextMeshProUGUI IntroText;

  [Header("Flow Settings")]
    [Tooltip("시작 스테이지 번호")]
    public int FirstStage = 1;

    [Tooltip("마지막 스테이지 번호")]
    public int LastStage = 6;

    [Tooltip("각 스테이지별 라운드 수")]
    public int RoundsPerStage = 6;

    [Header("Timings (sec)")]
    [Tooltip("안내 UI 유지시간 (소리 전까지)")]
    public float PreSoundDelay = 1f;

    [Tooltip("소리 발생 후 하이라이트가 켜지기까지의 지연")]
    public float HighlightDelay = 10f;

    [Tooltip("소리 직후부터의 총 선택 제한시간 (예: 15초)")]
    public float AnswerTimeout = 15f;

    [Tooltip("스테이지 사이 대기시간")]
    public float InterStageDelay = 1.0f;

    [Header("Debug / Log")]
    [Tooltip("디버그 로그 출력 여부")]
    public bool EnableLogging = true;

    [SerializeField] private CanvasGroup _uiCanvasGroup;
    [SerializeField] private CanvasGroup _introCanvasGroup; 

    private List<GameObject> _activeBalls = new List<GameObject>();
    private GameObject _correctBall = null;
    private GameObject _selectedBall = null;
    private bool _isAwaitingSelection = false;
    private bool _isShowingWrongMsg = false;

    private Coroutine _hintCo;

    private readonly Dictionary<InteractiveSphere, System.Action<InteractiveSphere.SphereState>> _stateHandlers
        = new Dictionary<InteractiveSphere, System.Action<InteractiveSphere.SphereState>>();

    void Start()
    {
        if (!HeadCamera) HeadCamera = Camera.main;
        if (!spawner || !HeadCamera || !UIPanel || !UIText)
        {
            Debug.LogError("[StageController] 레퍼런스가 비었습니다.");
            enabled = false;
            return;
        }

        UIPanel.SetActive(false);
        IntroPanel.SetActive(false);
        StartCoroutine(RunAllStages());
    }

    private IEnumerator RunAllStages()
    {
        // 인트로
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리 위치 분별 훈련을 시작하겠습니다.", 0.4f, 3f, 0.8f));
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리가 나는 공을 찾아 선택해 주세요.", 0.4f, 3f, 0.8f));

        // 스테이지 루프
        for (int stage = FirstStage; stage <= LastStage; stage++)
        {
            spawner.SetStage(stage);
            spawner.RebaseFromCamera(HeadCamera.transform.position, HeadCamera.transform.rotation);

            for (int round = 1; round <= RoundsPerStage; round++)
            {
                yield return StartCoroutine(RunOneRound(stage, round));
            }

            yield return new WaitForSeconds(InterStageDelay);
        }

        // 아웃트로
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리 위치 분별 훈련을 종료하겠습니다.", 0.4f, 3f, 0.8f));

        if (EnableLogging)
            Debug.Log("모든 스테이지 완료!");

        SceneLoader.Instance.LoadMain();
    }

    private IEnumerator RunOneRound(int stage, int round)
    {
        // 1) 구 배치
        _activeBalls = spawner.SpawnSet();
        ToggleInteractivity(_activeBalls, false); // 소리나기 전에는 선택할 수 없도록
        if (_activeBalls == null || _activeBalls.Count == 0)
        {
            Debug.LogError("[StageController] 스폰 실패");
            yield break;
        }

        AttachAndSubscribe(_activeBalls);

        yield return new WaitForSeconds(PreSoundDelay);

        // 2) 소리 발생
        _correctBall = PickRandomBall(_activeBalls);
        _correctBall.GetComponent<InteractiveSphere>()?.TriggerSound();
        ToggleInteractivity(_activeBalls, true);

        if (EnableLogging)
            Debug.Log($"[Round] Stage {stage} Round {round}: 소리 발생 - 정답 구 {_correctBall.name}");

        _selectedBall = null;
        _isAwaitingSelection = true;
        _isShowingWrongMsg = false;

        // 힌트(하이라이트)
        if (HighlightDelay > 0f)
            _hintCo = StartCoroutine(HintAfterDelay(_correctBall, HighlightDelay));

        // 정답을 선택할 때까지 대기
        while (_isAwaitingSelection)
            yield return null;

        // 힌트 코루틴 정리
        if (_hintCo != null) { StopCoroutine(_hintCo); _hintCo = null; }

        // 3) 정답 피드백
        UIPanel.SetActive(true);
        bool isLastRound = (stage == LastStage) && (round == RoundsPerStage);
        string msg = isLastRound ? "정답입니다!" : "정답입니다! 다음 문제가 곧 진행됩니다.";

        if (EnableLogging)
            Debug.Log("[Round] 결과: 정답");

        yield return StartCoroutine(ShowFade(UIPanel, UIText, msg, 0.2f, 3f, 0.3f));

        // 4) 정리
        UIPanel.SetActive(false);
        CleanupBalls();
    }

    private IEnumerator HintAfterDelay(GameObject correct, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_isAwaitingSelection && correct)
        {
            correct.GetComponent<InteractiveSphere>()?.OnMarkTimeOver();
            if (EnableLogging)
                Debug.Log("[Round] 정답 구 빛남(힌트)");
        }
    }

    private GameObject PickRandomBall(List<GameObject> balls)
    {
        int idx = Random.Range(0, balls.Count);
        return balls[idx];
    }

    private void AttachAndSubscribe(List<GameObject> balls)
    {
        foreach (var go in balls)
        {
            var sphere = go.GetComponent<InteractiveSphere>();
            if (sphere == null) continue;
            // 상태 변경 이벤트(StateChanged) 구독
            System.Action<InteractiveSphere.SphereState> handler = null;
            handler = (newState) =>
            {
                if (newState == InteractiveSphere.SphereState.Touched)
                    OnSphereSelected(sphere);
            };

            sphere.StateChanged += handler;
            _stateHandlers[sphere] = handler;
        }
    }

    private void OnSphereSelected(InteractiveSphere sphere)
    {
        if (!_isAwaitingSelection || sphere == null) return;

        var go = sphere.gameObject;
        if (go == _correctBall)
        {
            _selectedBall = go;
            sphere.OnCorrect();
            _isAwaitingSelection = false;
            return;
        }

        // 오답: UI 메시지 + 구 제거
        if (_stateHandlers.TryGetValue(sphere, out var handler))
        {
            sphere.StateChanged -= handler;
            _stateHandlers.Remove(sphere);
        }
        _activeBalls.Remove(go);

        sphere.MarkWrongAndVanish(0.15f, 2f);

        if (!_isShowingWrongMsg)
            StartCoroutine(ShowWrongOnce());
    }

    private IEnumerator ShowWrongOnce()
    {
        _isShowingWrongMsg = true;
        UIPanel.SetActive(true);
        yield return StartCoroutine(ShowFade(UIPanel, UIText, "실패하였습니다. 다른 공을 선택해주세요.", 0.2f, 1.2f, 0.25f));
        UIPanel.SetActive(false);
        _isShowingWrongMsg = false;
    }

    private void CleanupBalls()
    {
        foreach (var go in _activeBalls)
        {
            var sphere = go ? go.GetComponent<InteractiveSphere>() : null;
            if (sphere != null && _stateHandlers.TryGetValue(sphere, out var handler))
            {
                sphere.StateChanged -= handler;
                _stateHandlers.Remove(sphere);
            }
            if (go) Destroy(go);
        }

        _activeBalls.Clear();
        _correctBall = null;
        _selectedBall = null;
        _isAwaitingSelection = false;
        _isShowingWrongMsg = false;
    }

    private IEnumerator ShowFade(GameObject panel, TextMeshProUGUI text, string message, float fadeIn, float hold, float fadeOut)
    {
        if (!panel || !text) yield break;

        var cg = panel.GetComponent<CanvasGroup>();
        if (!cg) cg = panel.AddComponent<CanvasGroup>();

        panel.SetActive(true);
        text.text = message;

        yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, fadeIn));
        if (hold > 0f) yield return new WaitForSeconds(hold);
        yield return StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, fadeOut));

        panel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f) { cg.alpha = to; yield break; }
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private void ToggleInteractivity(List<GameObject> balls, bool on)
    {
        foreach (var go in balls)
        {
            if (!go) continue;
            var col = go.GetComponent<Collider>();
            if (col) col.enabled = on;

            var grab = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab) grab.enabled = on;
        }
    }
}
