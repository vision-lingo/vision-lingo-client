using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRHandStateChecker : MonoBehaviour
{
    public static XRHandStateChecker Instance {get; private set;}
    public bool IsLeftHandPinch {get; private set;}
    public bool IsRightHandPinch {get; private set;}
    [SerializeField] private InputActionProperty _leftAction;
    [SerializeField] private InputActionProperty _rightAction;
    [SerializeField] private XRRayInteractor _leftRay;
    [SerializeField] private XRRayInteractor _rightRay;

    private void Awake()
    {
        Instance = this;  
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
        _leftRay.selectEntered.AddListener(OnLeftPinch);
        _leftRay.selectExited.AddListener(OnLeftPinchOut);
        _rightRay.selectEntered.AddListener(OnRightPinch);
        _rightRay.selectExited.AddListener(OnRightPinchOut);


        _leftRay.hoverEntered.AddListener(OnLeftPinch_H);
        _leftRay.hoverExited.AddListener(OnLeftPinchOut_H);
        _rightRay.hoverEntered.AddListener(OnRightPinch_H);
        _rightRay.hoverExited.AddListener(OnRightPinchOut_H);
        var l_action = _leftAction.action;
        l_action.Enable();
        l_action.performed += OnLeftPerformed;
        l_action.canceled += OnLeftCanceled;
        var r_action = _rightAction.action;
        r_action.Enable();
        r_action.performed += OnRightPerformed;
        r_action.canceled += OnRightCanceled;
    }
    private void Update()
    {
        //Debug.Log($"[UPDATE]::::readValue<int>{_leftAction.action.ReadValue<int>()}");
           // Debug.Log($"[UPDATE]::::readValue<bool>{_leftAction.action.ReadValue<bool>()}");
            
      //IsLeftHandPinch = _leftAction.action.ReadValue<bool>();  
    }
    void OnLeftPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[InputTest]::::OnLeftPerformed");
        IsLeftHandPinch = true;
            //m_CurrentPosition = context.ReadValue<Vector3>();
    }
    void OnLeftCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("[InputTest]::::OnLeftCanceled");
        IsLeftHandPinch = false;
            //m_CurrentPosition = context.ReadValue<Vector3>();
    }
    void OnRightPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[InputTest]::::OnRightPerformed");
        IsRightHandPinch = true;
            //m_CurrentPosition = context.ReadValue<Vector3>();
    }
    void OnRightCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("[InputTest]::::OnRightCanceled");
        IsRightHandPinch = false;
            //m_CurrentPosition = context.ReadValue<Vector3>();
    }

    private void OnLeftPinch(SelectEnterEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnLeftPinch");
        IsLeftHandPinch = true;
    }
    private void OnLeftPinchOut(SelectExitEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnLeftPinchOut");
        IsLeftHandPinch = false;
    }
     private void OnRightPinch(SelectEnterEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnRightPinch");
        IsRightHandPinch = true;
    }
    private void OnRightPinchOut(SelectExitEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnRightPinchOut");
        IsRightHandPinch = false;
    }

    private void OnLeftPinch_H(HoverEnterEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnLeftPinch");
        IsLeftHandPinch = true;
    }
    private void OnLeftPinchOut_H(HoverExitEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnLeftPinchOut");
        IsLeftHandPinch = false;
    }
     private void OnRightPinch_H(HoverEnterEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnRightPinch");
        IsRightHandPinch = true;
    }
    private void OnRightPinchOut_H(HoverExitEventArgs selectEnterEventArgs)
    {
        Debug.Log("[InputTest]::::OnRightPinchOut");
        IsRightHandPinch = false;
    }
}
