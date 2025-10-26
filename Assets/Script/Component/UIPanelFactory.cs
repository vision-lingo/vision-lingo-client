using UnityEngine;
using System.Collections.Generic;

public class UIPanelFactory : MonoBehaviour
{
    public static UIPanelFactory Instance { get; private set; }

    [SerializeField] private UIPanel panelPrefab;
    [SerializeField] private Transform uiParent;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void ShowMessage(string message)
    {
        messageQueue.Enqueue(message);
        if (!isShowing)
        {
            DisplayNextMessage();
        }
    }

    private void DisplayNextMessage()
    {
        if (messageQueue.Count == 0)
        {
            isShowing = false;
            return;
        }

        isShowing = true;
        string msg = messageQueue.Dequeue();
        UIPanel panel = Instantiate(panelPrefab, uiParent);
        panel.Show(msg, onComplete: DisplayNextMessage);
    }
}