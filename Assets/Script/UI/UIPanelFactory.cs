using UnityEngine;
using System.Collections;
using System;

public class UIPanelFactory : MonoBehaviour
{
    public static UIPanelFactory Instance { get; private set; }

    [SerializeField] private UIPanel panelPrefab;
    [SerializeField] private Transform uiParent;

    public Transform UIParent => uiParent;
    public bool IsIdle { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        SceneLoader.Instance.SetLoadSceneAct(ClearPopup, StopUIProgress);
    }
    private void StopUIProgress()
    {
        IsIdle = false;
    }
    // TODO: 추후 Queue로 바꿔야 함.
    private void ClearPopup()
    {
        for(int i = 0; i < uiParent.childCount; i++)
        {
            Destroy(uiParent.GetChild(i).gameObject);
        }
    }

    public void ShowMessage(string text, bool isCenter = false, Action afterAct = null)
    {
        if (Instance == null || panelPrefab == null) return; // 안전 체크
        StartCoroutine(ShowMessageCoroutine(text, isCenter, afterAct));
    }
    // 25/10/29 CY: 마지막 메세지는 오버로드된 새로운 Show 메서드 호출
    public GameObject ShowLastMessage(string text, bool isCenter = false)
    {
        if (Instance == null || panelPrefab == null) return null; // 안전 체크
        var panel = Instantiate(panelPrefab, uiParent);

        var pos = isCenter ? UIPanelSettingsHelper.GetCenterPosition() :
                             UIPanelSettingsHelper.GetUpperPosition(0.5f);
        var fade = UIPanelSettingsHelper.GetDefaultFadeSettings();
        panel.Show(text, pos, fade.fadeInTime, null);
        return panel.gameObject;
    }

    private IEnumerator ShowMessageCoroutine(string text, bool isCenter, Action afterAct = null)
    {
        IsIdle = false;

        var panel = Instantiate(panelPrefab, uiParent);
        Debug.Log($"panel: {panel}");
        var pos = isCenter ? UIPanelSettingsHelper.GetCenterPosition() :
                             UIPanelSettingsHelper.GetUpperPosition(0.5f);
        var fade = UIPanelSettingsHelper.GetDefaultFadeSettings();

        panel.Show(text, pos, fade.fadeInTime, fade.displayTime, fade.fadeOutTime, afterAct);
        Debug.Log($"panel.gameObject: {panel.gameObject}");
        // gameObject.activeSelf 메서드는 매우 불안정함.
        while (/*panel.gameObject.activeSelf*/ !panel.IsCompleted)
            yield return null;

        IsIdle = true;
    }
}
