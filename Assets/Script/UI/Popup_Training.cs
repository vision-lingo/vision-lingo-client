using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Training : Popup
{
    [SerializeField] private Button _btn_SetVolume;
    [SerializeField] private Button _btn_GotoLobbey;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Action _act_OpenAdjustVolumeWindow;
    //private Action _act_GotoLobbey;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        _btn_SetVolume.onClick.AddListener(OpenAdjustVolumeWindow);
        _btn_GotoLobbey.onClick.AddListener(GotoLobbey);
    }

    public void SetPopup(Action act_OpenAdjustVolumeWindow)
    {
        _act_OpenAdjustVolumeWindow = act_OpenAdjustVolumeWindow;
    }

    public override void OpenPopup()
    {
        gameObject.SetActive(true);
    }

    public override void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    private void OpenAdjustVolumeWindow()
    {
        _act_OpenAdjustVolumeWindow?.Invoke();
    }

    private void GotoLobbey()
    {
        SceneLoader.Instance.LoadLobby();
        MainSystem.Instance.Act_Resume?.Invoke();
    }
}
