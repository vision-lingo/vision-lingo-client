using UnityEngine;
using UnityEngine.UI;

public class TrainingHeadUIController : MonoBehaviour
{
    [SerializeField] private Button _btnGotoLobby;
    [SerializeField] private Button _btn_SetVolume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _btnGotoLobby.onClick.AddListener(GotoLobby);
        _btn_SetVolume.onClick.AddListener(SetVolume);
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
        MainSystem.Instance.Act_Pause?.Invoke();
        HeadUIController.Instance.ShowAdjustVolumeWindow();
        MainSystem.Instance.SoundController.PlayMusic("WhirlwindOfJoy");
    }


}
