using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;

    public static SceneLoader Instance => _instance ?? throw new System.Exception("SceneLoader is not initialized.");

    public Action Act_SceneLoadStart { get; private set; }
    public Action Act_SceneLoadCompleted { get; private set; }

    public bool IsTutorialScene { get; private set; }  
    private IEnumerator IE_LoadSceneHandle = null;
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
        // 게임 시작 시 자동으로 Tutorial 씬으로 이동
        //LoadTraining();
        LoadLobby();
    }
    public void SetLoadSceneAct(Action actSceneLoadStart, Action actSceneLoadCompleted)
    {
        if (actSceneLoadStart != null)
            Act_SceneLoadStart += actSceneLoadStart;
        if (actSceneLoadCompleted != null)
            Act_SceneLoadCompleted += actSceneLoadCompleted;
    }
    /// <summary>
    /// 지정된 씬으로 이동
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (IE_LoadSceneHandle != null)
            return;
        StartCoroutine(IE_LoadSceneHandle = IE_LoadScene(sceneName));
    }

    private IEnumerator IE_LoadScene(string sceneName)
    {
        MainSystem.Instance.Loggers.LogInfo("SceneLoader", "IE_LoadScene", "Act_SceneLoadStart");
        Act_SceneLoadStart?.Invoke();
        IsTutorialScene = sceneName.Contains("Tutorial");
        AsyncOperation sceneLoadHanldle = SceneManager.LoadSceneAsync(sceneName);
        while (!sceneLoadHanldle.isDone)
            yield return null;
        MainSystem.Instance.Loggers.LogInfo("SceneLoader", "IE_LoadScene", "Act_SceneLoadCompleted");
        Act_SceneLoadCompleted?.Invoke();
        IE_LoadSceneHandle = null;
    }

    /// <summary>
    /// Tutorial 씬으로 이동
    /// </summary>
    public void LoadTutorial() => LoadScene("Tutorial");

    /// <summary>
    /// Main 씬으로 이동
    /// </summary>
    public void LoadLobby() => LoadScene("Lobby");

    public void LoadTraining() => LoadScene("TrainingStage");

}