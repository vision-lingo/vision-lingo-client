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

        var pos = isCenter ? UIPanelSettingsHelper.GetCenterPosition() :
                             UIPanelSettingsHelper.GetUpperPosition(0.5f);
        var fade = UIPanelSettingsHelper.GetDefaultFadeSettings();

        panel.Show(text, pos, fade.fadeInTime, fade.displayTime, fade.fadeOutTime, afterAct);

        while (panel.gameObject.activeSelf)
            yield return null;

        IsIdle = true;
    }
}
