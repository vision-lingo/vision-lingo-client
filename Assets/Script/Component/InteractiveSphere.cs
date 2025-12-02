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
    private Coroutine _volumeBoostCoroutine;
    private float _originalMasterVolume; // To store master volume before the sound starts
    private ParticleController _particleController;


    [Header("Random Audio Loop")]
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float baseVolume = 1.0f; // Default volume (adjusted by master volume)


    [Header("Effects")]
    [SerializeField] private GameObject correctEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    [SerializeField] private GameObject _waveEffect;
    [SerializeField] private float _scaleFactor;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _failColor;
    [SerializeField] private Color _hoverColor;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _timeOverColor;
    [SerializeField] private Color _wrongGray;

    [SerializeField] private bool isHoverable = true;
    [SerializeField] private bool _isTutorial = false;

    // 에디터/PC 테스트용 마우스 클릭 허용
    [SerializeField] private bool enableMouseClick = true;

    private IEnumerator IE_SequenceChangeColor_Handle = null;

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

    bool IXRHeadInteractable.IsInteractable { get => !_isTutorial; set => _isTutorial = value; }

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
        if(IE_SequenceChangeColor_Handle != null)
            StopCoroutine(IE_SequenceChangeColor_Handle);
        IE_SequenceChangeColor_Handle = null;
        if(_volumeBoostCoroutine != null)
            StopCoroutine(_volumeBoostCoroutine);
        _volumeBoostCoroutine = null;
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

    // 마우스 클릭으로 선택
    private void OnMouseDown()
    {
        if (!enableMouseClick) return;

        if (currentState == SphereState.Default 
            || currentState == SphereState.SoundTriggered
            || currentState == SphereState.TimeOver)
        {
            OnTouched();
        }
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

    public void OnSelect()
    {
        OnTouched();
    }

    public void OnCorrect()
    {
        SetState(SphereState.Correct);
        ChangeColor(_correctColor, 2f);
        StopSound();

        Vector3 spawnPos = effectSpawnPoint ? effectSpawnPoint.position : transform.position;
        var fx = Instantiate(correctEffectPrefab, spawnPos, Quaternion.identity);
        if(!fx.TryGetComponent(out _particleController)) 
            MainSystem.Instance.Loggers.LogError("InteractiveSphere", "OnCorrect", $"_particleController is null");
        Destroy(fx, 5f);
    }

    public void OnTouched() 
    {
        SetState(SphereState.Touched);
    }

    public void TriggerSound()
    {
        SetState(SphereState.SoundTriggered);

        // Store the current master volume before the sound starts
        _originalMasterVolume = MainSystem.Instance.SoundController.CurrMaxsterVolume;

        var pick = UnityEngine.Random.Range(0, clips.Length);
        _audioSource.loop = true;
        _audioSource.clip = clips[pick];
        // CHECK: 왜 베이스 볼륨을 쓰는거지?
        // 이 오브젝트에 대한 볼륨
        _audioSource.volume = baseVolume; // Start with base volume
        _audioSource.Play();

        // Start increasing volume over time
        if (_volumeBoostCoroutine != null)
            StopCoroutine(_volumeBoostCoroutine);
        _volumeBoostCoroutine = StartCoroutine(VolumeBoostOverTime());
    }

    private IEnumerator VolumeBoostOverTime()
    {
        // 1-5 seconds: maintain user settings
        float timer = 0f;
        while (timer < 5f)
        {
            if (!MainSystem.Instance.IsPause)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            // 6-10 seconds: master volume x3
            MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume * 3f);
        }
        
        // Wait 5 more seconds (total 10 seconds)
        timer = 0f;
        while (timer < 5f)
        {
            if (!MainSystem.Instance.IsPause)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            // After 11 seconds: master volume x5
            MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume * 5f);
        }
    }

    public void OnResume()
    {
        _audioSource.Play();
        if(_particleController != null)
            _particleController.ResumeParticles();   
    }

    public void OnPause()
    {
        _audioSource.Pause();
        if(_particleController != null)
            _particleController.PauseParticles();   
    }

    public void StopSound()
    {
        // Stop the volume boost coroutine
        if (_volumeBoostCoroutine != null)
        {
            StopCoroutine(_volumeBoostCoroutine);
            _volumeBoostCoroutine = null;
        }

        // Restore master volume to its original value
        MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume);

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.loop = false;
        }
    }

    public void MarkWrong()
    {
        SetState(SphereState.Wrong);
        ChangeColor(_failColor);
    }

    public void MarkWrongAndVanish(float vanishDuration = 0.35f, float delay = 0f)
    {
        SetState(SphereState.Wrong);
        isHoverable = false;

        ChangeColor(_wrongGray);
        DisableInteractivity();

        StartCoroutine(IE_Vanish(vanishDuration, delay));
    }

    public void DisableInteractivity()
    {
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (_grab) _grab.enabled = false;
    }

    private IEnumerator IE_Vanish(float dur, float delay)
    {
        // 요청한 지연 시간만큼 대기 (보이는 상태로)
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

        float t = 0f;
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.zero;

        while (t < dur)
        {
            if (!MainSystem.Instance.IsPause)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                transform.localScale = Vector3.Lerp(start, end, k);
            }
            
            yield return null;
        }

        Destroy(gameObject);
    }

    public void OnMarkTimeOver()
    {
        SetState(SphereState.TimeOver);
        Color[] subColors = null;
        float[] subTimes = null;
        if(_isTutorial)
        {
            subColors = new Color[]{_timeOverColor * 10};
            subTimes = new float[] {5.0f};
        }
        else
        {
            subColors = new Color[]{_timeOverColor * 10, _timeOverColor * 30, _timeOverColor * 50};
            subTimes = new float[] {5.0f, 5.0f, 10.0f};
        }
        
        SequenceChangeColor(_timeOverColor, subColors, subTimes);
    }
    
    public void ResetToDefault()
    {
        SetState(SphereState.Default);
        ChangeColor(_defaultColor);
    }

    public void ChangeColor(Color color, float intensity = 1)
    {
        Color finalColor = color * intensity;
        _meshRenderer.material.SetColor("_emission", finalColor);
    }

    private void SequenceChangeColor(Color startColor, Color[] endColor, float[] maxTime)
    {
        if(IE_SequenceChangeColor_Handle != null)
            return;
        StartCoroutine(IE_SequenceChangeColor_Handle = IE_SequenceChangeColor(startColor, endColor, maxTime));
    }

    private IEnumerator IE_SequenceChangeColor(Color startColor, Color[] endColor, float[] subTime)
    {
        Color finalColor = startColor;
        Color afterColor = startColor;
        int endColorLength = endColor.Length;
        int subTimeLength = subTime.Length;
        MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"subTimeLength: {subTimeLength}");
        for(int i = 0; i < subTimeLength; i++)
        {
            float currTime = 0.0f;
            MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"subTime[{i}]: {subTime[i]}");
            MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"endColor[{i}]: {endColor[i]}");
            while(subTime[i] > currTime)
            {
                if(!MainSystem.Instance.IsPause)
                    currTime += Time.deltaTime;
                yield return null;
                finalColor = afterColor + endColor[i] * (currTime/subTime[i]);
                _meshRenderer.material.SetColor("_emission", finalColor);
                //MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"{i}_finalColor: {finalColor}");
            }
            afterColor = finalColor;
            MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"end[{i}]");
        }
        MainSystem.Instance.Loggers.LogInfo("InteractiveSphere", "IE_SequenceChangeColor", $"end");
        _meshRenderer.material.SetColor("_emission", endColor[endColorLength - 1]);
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