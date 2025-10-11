using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CubeActions : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable _interactable;
    [SerializeField] private Material _mat;
    [SerializeField] private Color[] colors;
    private readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");
    private void Start()
    {
        _interactable.firstFocusEntered.AddListener(OnFirstFocusEntered);
        _interactable.firstHoverEntered.AddListener(OnFirstHover);
        _interactable.firstSelectEntered.AddListener(OnFirstSelect);
        
        _interactable.hoverEntered.AddListener(OnHover);
        _interactable.focusEntered.AddListener(OnFocus);
        _interactable.hoverExited.AddListener(OnHoverExit);
        _interactable.selectEntered.AddListener(OnSelect);
        _interactable.selectExited.AddListener(OnSelectExit);
        _interactable.activated.AddListener(OnActivate);


    }

    private void Update()
    {
        Debug.Log($"[CheckUpdate]:::: _interactable.canFocus: {_interactable.canFocus}");
    }

    private void OnFirstFocusEntered(FocusEnterEventArgs focusEnterEventArgs)
    {
        Debug.Log("[CheckInteraction]::::OnFirstFocusEntered");
    }
    private void OnFirstHover(HoverEnterEventArgs hoverEnterEventArgs)
    {
        _mat.color = colors[1];
        SetEmission(true, 100f);
        Debug.Log("[CheckInteraction]::::OnFirstHover");
    }
        public void OnFirstSelect(SelectEnterEventArgs selectEnterEventArgs)
    {
        _mat.color = colors[2];
        SetEmission(true, 100f);
        Debug.Log("[CheckInteraction]::::OnFirstSelect");
    }

    private void OnHoverExit(HoverExitEventArgs hoverExitEventArgs)
    {
        _mat.color = colors[0];
        SetEmission(false, 100f);
        Debug.Log("[CheckInteraction]::::OnHoverExit");
    }

    private void OnHover(HoverEnterEventArgs hoverEnterEventArgs)
    {
        _mat.color = colors[1];
        SetEmission(true, 100f);
        Debug.Log("[CheckInteraction]::::OnHover");
    }

    public void OnSelect(SelectEnterEventArgs selectEnterEventArgs)
    {
        _mat.color = colors[2];
        SetEmission(true, 100f);
        Debug.Log("[CheckInteraction]::::OnSelect");
    }
    public void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
    {
        _mat.color = colors[3];
        SetEmission(false, 100f);
        Debug.Log("[CheckInteraction]::::OnSelectExit");
    }
    public void OnFocus(FocusEnterEventArgs selectEnterEventArgs)
    {
        _mat.color = colors[4];
        Debug.Log("[CheckInteraction]::::OnFocus");
    }

    public void OnActivate(ActivateEventArgs activateEventArgs)
    {
        _mat.color = colors[5];
        Debug.Log("[CheckInteraction]::::OnActivate");
    }
    public void SetEmission(bool enable, float intensity)
    {
        if (enable)
        {
            // Emission 활성화
            _mat.EnableKeyword("_EMISSION");

            // 색상 * 강도를 Emission Color 속성에 설정
            Color finalColor = Color.white * intensity;
            _mat.SetColor("_EmissionColor", finalColor);
        }
        else
        {
            // Emission 비활성화
            _mat.DisableKeyword("_EMISSION");
            // 또는 강도를 0으로 설정하여 시각적으로 끄는 방법도 가능
            // material.SetColor(emissionColorID, Color.black);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[CheckInteraction]::::collision.gameObject.name: {collision.gameObject.name}");
    }

}
