// Requires hands package and VisionOSHandExtensions which is only compiled for visionOS and Editor
using System.Collections.Generic;
using UnityEngine;
#if UNITY_VISIONOS
using UnityEngine.XR.VisionOS;
using UnityEngine.XR.Hands;
#endif

//#if INCLUDE_UNITY_XR_HANDS && (UNITY_VISIONOS || UNITY_EDITOR)
public class HandVisualizer_CY : MonoBehaviour
{
    [Header("Clap Detection")]
    [SerializeField] private WorldFixPopupController _worldFixPopupController;
    [SerializeField] private float _clapThreshold = 0.15f;
    [SerializeField] private float _clapCooltime = 2.0f;
    private float _currClapCooltime = 0.0f;
    [SerializeField]
#if UNITY_VISIONOS
    GameObject m_JointVisualsPrefab;
    XRHandSubsystem m_Subsystem;
    HandGameObjects m_LeftHandGameObjects;
    HandGameObjects m_RightHandGameObjects;


    static readonly List<XRHandSubsystem> k_SubsystemsReuse = new();

    // Clap Detection Fields

    protected void OnEnable()
    {
        if (m_Subsystem == null)
            return;

        UpdateRenderingVisibility(m_LeftHandGameObjects, m_Subsystem.leftHand.isTracked);
        UpdateRenderingVisibility(m_RightHandGameObjects, m_Subsystem.rightHand.isTracked);
    }

    protected void OnDisable()
    {
        if (m_Subsystem != null)
            UnsubscribeSubsystem();

        UpdateRenderingVisibility(m_LeftHandGameObjects, false);
        UpdateRenderingVisibility(m_RightHandGameObjects, false);
    }

    void UnsubscribeSubsystem()
    {
        m_Subsystem.trackingAcquired -= OnTrackingAcquired;
        m_Subsystem.trackingLost -= OnTrackingLost;
        m_Subsystem.updatedHands -= OnUpdatedHands;
        m_Subsystem = null;
    }

    protected void OnDestroy()
    {
        if (m_LeftHandGameObjects != null)
        {
            m_LeftHandGameObjects.OnDestroy();
            m_LeftHandGameObjects = null;
        }

        if (m_RightHandGameObjects != null)
        {
            m_RightHandGameObjects.OnDestroy();
            m_RightHandGameObjects = null;
        }
    }
#endif

    protected void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_worldFixPopupController != null)
            {
                if (!_worldFixPopupController.IsActiveCanvas)
                    _worldFixPopupController.OpenCanvas(Camera.main.transform);
                else
                    _worldFixPopupController.CloseCanvas();
                MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"Clap Detected! UI Active: {_worldFixPopupController.IsActiveCanvas}");
            }
        }
#elif UNITY_VISIONOS
        if (m_Subsystem != null)
        {
            if (m_Subsystem.running)
                return;

            UnsubscribeSubsystem();
            UpdateRenderingVisibility(m_LeftHandGameObjects, false);
            UpdateRenderingVisibility(m_RightHandGameObjects, false);
            return;
        }

        SubsystemManager.GetSubsystems(k_SubsystemsReuse);
        for (var i = 0; i < k_SubsystemsReuse.Count; ++i)
        {
            var handSubsystem = k_SubsystemsReuse[i];
            if (handSubsystem.running)
            {
                UnsubscribeHandSubsystem();
                m_Subsystem = handSubsystem;
                break;
            }
        }

        if (m_Subsystem == null)
            return;

        if (m_LeftHandGameObjects == null)
        {
            m_LeftHandGameObjects = new HandGameObjects(
                Handedness.Left,
                transform,
                m_JointVisualsPrefab);
        }

        if (m_RightHandGameObjects == null)
        {
            m_RightHandGameObjects = new HandGameObjects(
                Handedness.Right,
                transform,
                m_JointVisualsPrefab);
        }

        UpdateRenderingVisibility(m_LeftHandGameObjects, m_Subsystem.leftHand.isTracked);
        UpdateRenderingVisibility(m_RightHandGameObjects, m_Subsystem.rightHand.isTracked);

        UpdateRenderingVisibility(m_LeftHandGameObjects, m_Subsystem.leftHand.isTracked);
        UpdateRenderingVisibility(m_RightHandGameObjects, m_Subsystem.rightHand.isTracked);

        SubscribeHandSubsystem();
#endif
    }
