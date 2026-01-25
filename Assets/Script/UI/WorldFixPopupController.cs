using UnityEngine;
using UnityEngine.UI;

public class WorldFixPopupController : MonoBehaviour
{
    [SerializeField] private Vector3 _offsetPos;
    [SerializeField] private Popup_Training _popup_Training;
    [SerializeField] private Popup_SetVolume _popup_SetVolume;

    public bool IsActiveCanvas { get; private set; } = false;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        // 여기서 popup의 역할들 초기화?
        _popup_Training.SetPopup(() => OpenSetVolumeWindow());
        // 볼륨 저장(세팅) 시 팝업 닫기.
        _popup_SetVolume.SetCloseAction(() => CloseCanvas());
    }

    public void OpenCanvas(Transform cameraPos)
    {
        MainSystem.Instance.Act_Pause?.Invoke();
        gameObject.SetActive(true);
        // 카메라 앞쪽으로 위치 설정 (카메라 위치 + 카메라 앞쪽 방향 * 거리)
        transform.position = cameraPos.position + (cameraPos.forward * _offsetPos.z);
        
        // UI가 카메라를 바라보도록 회전 설정 (WorldFixPopupController가 카메라와 같은 방향을 봄)
        // transform.rotation = cameraPos.rotation; 
        
        // 혹은 UI가 항상 카메라를 정면으로 마주보게 하려면 (Billboarding):
        transform.LookAt(transform.position + cameraPos.rotation * Vector3.forward, cameraPos.rotation * Vector3.up);
        _popup_Training.OpenPopup();
        IsActiveCanvas = true;
    }
    public void CloseCanvas()
    {
        MainSystem.Instance.Act_Resume?.Invoke();
        gameObject.SetActive(false);
        IsActiveCanvas = false;
    }
    public void OpenSetVolumeWindow()
    {
        _popup_SetVolume.OpenPopup();
    }
    
}
