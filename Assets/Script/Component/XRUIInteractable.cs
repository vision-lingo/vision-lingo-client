using UnityEngine;

public class XRUIInteractable : MonoBehaviour, IXRHeadInteractable
{

    public void OnRayOut()
    {
        Debug.Log($"XRUIInteractor::::OnRayOut");
    }

    public void OnRayOver()
    {
         Debug.Log($"XRUIInteractor::::OnRayOver");
    }
}
