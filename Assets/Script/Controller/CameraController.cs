using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}
