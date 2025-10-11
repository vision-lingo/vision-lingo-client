using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class EyeTrackingCheck : MonoBehaviour
{
    [SerializeField] private XRRayInteractor _interactor;
    [SerializeField] private XRGrabInteractable _interactable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _interactor.hoverEntered.AddListener(OnHover);
        _interactor.selectEntered.AddListener(OnSelect);
        _interactor.uiHoverEntered.AddListener(OnUIHover);

    }

    private void Update()
    {
        ARRaycastHit hit;
        if (_interactor.TryGetCurrentARRaycastHit(out hit))
        {
            Debug.Log($"[CheckUpdate]::::TryGetCurrentARRaycastHit Success{hit.trackable.gameObject.name}");
        }
        else
            Debug.Log($"[CheckUpdate]::::TryGetCurrentARRaycastHit Fail"); 
    }

    private void OnHover(HoverEnterEventArgs hoverEnterEventArgs)
    {
        Debug.Log($"[CheckInteraction]::::ON HOVER!::::name: {hoverEnterEventArgs.interactableObject.transform.gameObject.name}");
    }
    private void OnSelect(SelectEnterEventArgs selectEnterEventArgs)
    {
        Debug.Log($"[CheckInteraction]::::ON OnSelect!::::name: {selectEnterEventArgs.interactableObject.transform.gameObject.name}");
    }
    private void OnUIHover(UIHoverEventArgs uihoverEnterEventArgs)
    {
        Debug.Log($"[CheckInteraction]::::ON OnUIHover!::::name: {uihoverEnterEventArgs.interactorObject}");
    }
    

}
