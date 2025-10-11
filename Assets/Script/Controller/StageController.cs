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

    [Header("Flow Settings")]
    [Tooltip("시작 스테이지 번호")]
    public int FirstStage = 1;

    [Tooltip("마지막 스테이지 번호")]
    public int LastStage = 6;

    [Tooltip("각 스테이지별 라운드 수")]
    public int RoundsPerStage = 6;

    [Header("Timings (sec)")]
    [Tooltip("안내 UI 유지시간 (소리 전까지)")]
    public float PreSoundDelay = 4f;

    [Tooltip("소리 발생 후 빛나기까지의 지연")]
    public float HighlightDelay = 10f;

    [Tooltip("하이라이트 후 선택 제한시간")]
    public float AnswerTimeout = 10f;

    [Tooltip("피드백 유지시간")]
    public float FeedbackHold = 2f;

    [Tooltip("스테이지 사이 대기시간")]
    public float InterStageDelay = 1.0f;

    [Header("Debug / Log")]
    [Tooltip("디버그 로그 출력 여부")]
    public bool EnableLogging = true;

    private List<GameObject> _activeBalls = new List<GameObject>();
    private GameObject _correctBall = null;

    void Start()
    {
        if (!HeadCamera) HeadCamera = Camera.main;
        if (!spawner || !HeadCamera || !UIPanel || !UIText)
        {
            Debug.LogError("[StageController] 레퍼런스가 비었습니다.");
            enabled = false;
            return;
        }

        // 진행 시작
        StartCoroutine(RunAllStages());
    }

    private IEnumerator RunAllStages()
    {
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

        if (EnableLogging)
            Debug.Log("모든 스테이지 완료!");
    }

    private IEnumerator RunOneRound(int stage, int round)
    {
        // 1) 안내 UI 표시 + 구 배치
        UIPanel.SetActive(true);
        UIText.text = $"스테이지 {stage}, 라운드 {round}\n소리가 나는 공을 선택해 주세요.";

        _activeBalls = spawner.SpawnSet();
        if (_activeBalls == null || _activeBalls.Count == 0)
        {
            Debug.LogError("[StageController] 스폰 실패");
            yield break;
        }

        yield return new WaitForSeconds(PreSoundDelay);

        // 2) 소리 발생 (UI 숨김)
        UIPanel.SetActive(false);
        _correctBall = PickRandomBall(_activeBalls);
        _correctBall.GetComponent<InteractiveSphere>()?.TriggerSound();

        if (EnableLogging)
            Debug.Log($"[Round] Stage {stage} Round {round}: 소리 발생 - 정답 구 {_correctBall.name}");

        // 3) 10초 후 빛남
        yield return new WaitForSeconds(HighlightDelay);
        _correctBall.GetComponent<InteractiveSphere>()?.MarkTimeOver();
        //_correctBall.GetComponent<TargetBall>()?.Highlight();

        if (EnableLogging)
            Debug.Log($"[Round] Stage {stage} Round {round}: 정답 구 빛남");

        // 4) 입력 대기 (현재는 랜덤 시뮬레이션)
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        bool isCorrect = Random.value > 0.5f;

        // 5) 피드백
        UIPanel.SetActive(true);
        UIText.text = isCorrect ? "맞았습니다!" : "틀렸습니다!";

        if (EnableLogging)
            Debug.Log($"[Round] 결과: {(isCorrect ? "정답" : "오답")}");

        yield return new WaitForSeconds(FeedbackHold);

        // 6) 정리
        UIPanel.SetActive(false);
        CleanupBalls();
    }

    private GameObject PickRandomBall(List<GameObject> balls)
    {
        int idx = Random.Range(0, balls.Count);
        return balls[idx];
    }

    private void CleanupBalls()
    {
        foreach (var go in _activeBalls)
        {
            if (go) Destroy(go);
        }
        _activeBalls.Clear();
        _correctBall = null;
    }
}
