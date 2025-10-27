using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
[RequireComponent(typeof(BoxCollider))]
public class XRUIInteractable : MonoBehaviour, IXRHeadInteractable
{
    [SerializeField] private Image _img_button;
    private Color _gray = new Color(0.2f, 0.2f, 0.2f, 0.0f);
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        RectTransform rect;
        BoxCollider boxCollider;
        if(!TryGetComponent(out rect))
        {
            Debug.LogError("Rect Transform is not found");
            return;
        }
        if(!TryGetComponent(out boxCollider))
        {
            Debug.LogError("boxCollider is not found");
            return;
        }
        if(!TryGetComponent(out _img_button))
        {
            Debug.LogError("_img_button is not found");
            return;
        }
        boxCollider.size = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, 1.0f);
    }

    public void OnRayOut()
    {
        _img_button.color += _gray;
        Debug.Log($"XRUIInteractor::::OnRayOut");
    }

    public void OnRayOver()
    {
        _img_button.color -= _gray;
         Debug.Log($"XRUIInteractor::::OnRayOver");
    }
}
