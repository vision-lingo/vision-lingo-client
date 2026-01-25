using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup_SetVolume : Popup
{

    [SerializeField] private Button _btn_Close;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private RectTransform _sliderHandle;

    private Action _act_Close;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _btn_Close.onClick.AddListener(ClosePopup); 
        SetAdjustVolumeUI();
    }


    public void SetCloseAction(Action act_Close)
    {
        _act_Close = act_Close;
    }


    public override void OpenPopup()
    {
        gameObject.SetActive(true);
        _volumeSlider.value = MainSystem.Instance.SoundController.CurrMaxsterVolume;
        MainSystem.Instance.SoundController.PlayMusic("WhirlwindOfJoy");
    }

    public override void ClosePopup()
    {
        MainSystem.Instance.SoundController.StopMusic();
        _act_Close?.Invoke();
        gameObject.SetActive(false);
    }

    private void SetAdjustVolumeUI()
    {
        _volumeSlider.onValueChanged.AddListener(OnControlVolume);
    }

    private void OnControlVolume(float _volume)
    {
        // slider handle 벗어남 방지 로직
        if(_volume < 0.051f)
        {
            _sliderHandle.anchoredPosition = new Vector2(-0.05f + (0.05f - _sliderHandle.anchorMin.x), 0f);
        }
        else
            _sliderHandle.anchoredPosition = new Vector2(-0.05f, 0f);
        MainSystem.Instance.SoundController.SetAudioVolume(0, _volume);
    }

}
