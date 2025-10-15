using System;
using UnityEngine;

#if UNITY_EDITOR || UNITY_VISIONOS
using UnityEngine.XR.VisionOS.InputDevices;
#endif

namespace UnityEngine.XR.VisionOS.Samples.URP
{
    public class InputTester : MonoBehaviour
    {
        [Serializable]
        struct TestObjects
        {
            public Transform Device;
            public Transform Ray;
            public Transform Target;
        }

        const int k_RequiredTestObjectsCount = 2;

        [SerializeField]
        TestObjects[] m_TestObjects;

        [SerializeField]
        Transform _rayTransform;
        [SerializeField]
        Transform m_CameraOffset;

#if UNITY_EDITOR || UNITY_VISIONOS
        PointerInput m_PointerInput;

        void OnEnable()
        {
            m_PointerInput ??= new PointerInput();
            m_PointerInput.Enable();

            if (m_TestObjects.Length != k_RequiredTestObjectsCount)
            {
                Debug.LogError($"Exactly {k_RequiredTestObjectsCount} sets of test objects are needed");
                enabled = false;
            }
        }

        void OnDisable()
        {
            m_PointerInput.Disable();
        }

        void OnValidate()
        {
            if (m_TestObjects.Length != k_RequiredTestObjectsCount)
                Debug.LogWarning($"Issue in InputTester: Exactly {k_RequiredTestObjectsCount} sets of test objects are needed");
        }

        void Update()
        {
            var defaultActions = m_PointerInput.Default;
            var primaryPointer = defaultActions.PrimaryPointer.ReadValue<VisionOSSpatialPointerState>();
            var secondaryPointer = defaultActions.SecondaryPointer.ReadValue<VisionOSSpatialPointerState>();
            var eyePos = defaultActions.EyePos.ReadValue<Vector3>();
            var eyeRot = defaultActions.EyeRot.ReadValue<Quaternion>();
            UpdateObjects(primaryPointer, m_TestObjects[0]);
            UpdateObjects(secondaryPointer, m_TestObjects[1]);
            RayTracking(eyePos, eyeRot);
        }
        MeshRenderer _lastHitInfo = null;
        // 인터렉션을 시작 할 때 실행 된다..
        // 아이 트래킹 지원 X라고 보는게 맞을듯..
        // Center 포지션만 가져오는 것만 가능
        // 아니면..핀치할 때 호버 효과를 줘야될 것 같음
        private void RayTracking(Vector3 eyePos, Quaternion eyeRot)
        {
            var rayOrigin = m_CameraOffset.TransformPoint(eyePos);
            var eyeDirection = eyeRot * Vector3.forward;
            var rayDirection = m_CameraOffset.TransformDirection(eyeDirection);
            _rayTransform.SetPositionAndRotation(rayOrigin, Quaternion.LookRotation(rayDirection));
            var ray = new Ray(rayOrigin, rayDirection);
            var hit = Physics.Raycast(ray, out var hitInfo);
            if (hit)
            {
                Debug.Log($"[EYETEST]::::hitInfo: {hitInfo.transform.gameObject.name}");
                _lastHitInfo = hitInfo.transform.gameObject.GetComponent<MeshRenderer>();
                _lastHitInfo.material.color = Color.magenta;
            }
            else
            {
                if (_lastHitInfo != null)
                    _lastHitInfo.material.color = Color.gray;
            }
            
        }


        void UpdateObjects(VisionOSSpatialPointerState pointerState, TestObjects objects)
        {
            var phase = pointerState.phase;
            var began = phase == VisionOSSpatialPointerPhase.Began;
            var active = began || phase == VisionOSSpatialPointerPhase.Moved;
            var deviceTransform = objects.Device;
            var rayTransform = objects.Ray;
            deviceTransform.gameObject.SetActive(active);
            rayTransform.gameObject.SetActive(active);

            if (began)
            {
                var rayOrigin = m_CameraOffset.TransformPoint(pointerState.startRayOrigin);
                var rayDirection = m_CameraOffset.TransformDirection(pointerState.startRayDirection);
                rayTransform.SetPositionAndRotation(rayOrigin, Quaternion.LookRotation(rayDirection));

                var ray = new Ray(rayOrigin, rayDirection);
                var hit = Physics.Raycast(ray, out var hitInfo);
                //Debug.Log($"[EYETEST]::::hitInfo: {hitInfo.transform.gameObject.name}");
                var targetTransform = objects.Target;
                targetTransform.gameObject.SetActive(hit);
                targetTransform.position = hitInfo.point;
            }

            if (active)
                deviceTransform.SetLocalPositionAndRotation(pointerState.inputDevicePosition, pointerState.inputDeviceRotation);
        }
#endif
    }
}
