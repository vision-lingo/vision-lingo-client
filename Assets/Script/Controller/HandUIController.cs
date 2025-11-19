using UnityEngine;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [SerializeField] private Button _btnSetVolume;
    [SerializeField] private Button _btnGotoLobby;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _btnSetVolume.onClick.AddListener(SetVolume);
        _btnGotoLobby.onClick.AddListener(GotoLobby);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            GotoLobby();
        if(Input.GetKeyDown(KeyCode.J))
            SetVolume();
    }

    private void GotoLobby()
    {
        HeadUIController.Instance.ShowMessage("소리 위치 분별 훈련을 종료하겠습니다.", Vector2.zero, SceneLoader.Instance.LoadLobby);
    }
    // 형태가 바뀔 수 있음.
    private void SetVolume()
    {
        HeadUIController.Instance.ShowAdjustVolumeWindow();
    }

}
