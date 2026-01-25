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
    private Color _defaultImgColor = Color.white;
    private Color _defaultTxtColor = Color.white;

    bool IXRHeadInteractable.IsInteractable { get => true; set {} }

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
        if (_tmp_text == null)
        {
            Debug.LogWarning($"({gameObject.name})_TextMeshProUGUI is not found");
            //return;
        }
        else
        {
            _defaultTxtColor = _tmp_text.color;
        }
        boxCollider.size = new Vector3(rect.sizeDelta.x * 1.1f, rect.sizeDelta.y * 1.1f, rect.sizeDelta.x < 1 ? 0.1f : 1.0f);
        if(_img_button != null)
            _defaultImgColor = _img_button.color;
    }

    public void OnRayOver()
    {
        if (_img_button != null)
            _img_button.color = _hoverImgColor;
        if(_tmp_text != null)
            _tmp_text.color = _hoverTxtColor;
    }

    public void OnRayOut()
    {
        if (_img_button != null)
            _img_button.color = _defaultImgColor;
        if(_tmp_text != null)
            _tmp_text.color = _defaultTxtColor;
    }

    public void OnSelect()
    {
        if(_btn_interaction != null)
            _btn_interaction.onClick?.Invoke();
    }
}
