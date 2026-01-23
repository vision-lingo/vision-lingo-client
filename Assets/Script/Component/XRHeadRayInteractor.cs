
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
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        Debug.DrawRay(rayOrigin, rayDirection, Color.blue);
#else
        // Vision Pro에서는 PointerInput 대신 HMD의 Center Eye 값을 직접 사용합니다.
        // 카메라의 월드 좌표를 기준으로 레이를 계산하여 좌표계 문제를 방지합니다.
        Vector3 rayOrigin = _rayOffsetTransform.position;
        Vector3 rayDirection = _mainCamera.transform.forward;
#endif
        PerformRaycast(rayOrigin, rayDirection);
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

        if (_isRayOver && _isRayPossible && _lastInteractable != null && (_lastInteractable as UnityEngine.Object) != null)
        {
            if (_lastInteractable.IsInteractable)
            {
                MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"_lastInteractable: {_lastInteractable}");
                MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "Update", $"_lastInteractable.IsInteractable: {_lastInteractable.IsInteractable}");
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
                    if (SceneLoader.Instance.IsTutorialScene)
                    {
                        InteractiveSphere sphere = _lastInteractable as InteractiveSphere;
                        if (sphere != null)
                        {
                            if (sphere.CurrentState == InteractiveSphere.SphereState.Tutorial_WrongSelect)
                            {
                                UIPanelFactory.Instance.ShakeUI(0.12f, 0.7f);
                            }
                            else
                            {
                                _lastInteractable.OnSelect();
                                _lastInteractable = null;
                                UIPanelFactory.Instance.IsInteract = true;
                            }
                        }
                        else
                        {
                            _lastInteractable.OnSelect();
                            _lastInteractable = null;
                            MainSystem.Instance.Loggers.LogWarning("XRHeadRayInteractor", "Update", "sphere is null");
                        }
                    }
                    else
                    {
                        _lastInteractable.OnSelect();
                        _lastInteractable = null;
                    }
                    _isRayPossible = false; // 쿨다운 시작
                    ResetRayState(); // 상호작용 후 상태 초기화
                }
            }
        }
        else
        {
            ResetRayState();
        }

    }


    private void PerformRaycast(Vector3 origin, Vector3 direction)
    {
        _rayDebugTransform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        var ray = new Ray(origin, direction);
        var hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity);

        // 인터페이스 타입은 유니티의 == null 오버라이딩이 동작하지 않으므로 직접 캐스팅하여 확인
        bool isLastValid = _lastInteractable != null && (_lastInteractable as UnityEngine.Object) != null;

        if (hit)
        {
#if !UNITY_EDITOR
            MainSystem.Instance.Loggers.LogInfo("XRHeadRayInteractor", "PerformRaycast", $"hitInfo: {hitInfo.transform.gameObject.name}");
#endif
            if (!isLastValid)
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
            if (isLastValid)
            {
                _lastInteractable.OnRayOut();
            }
            _isRayOver = false;
            _lastInteractable = null;
        }
    }

    private void ResetRayState()
    {
        _currRayTime = 0.0f;
        Act_FillGauge?.Invoke(0);
    }
}
