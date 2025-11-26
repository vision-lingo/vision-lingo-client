
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR || UNITY_VISIONOS
using UnityEngine.XR.VisionOS.InputDevices;
#endif

public class XRHeadRayInteractor : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera; 
    [SerializeField] private Transform _rayOffsetTransform;
    [SerializeField] private Transform _rayDebugTransform;
    [SerializeField] private float _rayTime = 1.0f;
    public Action<float> Act_FillGauge;
    private bool _isRayOver;
    private float _currRayTime = 0.0f;

    private PointerInput _pointerInput;
    private IXRHeadInteractable _lastInteractable = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        _pointerInput ??= new PointerInput();
        _pointerInput.Enable();
    }

    void OnDisable()
    {
        _pointerInput.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(_isRayOver)
        {
            _currRayTime += Time.deltaTime;
            Act_FillGauge?.Invoke(1 - _currRayTime / _rayTime);
            if(_currRayTime > _rayTime)
            {
                if(_lastInteractable != null)
                {
                    _lastInteractable.OnSelect();
                    _lastInteractable = null;
                }
                _currRayTime = 0.0f;
                _isRayOver = false;
            }
        }
        else
        {
            _currRayTime = 0.0f;
            Act_FillGauge?.Invoke(0);
        }
        //var defaultActions = _pointerInput.Default;
        #if UNITY_EDITOR
        Debug_RayCast();
        #else
        // Vision Pro에서는 PointerInput 대신 HMD의 Center Eye 값을 직접 사용합니다.
        // 핀치 제스처 시 포인터 입력이 손으로 전환되어 값이 튀는 현상을 방지합니다.
        // 카메라의 월드 좌표를 기준으로 레이를 계산하여 좌표계 문제를 방지합니다.
        RayTracking(_mainCamera.transform);
        #endif
    }


    private void RayTracking(Transform cameraTransform)
    {
        var rayOrigin = _rayOffsetTransform.position;// + _rayPosOffset; //+ cameraTransform.TransformDirection(_rayPosOffset);
        var rayDirection = cameraTransform.forward;
        _rayDebugTransform.SetPositionAndRotation(rayOrigin, Quaternion.LookRotation(rayDirection));
        var ray = new Ray(rayOrigin, rayDirection);
        var hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity);
        if (hit)
        {
            Debug.Log($"[EYETEST]::::hitInfo: {hitInfo.transform.gameObject.name}");
            if(_lastInteractable == null)
            {
                if(hitInfo.transform.TryGetComponent(out _lastInteractable))
                {
                    _lastInteractable.OnRayOver();
                    _isRayOver = true;
                }
            }
            
        }
        else
        {
            if(_lastInteractable != null)
            {
                _lastInteractable.OnRayOut();
                _isRayOver = false;
                _lastInteractable = null;
            }
        }
    }
    private void Debug_RayCast()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        Debug.DrawRay(rayOrigin, rayDirection, Color.blue);
        _rayDebugTransform.SetPositionAndRotation(rayOrigin, Quaternion.LookRotation(rayDirection));
        var ray = new Ray(rayOrigin, rayDirection);
        var hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity);
        if (hit)
        {
            Debug.Log($"[EYETEST]::::hitInfo: {hitInfo.transform.gameObject.name}");
            if(_lastInteractable == null)
            {
                if(hitInfo.transform.TryGetComponent(out _lastInteractable))
                {
                    _lastInteractable.OnRayOver();
                }
            }
        }
        else
        {
            if(_lastInteractable != null)
            {
                _lastInteractable.OnRayOut();
                _lastInteractable = null;
            }
        }
    }
}
