using UnityEngine;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [SerializeField] private Button _btnSetVolume;
    [SerializeField] private Button _btnGotoLobby;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _btnGotoLobby.onClick.AddListener(()=> SceneLoader.Instance.LoadMain());
    }

}
