using UnityEngine;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [SerializeField] private Button _btnSetVolume;
    [SerializeField] private Button _btnGotoLobby;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _btnGotoLobby.onClick.AddListener(GotoLobby);
    }

    private void GotoLobby()
    {
        UIPanelFactory.Instance.ShowMessage("소리 위치 분별 훈련을 종료하겠습니다.", true, SceneLoader.Instance.LoadMain);
    }

}
