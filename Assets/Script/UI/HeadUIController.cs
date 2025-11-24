using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TODO: 추후 리팩토링 필요함.
/// </summary>
public class HeadUIController : MonoBehaviour
{
    public static HeadUIController Instance {get; private set;}
    // 추후 VR 전용으로 만들어야될 수 있음.
    [SerializeField] private UIPanel _uiPanel;
    [SerializeField] private GameObject _dimObj;
    [SerializeField] private GameObject _volumeAdjustWindow;
    [SerializeField] private Button _btn_SetVolume;
    [SerializeField] private Vector3 _followOffset;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private RectTransform _sliderHandle;

    private IEnumerator IE_ShowMessageHandle = null;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _btn_SetVolume.onClick.AddListener(CloseAdjustVolumeWindow); 
        SetAdjustVolumeUI();
    }
    private void SetAdjustVolumeUI()
    {
        // 25/10/29 CY: 튜토리얼 Scene에서만 쓰이는 UI 액션 할당
        MainSystem.Instance.SoundController.SetAudioVolume(0, _volumeSlider.value);
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

    public void ShowAdjustVolumeWindow()
    {
        _dimObj.SetActive(true);
        MainSystem.Instance.Act_Pause?.Invoke();
        _volumeAdjustWindow.SetActive(true);
    }
    public void CloseAdjustVolumeWindow()
    {
        _dimObj.SetActive(false);
        MainSystem.Instance.Act_Resume?.Invoke();
        _volumeAdjustWindow.SetActive(false);
        MainSystem.Instance.SoundController.StopMusic();
    }


    public void ShowMessage(string text, Vector2 pos, Action afterAct)
    {
        if(IE_ShowMessageHandle != null)
            return;
        _dimObj.SetActive(true);
        StartCoroutine(IE_ShowMessageHandle = IE_ShowMessage(text, pos, afterAct));
    }

    private IEnumerator IE_ShowMessage(string text, Vector2 pos, Action afterAct)
    {
        var fade = UIPanelSettingsHelper.GetDefaultFadeSettings();
        
        _uiPanel.Show(text, pos, fade.fadeInTime, fade.displayTime, fade.fadeOutTime, afterAct);
        while(!_uiPanel.IsCompleted)
        {
            yield return null;
        }
        IE_ShowMessageHandle = null;
        _dimObj.SetActive(false);
    }

    private void OnDisable()
    {
       Instance = null; 
    }
}
