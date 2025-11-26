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

    // 런타임으로 원형(링) 알파 텍스처를 생성합니다. 폴백 셰이더에서 사용.
    private Texture2D GenerateRadialTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] cols = new Color32[width * height];

        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float maxDist = Mathf.Min(width, height) * 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / maxDist; // 0..1

                // 4색 그라데이션: 중심에서 바깥으로 A -> B -> C -> D 보간
                Color ca = glowColorA;
                Color cb = glowColorB;
                Color cc = glowColorC;
                Color cd = glowColorD;

                Color col = Color.clear;
                float p1 = 0.33f;
                float p2 = 0.66f;
                if (d <= p1)
                {
                    float tt = Mathf.SmoothStep(0f, 1f, d / p1);
                    col = Color.Lerp(ca, cb, tt);
                }
                else if (d <= p2)
                {
                    float tt = Mathf.SmoothStep(0f, 1f, (d - p1) / (p2 - p1));
                    col = Color.Lerp(cb, cc, tt);
                }
                else
                {
                    float tt = Mathf.SmoothStep(0f, 1f, (d - p2) / (1f - p2));
                    col = Color.Lerp(cc, cd, tt);
                }

                // 링 알파: inner에서 강하고 outer에서 서서히 사라지도록
                float innerCut = 0.45f;
                float outerCut = 0.95f;
                float normalized = Mathf.Clamp01((d - innerCut) / Mathf.Max(0.0001f, (outerCut - innerCut)));
                // 기본적인 부드러운 감소 (중심을 더 진하게)
                float falloffExp = 1.6f; // 중심 강조
                float baseAlpha = Mathf.Pow(Mathf.Clamp01(1.0f - normalized), falloffExp);
                // 가우시안 기반의 엣지 소프트니스 (외곽을 부드럽게)
                float edgeSoftness = 1.8f;
                float gauss = Mathf.Exp(-normalized * normalized * edgeSoftness * 4.0f);
                float ringAlpha = baseAlpha * gauss;

                // 내측 레드 강조를 비활성화 (원래 상태로 되돌림)
                float nearRedBoost = 0.0f;
                float nearFactor = 0.0f;
                if (nearRedBoost > 0.0001f)
                    nearFactor = Mathf.Pow(Mathf.Clamp01(1.0f - normalized), Mathf.Max(0f, nearRedBoost));
                // 색상과 ColorA를 혼합 (nearFactor이 0이면 영향 없음)
                col = Color.Lerp(col, glowColorA * 1.0f, nearFactor);

                // 색상 강도와 알파 적용 (기본값으로 복원)
                float colorIntensity = 1.0f;
                float edgeAlphaMul = 1.0f;
                col.r *= ringAlpha * colorIntensity;
                col.g *= ringAlpha * colorIntensity;
                col.b *= ringAlpha * colorIntensity;
                col.a *= ringAlpha * edgeAlphaMul;

                Color32 outCol = new Color(col.r, col.g, col.b, col.a);
                cols[y * width + x] = outCol;
            }
        }

        tex.SetPixels32(cols);
        tex.Apply();
        return tex;
    }

    [SerializeField]
    private SphereState currentState = SphereState.Default;

    [SerializeField]
    private MeshRenderer _meshRenderer;
    private Material _mat;

    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
    
    private AudioSource _audioSource;
    private Coroutine _volumeBoostCoroutine;
    private float _originalMasterVolume; // 공 소리 시작 전 마스터 볼륨 저장
    private ParticleController _particleController;


    [Header("Random Audio Loop")]
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float baseVolume = 1.0f; // 기본 음량 (마스터 볼륨으로 조절)

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

    [Header("Glow Effect")]
    [SerializeField] private Shader glowShader; // optional assign in inspector
    [SerializeField] private float glowBaseScale = 1.0f; // base multiplier for glow size
    [SerializeField] private float glowQuadDistance = 0.01f; // offset from sphere surface so it doesn't z-fight
    [SerializeField] private Camera glowCamera; // 명시적으로 사용할 카메라 (지정되지 않으면 Camera.main 사용)
    [SerializeField] private float glowGlobalScale = 2.5f; // 전체 발광 확산 크기 조절 (기본 2.5x)

    // Colors for radial gradient (hex colors given by user)
    [SerializeField] private Color glowColorA = new Color(1f, 0.0f, 0.1725f, 1f); // #FF002C
    [SerializeField] private Color glowColorB = new Color(0.980f, 0.180f, 0.451f, 0.88f); // #FA2E73E0 ~ 88%
    [SerializeField] private Color glowColorC = new Color(0.984f, 0.372f, 0.749f, 0.67f); // #FB5FBFAB ~ 67%
    [SerializeField] private Color glowColorD = new Color(0.423f, 0.152f, 0.455f, 0.26f); // #6C277242 ~ 26%

    private GameObject _glowQuad; // 이전 쿼드 필드(호환성; 현재는 링 사용)
    private GameObject _glowRing;
    private MeshFilter _glowMeshFilter;
    private Material _glowMat;
    private Coroutine _glowCoroutine;

    [SerializeField] private bool isHoverable = true;

    // 에디터/PC 테스트용 마우스 클릭 허용
    [SerializeField] private bool enableMouseClick = true;

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

    private void CreateGlowQuadIfNeeded()
    {
        if (_glowRing != null) return;
        // 발광을 그릴 링 메쉬(원형 도넛 형태)를 생성합니다. 쿼드 대신 링을 사용하여 사각형 모서리 문제 제거
        Shader sh = glowShader != null ? glowShader : Shader.Find("Custom/GlowRadial");

        // 셰이더가 에디터에서는 보이지만 Game 뷰 또는 현재 렌더러에서 지원되지 않을 수 있음.
        // 지원 여부 확인 후 폴백 처리
        bool shaderOk = (sh != null && sh.isSupported);

        if (sh == null || !shaderOk)
        {
            Debug.LogWarning("글로우 셰이더를 찾을 수 없거나 지원되지 않습니다. 폴백 Unlit 텍스처 머티리얼을 사용합니다.");
            // 폴백용 Unlit 셰이더 찾기 (URP/기본 등 가능한 것부터 시도)
            Shader fallback = Shader.Find("Unlit/Transparent") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (fallback == null)
            {
                Debug.LogWarning("폴백 셰이더를 찾지 못했습니다. 발광이 비활성화됩니다.");
                return;
            }
            _glowMat = new Material(fallback);
            // 런타임으로 원형 알파 텍스처 생성하여 적용
            var tex = GenerateRadialTexture(256, 256);
            _glowMat.mainTexture = tex;
            // 주의: 폴백 머티리얼에는 전역 틴트 색을 적용하지 않습니다.
            // 텍스처 자체에 4색 그라데이션을 포함시켰으므로 색을 덮어쓰면 단색화됩니다.
        }
        else
        {
            _glowMat = new Material(sh);
            _glowMat.SetColor("_ColorA", glowColorA);
            _glowMat.SetColor("_ColorB", glowColorB);
            _glowMat.SetColor("_ColorC", glowColorC);
            _glowMat.SetColor("_ColorD", glowColorD);
        }

        // 링 오브젝트와 메쉬 필터/렌더러 생성
        _glowRing = new GameObject(gameObject.name + "_GlowRing");
        // 부모로 두지 않고 월드 위치/회전을 스크립트에서 직접 제어합니다 (스케일 상속 문제 방지)
        _glowRing.transform.SetParent(null);
        _glowMeshFilter = _glowRing.AddComponent<MeshFilter>();
        var mr = _glowRing.AddComponent<MeshRenderer>();
        mr.sharedMaterial = _glowMat;
        _glowRing.SetActive(false);

        // 초기 링 메쉬 생성 (작은 토막으로 시작)
        float sphereRadius = _meshRenderer != null ? _meshRenderer.bounds.extents.x : 0.5f;
        CreateOrUpdateRingMesh(sphereRadius * 1.02f, sphereRadius * 1.05f);
    }

    private void CreateOrUpdateRingMesh(float innerRadius, float outerRadius, int segments = 64)
    {
        if (_glowMeshFilter == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "GlowRingMesh";

        int vertCount = segments * 2;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] tris = new int[segments * 6];

        // 빌보드 기준 벡터 (오른쪽/업)은 카메라에 맞춰 스크립트에서 설정할 예정; 여기서는 로컬 XY 평면에 생성
        for (int i = 0; i < segments; i++)
        {
            float a = (float)i / segments * Mathf.PI * 2f;
            float ca = Mathf.Cos(a);
            float sa = Mathf.Sin(a);

            // 두 개의 링 버텍스 (inner, outer)
            Vector3 innerPos = new Vector3(ca * innerRadius, sa * innerRadius, 0f);
            Vector3 outerPos = new Vector3(ca * outerRadius, sa * outerRadius, 0f);
            verts[i * 2] = innerPos;
            verts[i * 2 + 1] = outerPos;

            // UVs: inner/outer을 outerRadius 기준으로 정규화하여 0.5 중심에서 반경 값으로 매핑
            // shader는 uv 중심(0.5,0.5) 기준으로 거리(dist)를 계산하므로 inner/outer이 다른 uv를 가져야 그라데이션이 생깁니다.
            float invOuter = 1f / outerRadius;
            uvs[i * 2] = new Vector2(innerPos.x * invOuter * 0.5f + 0.5f, innerPos.y * invOuter * 0.5f + 0.5f);
            uvs[i * 2 + 1] = new Vector2(outerPos.x * invOuter * 0.5f + 0.5f, outerPos.y * invOuter * 0.5f + 0.5f);
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int i0 = i * 2;
            int i1 = i * 2 + 1;
            int i2 = ((i + 1) % segments) * 2;
            int i3 = ((i + 1) % segments) * 2 + 1;

            // tri 1
            tris[ti++] = i0;
            tris[ti++] = i2;
            tris[ti++] = i1;
            // tri 2
            tris[ti++] = i1;
            tris[ti++] = i2;
            tris[ti++] = i3;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        _glowMeshFilter.sharedMesh = mesh;
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
        Debug.Log($"[인터랙티브스피어] 상태 변경: {newState}");
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

        // 공 소리 시작 전 현재 마스터 볼륨 저장
        _originalMasterVolume = MainSystem.Instance.SoundController.CurrMaxsterVolume;

        var pick = UnityEngine.Random.Range(0, clips.Length);
        _audioSource.loop = true;
        _audioSource.clip = clips[pick];
        _audioSource.volume = baseVolume; // 기본 음량으로 시작
        _audioSource.Play();

        // 시간별 음량 증가 시작
        if (_volumeBoostCoroutine != null)
            StopCoroutine(_volumeBoostCoroutine);
        _volumeBoostCoroutine = StartCoroutine(VolumeBoostOverTime());

        // Start glow expansion effect
        StartGlowEffect();
    }

    private IEnumerator VolumeBoostOverTime()
    {
        // 1~5초: 사용자 설정값 유지
        yield return new WaitForSeconds(5f);

        if (_audioSource != null && _audioSource.isPlaying)
        {
            // 6~10초: 마스터 볼륨 3배
            MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume * 3f);
        }

        yield return new WaitForSeconds(5f); // 5초 더 대기 (총 10초)

        if (_audioSource != null && _audioSource.isPlaying)
        {
            // 11초 이후: 마스터 볼륨 5배
            MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume * 5f);
        }
    }

    private void StartGlowEffect()
    {
        CreateGlowQuadIfNeeded();
        if (_glowRing == null) return;

        // 쿼드는 10초까지 비활성 상태로 유지(사용자 요구: 1-10초 표시 없음)
        // 코루틴에서 10초 후에 활성화합니다.
        if (_glowCoroutine != null)
            StopCoroutine(_glowCoroutine);
        _glowCoroutine = StartCoroutine(IE_GlowExpansion());

        Debug.Log("발광 이펙트 시작 요청: 게임 뷰에서 보이지 않을 경우 셰이더 호환성 또는 카메라 레이어 문제를 확인하세요.");
    }

    private void StopGlowEffect()
    {
        if (_glowCoroutine != null)
        {
            StopCoroutine(_glowCoroutine);
            _glowCoroutine = null;
        }

        if (_glowRing != null)
        {
            _glowRing.SetActive(false);
            if (_glowMeshFilter != null)
                _glowMeshFilter.sharedMesh = null;
        }
    }

    private IEnumerator IE_GlowExpansion()
    {
        // 사용자 요청 타임라인을 따릅니다:
        // 1-10초: 발광 확장 표시 없음
        // 11-15초: 1.0배 -> 1.5배
        // 16-20초: 1.6배 -> 2.0배
        // 21-30초: 2.1배 -> 4.0배

        // 10초까지 대기한 뒤 확장을 시작합니다
        yield return new WaitForSeconds(10f);

        // 대기 끝나면 링 오브젝트 활성화
        if (_glowRing != null)
            _glowRing.SetActive(true);

        // 진단 로그: 어떤 카메라를 사용하는지 출력
        Camera cam = glowCamera != null ? glowCamera : Camera.main;
        Debug.Log($"발광 이펙트: 빌보드 카메라 = {(cam != null ? cam.name : "(없음)")}");

        float t = 0f;

        // 1단계: 11-15초 (5초) -> 1.0 -> 1.5
        float phase1Dur = 5f;
        t = 0f;
        while (t < phase1Dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / phase1Dur);
            float s = Mathf.Lerp(1.0f, 1.5f, k);
            UpdateGlowScale(s);
            yield return null;
        }

        // 1.6으로 약간 점프한 뒤 1초 대기 (16초 기준)
        UpdateGlowScale(1.6f);
        yield return new WaitForSeconds(1f);

        // 2단계: 16-20초 (5초) -> 1.6 -> 2.0
        float phase2Dur = 5f;
        t = 0f;
        while (t < phase2Dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / phase2Dur);
            float s = Mathf.Lerp(1.6f, 2.0f, k);
            UpdateGlowScale(s);
            yield return null;
        }

        // 2.1로 약간 점프한 뒤 1초 대기 (21초 기준)
        UpdateGlowScale(2.1f);
        yield return new WaitForSeconds(1f);

        // 3단계: 21-30초 (10초) -> 2.1 -> 4.0
        float phase3Dur = 10f;
        t = 0f;
        while (t < phase3Dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / phase3Dur);
            float s = Mathf.Lerp(2.1f, 4.0f, k);
            UpdateGlowScale(s);
            yield return null;
        }

        // 최종 크기 유지
        UpdateGlowScale(4.0f);
    }

    private void UpdateGlowScale(float multiplier)
    {
        CreateGlowQuadIfNeeded();
        if (_glowRing == null) return;

        // 구의 월드 반지름(근사)
        float sphereWorldRadius = 0.5f;
        if (_meshRenderer != null)
            sphereWorldRadius = _meshRenderer.bounds.extents.x;

        // inner은 구 표면 바로 바깥에서 시작하도록 함
        float innerRadius = sphereWorldRadius + 0.01f; // 약간의 여유
        // 전역 스케일을 곱해 전체 확산 범위를 키움
        float outerRadius = innerRadius * multiplier * glowGlobalScale;

        // 메쉬를 재생성/갱신 (로컬 XY 평면에 만들었으므로 회전/위치 설정 필요)
        CreateOrUpdateRingMesh(innerRadius, outerRadius, 128);

        // 셰이더 컷오프 및 구 정보 설정
        float innerCutoff = 0.45f;
        float outerCutoff = 0.95f;
        if (_glowMat != null)
        {
            _glowMat.SetFloat("_InnerCutoff", innerCutoff);
            _glowMat.SetFloat("_OuterCutoff", outerCutoff);
            _glowMat.SetVector("_SphereCenter", transform.position);
            _glowMat.SetFloat("_SphereRadius", sphereWorldRadius);
        }

        // 카메라 방향으로 링을 밀어내고 빌보드 회전 (지정된 glowCamera 우선)
        Camera cam = glowCamera != null ? glowCamera : Camera.main;
        if (cam != null)
        {
            Vector3 camDir = (cam.transform.position - transform.position).normalized;
            float pushOut = sphereWorldRadius * 1.02f + 0.02f; // 충분한 마진
            _glowRing.transform.position = transform.position + camDir * pushOut;
            _glowRing.transform.rotation = Quaternion.LookRotation(_glowRing.transform.position - cam.transform.position);
            // 링은 카메라 앞에 완전히 위치하므로 구 내부 침범 없음
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
        // 음량 증가 코루틴 중단
        if (_volumeBoostCoroutine != null)
        {
            StopCoroutine(_volumeBoostCoroutine);
            _volumeBoostCoroutine = null;
        }

        // 마스터 볼륨을 원래 값으로 복원
        MainSystem.Instance.SoundController.SetAudioVolume(0, _originalMasterVolume);

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.loop = false;
        }

        // Stop glow
        StopGlowEffect();
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
        SequenceChangeColor(_timeOverColor, _timeOverColor * 10, 2);
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

    private void SequenceChangeColor(Color startColor, Color endColor, float maxTime)
    {
        StartCoroutine(IE_SequenceChangeColor(startColor, endColor, maxTime));
    }

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

    [ContextMenu("테스트/기본으로 초기화")]
    private void TestDefault() => ResetToDefault();

    [ContextMenu("테스트/소리 트리거")]
    private void TestTriggerSound() => TriggerSound();

    [ContextMenu("테스트/공 터치")]
    private void TestTouched() => OnTouched();

    [ContextMenu("테스트/틀렸음 표시")]
    private void TestWrong() => MarkWrong();

    [ContextMenu("테스트/시간초과 표시")]
    private void TestTimeOver() => OnMarkTimeOver();

    [ContextMenu("디버그/현재 상태 로그")]
    private void DebugLogState() => Debug.Log($"[인터랙티브스피어] 현재 상태: {CurrentState}");

    [ContextMenu("디버그/상태 변경 이벤트 트리거")]
    private void DebugTriggerEvent() => OnStateChanged(currentState);
    
#endif
}