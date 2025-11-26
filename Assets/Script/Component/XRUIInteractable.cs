using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using TMPro;
[RequireComponent(typeof(BoxCollider))]
public class XRUIInteractable : MonoBehaviour, IXRHeadInteractable
{
    [SerializeField] private Button _btn_interaction;
    [SerializeField] private Image _img_button;
    [SerializeField] private Color _hoverImgColor;
    [SerializeField] private TextMeshProUGUI _tmp_text;
    [SerializeField] private Color _hoverTxtColor;
    private Color _defaultImgColor;
    private Color _defaultTxtColor;
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
        if(!TryGetComponent(out rect))
        {
            Debug.LogError("Rect Transform is not found");
            return;
        }
        if(!TryGetComponent(out _btn_interaction))
        {
            Debug.LogError("_btn_interaction is not found");
            return;
        }
        _tmp_text = GetComponentInChildren<TextMeshProUGUI>();
        if(_tmp_text == null)
        {
            Debug.LogError($"({gameObject.name})_TextMeshProUGUI is not found");
            return;
        }
        boxCollider.size = new Vector3(rect.sizeDelta.x * 1.1f, rect.sizeDelta.y * 1.1f, 1.0f);

        _defaultImgColor = _img_button.color;
        _defaultTxtColor = _tmp_text.color;
    }

    public void OnRayOver()
    {
        _img_button.color = _hoverImgColor;
        _tmp_text.color = _hoverTxtColor;
    }

    public void OnRayOut()
    {
        _img_button.color = _defaultImgColor;
        _tmp_text.color = _defaultTxtColor;
    }

    public void OnSelect()
    {
        if(_btn_interaction != null)
            _btn_interaction.onClick?.Invoke();
    }
}
