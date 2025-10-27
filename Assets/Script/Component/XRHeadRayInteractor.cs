
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;
#if UNITY_EDITOR || UNITY_VISIONOS
using UnityEngine.XR.VisionOS.InputDevices;
#endif

public class XRHeadRayInteractor : MonoBehaviour
{
    [SerializeField] private Transform _rayDebugTransform;
    [SerializeField] private Transform cameraOffset;
    [SerializeField] private Vector3 _rayPosOffset;
    private PointerInput _pointerInput;
    private IXRHeadInteractable _lastInteractable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
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
        var defaultActions = _pointerInput.Default;
        #if UNITY_EDITOR
        Debug_RayCast();
        #endif
        var eyePos = defaultActions.EyePos.ReadValue<Vector3>();
        var eyeRot = defaultActions.EyeRot.ReadValue<Quaternion>();
        RayTracking(eyePos, eyeRot);
    }


    private void RayTracking(Vector3 eyePos, Quaternion eyeRot)
    {
        eyePos += _rayPosOffset;
        var rayOrigin = cameraOffset.TransformPoint(eyePos);
        var eyeDirection = eyeRot * Vector3.forward;
        var rayDirection = cameraOffset.TransformDirection(eyeDirection);
        _rayDebugTransform.SetPositionAndRotation(rayOrigin, Quaternion.LookRotation(rayDirection));
        var ray = new Ray(rayOrigin, rayDirection);
        var hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity);
        if (hit)
        {
            Debug.Log($"[EYETEST]::::hitInfo: {hitInfo.transform.gameObject.name}");
            if(hitInfo.transform.TryGetComponent(out _lastInteractable))
            {
                _lastInteractable.OnRayOver();
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
            if(hitInfo.transform.TryGetComponent(out _lastInteractable))
            {
                _lastInteractable.OnRayOver();
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
