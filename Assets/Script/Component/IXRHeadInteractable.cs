
using UnityEngine;

public interface IXRHeadInteractable
{
    public bool IsInteractable { get; set; }
    public abstract void OnRayOver();
    public abstract void OnRayOut();
    public abstract void OnSelect();
}
