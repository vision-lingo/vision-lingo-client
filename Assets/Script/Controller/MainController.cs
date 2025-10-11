using UnityEngine;

public sealed class MainController : MonoBehaviour
{
    public void OnGoToTest_Merge_1Button() => SceneLoader.Instance.LoadScene("Test_Merge_1");
    public void OnQuitButton() => SceneLoader.Instance.QuitGame();
}
