using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.Animations;

public class InteractiveSphere : MonoBehaviour, IXRHeadInteractable
{
    public enum SphereState
    {
        None = -1,
        Default = 0,
        Wave = 1, // only tutorial
        SoundTriggered = 2,
        //Hover = 3, // hover는 상태로 두면 안될 듯.
        Correct = 3,
        Touched = 4,
        Wrong = 5,
        TimeOver = 6
    }

    [SerializeField]
    private SphereState currentState = SphereState.Default;

    [SerializeField]
    private MeshRenderer _meshRenderer;
    private Material _mat;
    
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;

    private AudioSource _audioSource;

    [Header("Random Audio Loop")]
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private GameObject _waveEffect;
    [SerializeField] private float _scaleFactor;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _failColor;
    [SerializeField] private Color _hoverColor;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _timeOverColor;
    
    [SerializeField] private bool isHoverable = true;
    private float _hoverScale;
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

    private void Awake()
    {
        if (_grab == null) _grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>(); 
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        _hoverScale = transform.localScale.x * _scaleFactor;
    }

    private void OnEnable()
    {
        if (_grab != null)
            _grab.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        Debug.Log("Sphere::::OnDisable");
        if (_grab != null)
            _grab.selectEntered.RemoveListener(OnSelectEntered);
        
        if(CurrentState == SphereState.Wave)
            OffWaveEffect();
        else
            ResetToDefault();
        
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        OnTouched();
    }

    private void Start()
    {
        _mat = _meshRenderer.material;
        ChangeColor(_defaultColor);
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

    public void OnWave()
    {
        SetState(SphereState.Wave);
        _waveEffect.SetActive(true);
    }
    public void OffWaveEffect()
    {
        //SetState(SphereState.Default);
        ResetToDefault();
        _waveEffect.SetActive(false);
    }


    public void OnRayOver()
    {
        if(!isHoverable) return;
        // default 상태일 때만 호버 가능
        if(currentState == SphereState.Default || currentState == SphereState.SoundTriggered)
        {
            transform.localScale += Vector3.one * _hoverScale;
            ChangeColor(_hoverColor);
        }
    }

    public void OnRayOut()
    {
        if(!isHoverable) return;
        if(currentState == SphereState.Default || currentState == SphereState.SoundTriggered)
        {
            transform.localScale -= Vector3.one * _hoverScale;
            ChangeColor(_defaultColor);
        }
    }

    public void OnCorrect()
    {
        SetState(SphereState.Correct);
        ChangeColor(_correctColor, 2f);
    }

    public void OnTouched() 
    {
        SetState(SphereState.Touched);
        //GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void TriggerSound()
    {
        SetState(SphereState.SoundTriggered);
        
        var pick = UnityEngine.Random.Range(0, clips.Length);
        _audioSource.loop = true;
        _audioSource.clip = clips[pick];
        _audioSource.Play();
    }

    public void MarkWrong()
    { 
        SetState(SphereState.Wrong);
        ChangeColor(_failColor);
    }

    public void OnMarkTimeOver()
    {
        SetState(SphereState.TimeOver);
        SequenceChangeColor(_timeOverColor, _timeOverColor * 10, 2);
        //ChangeColor(_timeOverColor, 10);
    }
    

    public void ResetToDefault()
    {
        //GetComponent<MeshRenderer>().material.color = Color.gray;
        SetState(SphereState.Default);
        ChangeColor(_defaultColor);
    }

    public void ChangeColor(Color color, float intensity = 1)
    {
        Color finalColor = color * intensity;
        _meshRenderer.material.SetColor("_emission", finalColor);
    }

    private void SequenceChangeColor(Color startColor, Color endColor, float maxTime)
    {
        StartCoroutine(IE_SequenceChangeColor(startColor, endColor, maxTime));
    }

    /// <summary>
    /// intensity 곱한 값을 넣어줘야 합니다.
    /// </summary>
    /// <param name="startColor"></param>
    /// <param name="endColor"></param>
    /// <returns></returns>
    private IEnumerator IE_SequenceChangeColor(Color startColor, Color endColor, float maxTime)
    {
        float currTime = 0.0f;
        Color finalColor = startColor;
        while(maxTime > currTime)
        {
            yield return null;
            currTime += Time.deltaTime;
            finalColor = endColor * (currTime/maxTime);
            _meshRenderer.material.SetColor("_emission", finalColor);
        }
        _meshRenderer.material.SetColor("_emission", endColor);
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
    private void TestTimeOver() => OnMarkTimeOver();

    [ContextMenu("Debug/Log Current State")]
    private void DebugLogState() => Debug.Log($"[InteractiveSphere] Current State: {CurrentState}");

    [ContextMenu("Debug/Trigger State Changed Event")]
    private void DebugTriggerEvent() => OnStateChanged(currentState);

#endif
}