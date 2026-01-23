
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;
using System;


//#if UNITY_EDITOR || UNITY_VISIONOS
//using UnityEngine.XR.VisionOS.InputDevices;
//#endif

public class XRHeadRayInteractor : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _rayOffsetTransform;
    [SerializeField] private Transform _rayDebugTransform;
    [SerializeField] private float _waitRayTime = 0.5f;
    [SerializeField] private float _rayTime = 1.0f;
    [SerializeField] private float _rayCooltime = 1.0f;
    public Action<float> Act_FillGauge;
    private float _totalRayTime;
    private float _currCoolTime = 0.0f;
    private float _currRayTime = 0.0f;
    private bool _isRayOver;
    private bool _isRayPossible = true;

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
        _totalRayTime = _waitRayTime + _rayTime;
    }

    void OnDisable()
    {
        _pointerInput.Disable();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        Debug_RayCast();
#else
        // Vision Pro에서는 PointerInput 대신 HMD의 Center Eye 값을 직접 사용합니다.
        // 핀치 제스처 시 포인터 입력이 손으로 전환되어 값이 튀는 현상을 방지합니다.
        // 카메라의 월드 좌표를 기준으로 레이를 계산하여 좌표계 문제를 방지합니다.
        RayTracking(_mainCamera.transform);
#endif
        if (!_isRayPossible)
        {
            _currCoolTime += Time.deltaTime;
            if (_currCoolTime >= _rayCooltime)
            {
                _isRayPossible = true;
                _currCoolTime = 0.0f;
            }
            return;
        }

        if (_isRayOver && _isRayPossible)
        {
            MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"_lastInteractable: {_lastInteractable}");
            MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"_lastInteractable.IsInteractable: {_lastInteractable.IsInteractable}");
            if (_lastInteractable != null && _lastInteractable.IsInteractable)
            {
                _currRayTime += Time.deltaTime;
                MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"_lastInteractable: {_currRayTime}");
                // _waitRayTime 이후부터 게이지가 차오르도록 계산
                if (_currRayTime >= _waitRayTime)
                {
                    float gaugeValue = (_currRayTime - _waitRayTime) / _rayTime;
                    Act_FillGauge?.Invoke(Mathf.Clamp01(gaugeValue));
                    MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"gaugeValue: {_currRayTime}");
                }

                if (_currRayTime >= _totalRayTime)
                {
                    // tutorial 페이지이면
                    // TODO: 추후에 반드시 바꿔야 함.. string 말고 int나 enum으로 교체해야 함.
                    if (SceneLoader.Instance.CurrentScene == "Tutorial")
                    {
                        InteractiveSphere sphere = (InteractiveSphere)_lastInteractable;
                        if (sphere != null)
                        {
                            if (sphere.CurrentState == InteractiveSphere.SphereState.Tutorial_WrongSelect)
                            {
                                UIPanelFactory.Instance.ShakeUI(0.12f, 0.7f);
                            }
                            else
                            {
                                UIPanelFactory.Instance.IsInteract = true;
                            }
                        }
                    }
                    else
                        _lastInteractable.OnSelect();
                    _isRayPossible = false; // 쿨다운 시작
                    ResetRayState(); // 상호작용 후 상태 초기화
                }
            }
        }
        else
        {
            ResetRayState();
        }
        //var defaultActions = _pointerInput.Default;

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
            MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "RayTracking", $"hitInfo: {hitInfo.transform.gameObject.name}");
            if (_lastInteractable == null)
            {
                if (hitInfo.transform.TryGetComponent(out _lastInteractable))
                {
                    _lastInteractable.OnRayOver();
                    _isRayOver = true;
                }
            }

        }
        else
        {
            if (_lastInteractable != null)
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
            // MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Debug_RayCast", $"hitInfo: {hitInfo.transform.gameObject.name}");
            if (_lastInteractable == null)
            {
                if (hitInfo.transform.TryGetComponent(out _lastInteractable))
                {
                    _lastInteractable.OnRayOver();
                    _isRayOver = true;
                }
            }
        }
        else
        {
            if (_lastInteractable != null)
            {
                _lastInteractable.OnRayOut();
                _lastInteractable = null;
                _isRayOver = false;
            }
        }
    }

    private void ResetRayState()
    {
        _currRayTime = 0.0f;
        Act_FillGauge?.Invoke(0);
    }
}
