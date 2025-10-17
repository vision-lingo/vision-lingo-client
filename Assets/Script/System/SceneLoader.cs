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
    public void LoadMain() => LoadScene(MainSystem.Instance.IsDev ? "Main_Dev" : "Main");

}