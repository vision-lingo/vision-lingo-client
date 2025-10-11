using UnityEngine;
using System;

public class InteractiveSphere : MonoBehaviour
{
    public enum SphereState
    {
        Default,
        SoundTriggered,
        Touched,
        Wrong,
        TimeOver
    }

    [SerializeField]
    private SphereState currentState = SphereState.Default;
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private MeshRenderer _meshRenderer;
    private Material _mat;


    public SphereState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState == value) return;
            
            currentState = value;
            OnStateChanged(currentState);
        }
    }

    public event Action<SphereState> StateChanged;

    private void Start()
    {
        _mat = _meshRenderer.material;
    }
    private void OnStateChanged(SphereState newState)
    {
        Debug.Log($"[InteractiveSphere] State changed to: {newState}");
        StateChanged?.Invoke(newState);

        // TODO: 상태별 시각/사운드 처리
        // ApplyVisualEffects(newState);
        // PlayStateSound(newState);
    }

    public void SetState(SphereState newState) => CurrentState = newState;

    public void OnTouched() {
        SetState(SphereState.Touched);
        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void TriggerSound()
    {
        SetState(SphereState.SoundTriggered);
        _audioSource.Play();
    }

    public void MarkWrong() => SetState(SphereState.Wrong);

    public void MarkTimeOver()
    {
        SetState(SphereState.TimeOver);
        SetEmission(true, 100);
    }

    public void ResetToDefault()
    {
        GetComponent<MeshRenderer>().material.color = Color.gray;
        SetState(SphereState.Default);
    }
    public void SetEmission(bool enable, float intensity)
    {
        if (enable)
        {
            // Emission 활성화
            _mat.EnableKeyword("_EMISSION");

            // 색상 * 강도를 Emission Color 속성에 설정
            Color finalColor = Color.red * intensity;
            _mat.SetColor("_EmissionColor", finalColor);
        }
        else
        {
            // Emission 비활성화
            _mat.DisableKeyword("_EMISSION");
            // 또는 강도를 0으로 설정하여 시각적으로 끄는 방법도 가능
            // material.SetColor(emissionColorID, Color.black);
        }
    }

#if UNITY_EDITOR
    private SphereState lastInspectorState;

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        
        if (lastInspectorState != currentState)
        {
            lastInspectorState = currentState;
            SetState(currentState);
        }
    }

    [ContextMenu("Test/Reset to Default")]
    private void TestDefault() => ResetToDefault();

    [ContextMenu("Test/Trigger Sound")]
    private void TestTriggerSound() => TriggerSound();

    [ContextMenu("Test/Touch Sphere")]
    private void TestTouched() => OnTouched();

    [ContextMenu("Test/Mark as Wrong")]
    private void TestWrong() => MarkWrong();

    [ContextMenu("Test/Mark as TimeOver")]
    private void TestTimeOver() => MarkTimeOver();

    [ContextMenu("Debug/Log Current State")]
    private void DebugLogState() => Debug.Log($"[InteractiveSphere] Current State: {CurrentState}");

    [ContextMenu("Debug/Trigger State Changed Event")]
    private void DebugTriggerEvent() => OnStateChanged(currentState);
#endif
}