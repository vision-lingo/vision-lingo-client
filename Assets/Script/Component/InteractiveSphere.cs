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

    private void OnStateChanged(SphereState newState)
    {
        Debug.Log($"[InteractiveSphere] State changed to: {newState}");
        StateChanged?.Invoke(newState);
        
        // TODO: 상태별 시각/사운드 처리
        // ApplyVisualEffects(newState);
        // PlayStateSound(newState);
    }

    public void SetState(SphereState newState) => CurrentState = newState;

    public void OnTouched() => SetState(SphereState.Touched);

    public void TriggerSound() => SetState(SphereState.SoundTriggered);

    public void MarkWrong() => SetState(SphereState.Wrong);

    public void MarkTimeOver() => SetState(SphereState.TimeOver);

    public void ResetToDefault() => SetState(SphereState.Default);

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