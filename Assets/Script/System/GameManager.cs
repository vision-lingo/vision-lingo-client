using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance ?? throw new System.Exception("GameManager not initialized.");

    public int PlayerScore { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetData() => PlayerScore = 0;
    public void AddScore(int score) => PlayerScore += score;
}