#if UNITY_VISIONOS
    private void CheckClap(XRHandSubsystem subsystem)
    {
        if (_currClapCooltime < _clapCooltime)
        {
            _currClapCooltime += Time.deltaTime;
        }

        MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"subsystem: {subsystem}");
        MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"subsystem.leftHand.isTracked: {subsystem.leftHand.isTracked}");
        MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"subsystem.rightHand.isTracked: {subsystem.rightHand.isTracked}");
        if (subsystem != null && subsystem.leftHand.isTracked && subsystem.rightHand.isTracked)
        {
            // Palm이 유효하지 않으면 Wrist를 사용 (fallback)
            var leftJoint = subsystem.leftHand.GetJoint(XRHandJointID.Palm);
            if (leftJoint.trackingState == XRHandJointTrackingState.WillNeverBeValid)
                leftJoint = subsystem.leftHand.GetJoint(XRHandJointID.Wrist);

            var rightJoint = subsystem.rightHand.GetJoint(XRHandJointID.Palm);
            if (rightJoint.trackingState == XRHandJointTrackingState.WillNeverBeValid)
                rightJoint = subsystem.rightHand.GetJoint(XRHandJointID.Wrist);

            MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"leftJointID: {leftJoint.id}, rightJointID: {rightJoint.id}");

            if (leftJoint.TryGetPose(out Pose leftPose) && rightJoint.TryGetPose(out Pose rightPose))
            {
                float distance = Vector3.Distance(leftPose.position, rightPose.position);
                MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"distance: {distance}");
                if (distance <= _clapThreshold)
                {
                    if (_currClapCooltime >= _clapCooltime)
                    {
                        if (!_worldFixPopupController.IsActiveCanvas)
                            _worldFixPopupController.OpenCanvas(Camera.main.transform);
                        else
                            _worldFixPopupController.CloseCanvas();
                        _currClapCooltime = 0.0f;
                        MainSystem.Instance.Loggers.LogInfo("HandVisualizer_CY", "CheckClap", $"Clap Detected! UI Active: {_worldFixPopupController.IsActiveCanvas}");
                    }
                }
            }
        }
    }

    void SubscribeHandSubsystem()
    {
        if (m_Subsystem == null)
            return;

        m_Subsystem.trackingAcquired += OnTrackingAcquired;
        m_Subsystem.trackingLost += OnTrackingLost;
        m_Subsystem.updatedHands += OnUpdatedHands;
    }

    void UnsubscribeHandSubsystem()
    {
        if (m_Subsystem == null)
            return;

        m_Subsystem.trackingAcquired -= OnTrackingAcquired;
        m_Subsystem.trackingLost -= OnTrackingLost;
        m_Subsystem.updatedHands -= OnUpdatedHands;
    }

    static void UpdateRenderingVisibility(HandGameObjects handGameObjects, bool isTracked)
    {
        if (handGameObjects == null)
            return;

        handGameObjects.SetHandActive(isTracked);
    }

    void OnTrackingAcquired(XRHand hand)
    {
        switch (hand.handedness)
        {
            case Handedness.Left:
                UpdateRenderingVisibility(m_LeftHandGameObjects, true);
                break;

            case Handedness.Right:
                UpdateRenderingVisibility(m_RightHandGameObjects, true);
                break;
        }
    }

    void OnTrackingLost(XRHand hand)
    {
        switch (hand.handedness)
        {
            case Handedness.Left:
                UpdateRenderingVisibility(m_LeftHandGameObjects, false);
                break;

            case Handedness.Right:
                UpdateRenderingVisibility(m_RightHandGameObjects, false);
                break;
        }
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        // We have no game logic depending on the Transforms, so early out here
        // (add game logic before this return here, directly querying from
        // subsystem.leftHand and subsystem.rightHand using GetJoint on each hand)
        if (updateType == XRHandSubsystem.UpdateType.Dynamic)
            return;

        CheckClap(subsystem);

        m_LeftHandGameObjects.UpdateJoints(
            subsystem.leftHand,
            (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints) != 0);

        m_RightHandGameObjects.UpdateJoints(
            subsystem.rightHand,
            (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandJoints) != 0);
    }

    class HandGameObjects
    {
        GameObject m_JointVisualsParent;

        readonly JointVisuals_CY[] m_JointVisuals = new JointVisuals_CY[XRHandJointID.EndMarker.ToIndex() + VisionOSHandExtensions.NumVisionOSJoints];

        static Vector3[] s_LinePointsReuse = new Vector3[2];
        const float k_LineWidth = 0.005f;


        public HandGameObjects(
            Handedness handedness,
            Transform parent,
            GameObject jointVisualsPrefab)
        {
            void AssignJoint(
                XRHandJointID jointID,
                Transform drawJointsParent)
            {
                var jointIndex = jointID.ToIndex();
                var jointVisualsObject = Instantiate(jointVisualsPrefab, drawJointsParent, false);
                var jointName = jointID < XRHandJointID.EndMarker ? jointID.ToString() : ((VisionOSHandJointID)jointID).ToString();
                jointVisualsObject.name = $"{jointName}";
                var jointVisuals = jointVisualsObject.GetComponent<JointVisuals_CY>();

                var line = jointVisuals.Line;
                line.startWidth = line.endWidth = k_LineWidth;
                s_LinePointsReuse[0] = s_LinePointsReuse[1] = jointVisuals.transform.position;
                line.SetPositions(s_LinePointsReuse);

                m_JointVisuals[jointIndex] = jointVisuals;
            }

            m_JointVisualsParent = new GameObject();
            var parentTransform = m_JointVisualsParent.transform;
            parentTransform.parent = parent;
            parentTransform.localPosition = Vector3.zero;
            parentTransform.localRotation = Quaternion.identity;
            m_JointVisualsParent.name = $"{handedness} Hand Joints";

            AssignJoint(XRHandJointID.Wrist, parentTransform);
            AssignJoint(XRHandJointID.Palm, parentTransform);

            for (var fingerIndex = (int)XRHandFingerID.Thumb;
                 fingerIndex <= (int)XRHandFingerID.Little;
                 ++fingerIndex)
            {
                var fingerId = (XRHandFingerID)fingerIndex;
                var jointIndexBack = fingerId.GetBackJointID().ToIndex();
                for (var jointIndex = fingerId.GetFrontJointID().ToIndex();
                     jointIndex <= jointIndexBack;
                     ++jointIndex)
                {
                    AssignJoint(XRHandJointIDUtility.FromIndex(jointIndex), parentTransform);
                }
            }

            AssignJoint((XRHandJointID)VisionOSHandJointID.ForearmWrist, parentTransform);
            AssignJoint((XRHandJointID)VisionOSHandJointID.ForearmArm, parentTransform);
        }

        public void OnDestroy()
        {
            var length = m_JointVisuals.Length;
            for (var jointIndex = 0; jointIndex < length; ++jointIndex)
            {
                var visuals = m_JointVisuals[jointIndex];
                Destroy(visuals.gameObject);
                m_JointVisuals[jointIndex] = default;
            }

            Destroy(m_JointVisualsParent);
            m_JointVisualsParent = null;
        }

        public void SetHandActive(bool isActive)
        {
            m_JointVisualsParent.SetActive(isActive);
        }


        public void UpdateJoints(
            XRHand hand,
            bool areJointsTracked)
        {
            if (!areJointsTracked)
                return;

            var wristPose = Pose.identity;
            var parentIndex = XRHandJointID.Wrist.ToIndex();
            UpdateJoint(hand.GetJoint(XRHandJointID.Wrist), ref wristPose, ref parentIndex);
            UpdateJoint(hand.GetJoint(XRHandJointID.Palm), ref wristPose, ref parentIndex, false);

            for (var fingerIndex = (int)XRHandFingerID.Thumb;
                fingerIndex <= (int)XRHandFingerID.Little;
                ++fingerIndex)
            {
                var parentPose = wristPose;
                var fingerId = (XRHandFingerID)fingerIndex;
                parentIndex = XRHandJointID.Wrist.ToIndex();

                var jointIndexBack = fingerId.GetBackJointID().ToIndex();
                for (var jointIndex = fingerId.GetFrontJointID().ToIndex();
                    jointIndex <= jointIndexBack;
                    ++jointIndex)
                {
                    UpdateJoint(hand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)), ref parentPose, ref parentIndex);
                }
            }

            parentIndex = XRHandJointID.Wrist.ToIndex();
            UpdateJoint(hand.GetVisionOSJoint(VisionOSHandJointID.ForearmWrist), ref wristPose, ref parentIndex);
            UpdateJoint(hand.GetVisionOSJoint(VisionOSHandJointID.ForearmArm), ref wristPose, ref parentIndex);
        }

        void UpdateJoint(
            XRHandJoint joint,
            ref Pose parentPose,
            ref int parentIndex,
            bool cacheParentPose = true)
        {
            if (joint.id == XRHandJointID.Invalid)
                return;

            var jointIndex = joint.id.ToIndex();
            var visuals = m_JointVisuals[jointIndex];
            if (!joint.TryGetPose(out var pose))
            {
                visuals.gameObject.SetActive(false);
                return;
            }

            joint.TryGetVisionOSTrackingState(out var trackingState);
            visuals.SetIsTracked(trackingState);
            var visualsTransform = visuals.transform;
            visualsTransform.SetLocalPositionAndRotation(pose.position, pose.rotation);

            if (joint.id != XRHandJointID.Wrist)
            {
                var parentVisuals = m_JointVisuals[parentIndex];
                s_LinePointsReuse[0] = parentVisuals.transform.position;
                s_LinePointsReuse[1] = visualsTransform.position;
                visuals.Line.SetPositions(s_LinePointsReuse);
            }

            if (cacheParentPose)
            {
                parentPose = pose;
                parentIndex = jointIndex;
            }
        }
    }
#endif
}
