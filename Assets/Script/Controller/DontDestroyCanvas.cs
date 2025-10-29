using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour
{
    private static DontDestroyCanvas instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
