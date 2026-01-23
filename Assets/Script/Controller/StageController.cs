using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;


public class StageController : MonoBehaviour
{

    private enum RoundState
    {
        Complete = 0,
        Current = 1,
        Remain = 2
    }
    [Serializable]
    private class TrainingRound
    {
        [SerializeField] private TextMeshProUGUI _text_currRound;
        [SerializeField] private Image[] _img_roundBg;
        [SerializeField] private TextMeshProUGUI[] _txt_round;
        [SerializeField] private Image[] _img_checkIcon;
        [SerializeField] private Color _currColor;
        [SerializeField] private Color _completeColor;
        [SerializeField] private Color _remainColor;

        public void Init()
        {
            _text_currRound.text = "1";
            _img_roundBg[0].color = _currColor;
            _txt_round[0].gameObject.SetActive(true);
            _img_checkIcon[0].gameObject.SetActive(false);
        }
        public void SetUI(int currIdx)
        {
            int length = _img_roundBg.Length;

            _text_currRound.text = (currIdx + 1).ToString();
            _img_roundBg[currIdx].color = _currColor;

            // 이전
            for(int i = currIdx - 1; i >= 0; i--)
            {
                _txt_round[i].gameObject.SetActive(false);
                _img_checkIcon[i].gameObject.SetActive(true);
                _img_roundBg[i].color = _completeColor;
            }
            // 다음
            for(int i = currIdx + 1; i < length; i++)
            {
                _txt_round[i].gameObject.SetActive(true);
                _img_roundBg[i].color = _remainColor;
            }
        }
    }

    [Header("Round 표시_CY")]
    [SerializeField] private TrainingRound _trainingRound;


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

    [Header("Round Progress UI")]
    public Transform RoundProgressContainer;
    public GameObject RoundDotPrefab;
    public Color FilledColor = new Color(0.2f, 0.8f, 0.4f);
    public Color EmptyColor = new Color(0.85f, 0.85f, 0.85f);

    private List<GameObject> _roundDots = new List<GameObject>();

    private List<GameObject> _activeBalls = new List<GameObject>();
    private GameObject _correctBall = null;
    private GameObject _selectedBall = null;
    private bool _isAwaitingSelection = false;
    private bool _isShowingWrongMsg = false;

    private int _totalRounds;
    private int _roundIndex;

    private Coroutine _hintCo;
    private Coroutine _wrongMsgCoroutine;

    private readonly Dictionary<InteractiveSphere, System.Action<InteractiveSphere.SphereState>> _stateHandlers
        = new Dictionary<InteractiveSphere, System.Action<InteractiveSphere.SphereState>>();

    // Firebase 메트릭 전송
    private int _wrongAttemptsThisRound = 0;   // 실패 횟수 (정답을 바로 맞추면 0)
    private float _roundStartTime = 0f;        // 소리 재생 시각(Time.time)
    private int _currentStage = 0;
    private int _currentRound = 0;
    private string _currentSessionId = string.Empty; // 스테이지 하나당 새로 생성

    void Start()
    {
        if (!HeadCamera) HeadCamera = Camera.main;
        if (!spawner || !HeadCamera || !UIPanel || !UIText)
        {
            MainSystem.Instance.Loggers.LogError("StageController", "Start", "References are missing.");
            enabled = false;
            return;
        }

        InitRoundProgressUI();

        UIPanel.SetActive(false);
        IntroPanel.SetActive(false);
        StartCoroutine(RunAllStages());
    }

    private void OnEnable()
    {
        MainSystem.Instance.Act_Pause += OnPause;
        MainSystem.Instance.Act_Resume += OnResume;
    }

    private void OnDisable()
    {
        MainSystem.Instance.Act_Pause -= OnPause;
        MainSystem.Instance.Act_Resume -= OnResume;
    }

    private void OnPause()
    {
        if (_correctBall != null)
            _correctBall.GetComponent<InteractiveSphere>()?.OnPause();
    }

    private void OnResume()
    {
        if (_correctBall != null)
            _correctBall.GetComponent<InteractiveSphere>()?.OnResume();
    }

