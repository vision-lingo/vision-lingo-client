using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TODO: 추후 리팩토링 필요함.
/// </summary>
public class HeadUIController : MonoBehaviour
{
    public static HeadUIController Instance {get; private set;}
    // 추후 VR 전용으로 만들어야될 수 있음.
    [SerializeField] private UIPanel _uiPanel;
    [SerializeField] private Vector3 _followOffset;

    private IEnumerator IE_ShowMessageHandle = null;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string text, Vector2 pos, Action afterAct)
    {
        if(IE_ShowMessageHandle != null)
            return;
        StartCoroutine(IE_ShowMessageHandle = IE_ShowMessage(text, pos, afterAct));
    }

    private IEnumerator IE_ShowMessage(string text, Vector2 pos, Action afterAct)
    {
        var fade = UIPanelSettingsHelper.GetDefaultFadeSettings();
        
        _uiPanel.Show(text, pos, fade.fadeInTime, fade.displayTime, fade.fadeOutTime, afterAct);
        while(!_uiPanel.IsCompleted)
        {
            
        }
        yield return new WaitUntil(()=>_uiPanel.IsCompleted);
        IE_ShowMessageHandle = null;
    }

    private void OnDisable()
    {
       Instance = null; 
    }
}
