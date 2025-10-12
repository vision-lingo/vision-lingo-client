using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;

    public static SceneLoader Instance => _instance ?? throw new System.Exception("SceneLoader is not initialized.");

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 자동으로 Main 씬으로 이동
        LoadMain();
    }

    /// <summary>
    /// 지정된 씬으로 이동
    /// </summary>
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    /// <summary>
    /// Main 씬으로 이동
    /// </summary>
    public void LoadMain() => LoadScene("Main");

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame() => QuitGameInternal();

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