    private IEnumerator RunAllStages()
    {
        // 인트로
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리 위치 분별 훈련을 시작하겠습니다.", 0.4f, 3f, 0.8f));
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리가 나는 공을 찾아 선택해 주세요.", 0.4f, 3f, 0.8f));

        ShowRoundProgress(update: true);
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, $"훈련은 총 {_totalRounds} 라운드입니다.", 0.4f, 3f, 0.8f));
        HideRoundProgress();

        // 스테이지 루프
        for (int stage = FirstStage; stage <= LastStage; stage++)
        {
            _currentStage = stage;

            spawner.SetStage(stage);
            spawner.RebaseFromCamera(HeadCamera.transform.position, HeadCamera.transform.rotation);

            for (int round = 1; round <= RoundsPerStage; round++)
            {
                _currentRound = round;
                yield return StartCoroutine(RunOneRound(stage, round));
            }

            if (InterStageDelay > 0f)
            {
                float timer = 0f;
                while (timer < InterStageDelay)
                {
                    if (!MainSystem.Instance.IsPause)
                    {
                        timer += Time.deltaTime;
                    }
                    yield return null;
                }
            }
        }

        // 아웃트로
        ShowRoundProgress(update: true);
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "모든 훈련이 끝났습니다.", 0.4f, 3f, 0.8f));

        HideRoundProgress();
        yield return StartCoroutine(ShowFade(IntroPanel, IntroText, "소리 위치 분별 훈련을 종료하겠습니다.", 0.4f, 3f, 0.8f));

        if (EnableLogging)
        if (EnableLogging)
            MainSystem.Instance.Loggers.LogInfo("StageController", "RunAllStages", "All stages completed!");

        SceneLoader.Instance.LoadLobby();
    }

    private IEnumerator RunOneRound(int stage, int round)
    {
        // 라운드 시작 시 UI 정리
        UIPanel.SetActive(false);
        _isShowingWrongMsg = false;
        if (_wrongMsgCoroutine != null)
        {
            StopCoroutine(_wrongMsgCoroutine);
            _wrongMsgCoroutine = null;
        }

        // 1) 구 배치
        _activeBalls = spawner.SpawnSet();
        ToggleInteractivity(_activeBalls, false); // 소리나기 전에는 선택할 수 없도록
        if (_activeBalls == null || _activeBalls.Count == 0)
        {
            MainSystem.Instance.Loggers.LogError("StageController", "RunOneRound", "Spawn failed");
            yield break;
        }
        _wrongAttemptsThisRound = 0;

        AttachAndSubscribe(_activeBalls);

        if (PreSoundDelay > 0f)
        {
            float timer = 0f;
            while (timer < PreSoundDelay)
            {
                if (!MainSystem.Instance.IsPause)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }

        // 2) 소리 발생
        _correctBall = PickRandomBall(_activeBalls);
        _correctBall.GetComponent<InteractiveSphere>()?.TriggerSound();
        ToggleInteractivity(_activeBalls, true);
        _roundStartTime = Time.time;

        if (EnableLogging)
            MainSystem.Instance.Loggers.LogInfo("StageController", "RunOneRound", $"[Round] Stage {stage} Round {round}: Sound Triggered - Correct Sphere {_correctBall.name}");

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
            MainSystem.Instance.Loggers.LogInfo("StageController", "RunOneRound", "[Round] Result: Correct");
        ShowRoundProgress(update: true);

        yield return StartCoroutine(ShowFade(UIPanel, UIText, msg, 0.2f, 3f, 0.3f));

        // 4) 정리
        CleanupBalls();
    }

    private IEnumerator HintAfterDelay(GameObject correct, float delay)
    {
        if (delay > 0f)
        {
            float timer = 0f;
            while (timer < delay)
            {
                if (!MainSystem.Instance.IsPause)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }
        if (_isAwaitingSelection && correct)
        {
            correct.GetComponent<InteractiveSphere>()?.OnMarkTimeOver();
            if (EnableLogging)
                MainSystem.Instance.Loggers.LogInfo("StageController", "HintAfterDelay", "[Round] Correct sphere glowing (Hint)");
        }
    }

    private GameObject PickRandomBall(List<GameObject> balls)
    {
        int idx = UnityEngine.Random.Range(0, balls.Count);
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
            // 실패 UI가 돌고 있으면 먼저 정리
            if (_wrongMsgCoroutine != null)
            {
                StopCoroutine(_wrongMsgCoroutine);
                _wrongMsgCoroutine = null;
            }
            _isShowingWrongMsg = false;
            UIPanel.SetActive(false);

            _selectedBall = go;
            sphere.OnCorrect();
            _isAwaitingSelection = false;

            float timeToCorrect = Time.time - _roundStartTime;

            _roundIndex++;
            //UpdateRoundDots();
            _trainingRound.SetUI(_roundIndex);

            return;
        }

        _wrongAttemptsThisRound += 1;

        if (_stateHandlers.TryGetValue(sphere, out var handler))
        {
            sphere.StateChanged -= handler;
            _stateHandlers.Remove(sphere);
        }
        _activeBalls.Remove(go);

        sphere.MarkWrongAndVanish(0.15f, 2f);

        if (!_isShowingWrongMsg)
            _wrongMsgCoroutine = StartCoroutine(ShowWrongOnce());
    }

    private IEnumerator ShowWrongOnce()
    {
        _isShowingWrongMsg = true;
        UIPanel.SetActive(true);
        yield return StartCoroutine(ShowFade(UIPanel, UIText, "실패하였습니다. 다른 공을 선택해주세요.", 0.2f, 1.2f, 0.25f));

        UIPanel.SetActive(false);
        _isShowingWrongMsg = false;
        _wrongMsgCoroutine = null;
    }

    private void CleanupBalls()
    {
        // 힌트/실패 메시지 코루틴 정리
        if (_hintCo != null)
        {
            StopCoroutine(_hintCo);
            _hintCo = null;
        }
        if (_wrongMsgCoroutine != null)
        {
            StopCoroutine(_wrongMsgCoroutine);
            _wrongMsgCoroutine = null;
        }
        _isShowingWrongMsg = false;
        UIPanel.SetActive(false);

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

        yield return FadeCanvasGroup(cg, cg.alpha, 1f, fadeIn);

        if (hold > 0f)
        {
            float timer = 0f;
            while (timer < hold)
            {
                if (!MainSystem.Instance.IsPause)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }

        yield return FadeCanvasGroup(cg, 1f, 0f, fadeOut);

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

    private void InitRoundProgressUI()
    {
        _totalRounds = (LastStage - FirstStage + 1) * RoundsPerStage;
        _roundIndex = 0;

        //BuildRoundDotsUI();
        //UpdateRoundDots();
        _trainingRound.Init();
        //RoundProgressContainer.gameObject.SetActive(false);
    }

    private void ShowRoundProgress(bool update = true)
    {
        if (update)
        {
            _trainingRound.SetUI(_roundIndex);
            //UpdateRoundDots();
        }

        //RoundProgressContainer.gameObject.SetActive(true);
    }

    private void HideRoundProgress()
    {
        RoundProgressContainer.gameObject.SetActive(false);
    }

    private void BuildRoundDotsUI()
    {
        // 기존 것 제거
        foreach (var dot in _roundDots)
            Destroy(dot);
        _roundDots.Clear();

        for (int i = 0; i < _totalRounds; i++)
        {
            var dot = Instantiate(RoundDotPrefab, RoundProgressContainer);
            var img = dot.GetComponent<UnityEngine.UI.Image>();
            img.color = EmptyColor;

            _roundDots.Add(dot);
        }
    }

    private void UpdateRoundDots()
    {
        for (int i = 0; i < _roundDots.Count; i++)
        {
            var img = _roundDots[i].GetComponent<UnityEngine.UI.Image>();
            img.color = (i < _roundIndex) ? FilledColor : EmptyColor;
        }
    }
}
