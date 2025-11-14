using UnityEngine;

public sealed class Test_Merge_1Controller : MonoBehaviour
{
    public void OnReturnToMainButton() => SceneLoader.Instance.LoadLobby();
}
