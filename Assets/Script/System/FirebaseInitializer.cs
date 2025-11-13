using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    public static bool IsReady { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitFirebase();
    }

    private void InitFirebase()
    {
        // Unity Editor에서는 Firebase Analytics를 사용하지 않음
#if UNITY_EDITOR
        IsReady = false;
        return;
#endif

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                IsReady = false;
                return;
            }

            var dep = task.Result;
            if (dep == DependencyStatus.Available)
            {
                try
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    IsReady = true;
                }
                catch
                {
                    IsReady = false;
                }
            }
            else
            {
                IsReady = false;
            }
        });
    }
}
