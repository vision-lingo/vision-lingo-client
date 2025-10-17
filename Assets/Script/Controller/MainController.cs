
using UnityEngine;

public sealed class MainController : MonoBehaviour
{
    [SerializeField] private Canvas _mainCanvas;

    private void Start()
    {
        _mainCanvas.worldCamera = Camera.main;
    }

    public void OnGoToScene(string sceneName)
    {
        string loadSceneName = MainSystem.Instance.IsDev ? $"{sceneName}_Dev" : sceneName;
        SceneLoader.Instance.LoadScene(loadSceneName);
    }
    public void OnQuitButton() => MainSystem.Instance.QuitGame();


}
