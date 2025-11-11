using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class XRButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Components")]
    [SerializeField] private Image _buttonImage;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _buttonIcon; // 버튼 내부 아이콘

    [Header("Color Settings")]
    [SerializeField] private Color _normalTextColor = Color.white;
    [SerializeField] private Color _normalBackgroundColor = new Color(0.369f, 0.369f, 0.369f, 0.18f); // #5E5E5E2E
    [SerializeField] private Color _normalIconColor = Color.white;

    [SerializeField] private Color _hoverTextColor = Color.white;
    [SerializeField] private Color _hoverBackgroundColor = new Color(0.369f, 0.369f, 0.369f, 0.70f); // #5E5E5EB3
    [SerializeField] private Color _hoverIconColor = Color.white;

    [SerializeField] private Color _pressTextColor = Color.black;
    [SerializeField] private Color _pressBackgroundColor = new Color(1f, 1f, 1f, 0.961f); // #FFFFFFF5
    [SerializeField] private Color _pressIconColor = Color.black;

    private bool _isHovered = false;
    private bool _isPressed = false;

    void Awake()
    {
        // 컴포넌트 자동 할당
        if (_buttonImage == null)
            _buttonImage = GetComponent<Image>();

        if (_buttonText == null)
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // 아이콘 자동 찾기 - "Image"라는 이름의 자식 오브젝트 찾기
        if (_buttonIcon == null)
        {
            Transform iconTransform = transform.Find("Image");
            if (iconTransform != null)
            {
                _buttonIcon = iconTransform.GetComponent<Image>();
            }
        }

        // 씬 파일에 저장된 오래된 값을 무시하고 코드 기본값 사용
        _normalTextColor = Color.white;
        _normalBackgroundColor = new Color(0.369f, 0.369f, 0.369f, 0.18f);
        _normalIconColor = Color.white;
        _hoverTextColor = Color.white;
        _hoverBackgroundColor = new Color(0.369f, 0.369f, 0.369f, 0.70f);
        _hoverIconColor = Color.white;
        _pressTextColor = Color.black;
        _pressBackgroundColor = new Color(1f, 1f, 1f, 0.961f);
        _pressIconColor = Color.black;
    }

    void Start()
    {
        // 초기 상태 설정
        SetNormalState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (!_isPressed)
        {
            SetHoverState();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _isPressed = false;
        SetNormalState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        SetPressState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        if (_isHovered)
        {
            SetHoverState();
        }
        else
        {
            SetNormalState();
        }
    }

    private void SetNormalState()
    {
        if (_buttonImage != null)
            _buttonImage.color = _normalBackgroundColor;

        if (_buttonText != null)
            _buttonText.color = _normalTextColor;

        if (_buttonIcon != null)
            _buttonIcon.color = _normalIconColor;
    }

    private void SetHoverState()
    {
        if (_buttonImage != null)
            _buttonImage.color = _hoverBackgroundColor;

        if (_buttonText != null)
            _buttonText.color = _hoverTextColor;

        if (_buttonIcon != null)
            _buttonIcon.color = _hoverIconColor;
    }

    private void SetPressState()
    {
        if (_buttonImage != null)
            _buttonImage.color = _pressBackgroundColor;

        if (_buttonText != null)
            _buttonText.color = _pressTextColor;

        if (_buttonIcon != null)
            _buttonIcon.color = _pressIconColor;
    }
}
