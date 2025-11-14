
using UnityEngine;
using UnityEngine.UI;

public sealed class MainController : MonoBehaviour
{
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private Button _btnGotoTraining;
    [SerializeField] private Button _btnGotoTutorial;
    [SerializeField] private Button _btnQuitGame;
    [SerializeField] private Button _btnGotoCredit;

    private void Start()
    {
        _mainCanvas.worldCamera = Camera.main;
        // 25/11/12 CY: button 이벤트 구독은 Scene에서 하는 것 보단 코드로 하는게 관리하기 편합니다. 
        if(_btnGotoTraining != null)
            _btnGotoTraining.onClick.AddListener(()=> OnGoToScene("TrainingStage"));
        else
            MainSystem.Instance.Loggers.LogInfo("MainController", "Start", "_btnGotoTraining is null.");
        if(_btnGotoTutorial != null)
            _btnGotoTutorial.onClick.AddListener(()=> OnGoToScene("Tutorial"));
        else
            MainSystem.Instance.Loggers.LogInfo("MainController", "Start", "_btnGotoTutorial is null.");
        if(_btnQuitGame != null)
            _btnQuitGame.onClick.AddListener(MainSystem.Instance.QuitGame);
        else
            MainSystem.Instance.Loggers.LogInfo("MainController", "Start", "_btnQuitGame is null.");
        if(_btnGotoCredit != null)
            _btnGotoCredit.onClick.AddListener(()=> OnGoToScene("Credits Scene"));
        else
            MainSystem.Instance.Loggers.LogInfo("MainController", "Start", "_btnGotoCredit is null.");
    }

    public void OnGoToScene(string sceneName)
    {
        string loadSceneName = MainSystem.Instance.IsDev ? $"{sceneName}_Dev" : sceneName;
        SceneLoader.Instance.LoadScene(loadSceneName);
    }

}
