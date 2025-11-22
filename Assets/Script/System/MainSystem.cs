using UnityEngine;
using System;

/// <summary>
/// 전체 시스템 관리
/// </summary>
public class MainSystem : SingletonT<MainSystem>
{
    [SerializeField] private SoundController _soundController;
    [SerializeField] private Loggers _loggers;
    [SerializeField] private bool _isDebug;
    [SerializeField] private bool _isDev;
    
    public Action Act_Pause = null;
    public Action Act_Resume = null;
    public SoundController SoundController => _soundController;
    public Loggers Loggers => _loggers;
    public float CurrMusicVolume
    {
        get { return _soundController.AudioMusic.volume; }
        set { _soundController.SetAudioVolume(0, value); }
    }
    public float CurrSfxVolume 
    { 
        get { return _soundController.AudioSfx.volume; } 
        set { _soundController.SetAudioVolume(1, value); } 
    }
    public bool IsPause { get; private set; }
    public bool IsDev => _isDev;



    private void Awake()
    {
        if (m_Instance != this && m_Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        Init();
    }

    private void Update()
    {

        
        //if(Input))
        // 부스 운영 시 사용해야될 수도 있으므로 따로 전처리문을 작성하지 않았음.
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Input.GetKeyDown(KeyCode.Keypad0)");
            SceneLoader.Instance.LoadLobby();
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Input.GetKeyDown(KeyCode.Keypad1)");
            SceneLoader.Instance.LoadTutorial();
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Input.GetKeyDown(KeyCode.Keypad2)");
            SceneLoader.Instance.LoadTraining();
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("IsPause");
            MainSystem.Instance.Act_Pause();
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("IsResume");
            MainSystem.Instance.Act_Resume();
        }

    }
    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame() => QuitGameInternal();

    private void Init()
    {
        _loggers.IsDebug = _isDebug;
        Act_Pause += OnPause;
        Act_Resume += OnResume;
    }

    private void OnPause()
    {
        IsPause = true;
    }
    private void OnResume()
    {
        IsPause = false;
    }

    /// <summary>
    /// 내부 Quit 처리
    /// </summary>
    private void QuitGameInternal()
    {
#if UNITY_EDITOR
        // 에디터에서 실행 중일 때
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드에서 실행될 때
        Application.Quit();
#endif
    }
}
