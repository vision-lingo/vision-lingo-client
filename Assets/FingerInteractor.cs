using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FingerInteractor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private MeshRenderer _interactedMesh;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger!");
        if(other.GetComponent<XRGrabInteractable>() != null)
        {
            Debug.Log("Trigger!222");
            _interactedMesh = other.GetComponent<MeshRenderer>();
            _interactedMesh.material.color = Color.magenta;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (_interactedMesh != null)
            _interactedMesh.material.color = Color.gray;
    }

}
