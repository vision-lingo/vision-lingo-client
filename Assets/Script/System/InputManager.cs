using UnityEngine;

public sealed class InputManager : MonoBehaviour
{
    private void Awake() => DontDestroyOnLoad(gameObject);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneLoader.Instance.LoadMain();
    }
}
