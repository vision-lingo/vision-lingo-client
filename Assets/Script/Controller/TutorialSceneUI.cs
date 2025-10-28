using System.Collections;
using UnityEngine;

public class TutorialSceneUI : MonoBehaviour
{
    [Header("Hand Images in Canvas")]
    [SerializeField] private GameObject hand1Image;
    [SerializeField] private GameObject hand2Image;

    [Header("Hand Durations")]
    [SerializeField] private float hand1Duration = 0.3f;
    [SerializeField] private float hand2Duration = 0.8f;

    [Header("Tutorial Sequence Data")]
    [SerializeField] private MessageGroupData[] tutorialSequence;

    private void Awake()
    {
        if (hand1Image != null) hand1Image.SetActive(false);
        if (hand2Image != null) hand2Image.SetActive(false);
    }

    private void Start()
    {
        if (UIPanelFactory.Instance == null)
        {
            Debug.LogError("UIPanelFactory가 Scene에 없습니다!");
            return;
        }

        StartCoroutine(RunTutorialSequence());
    }

    private IEnumerator RunTutorialSequence()
    {
        foreach (var group in tutorialSequence)
        {
            // 메시지 표시
            foreach (var text in group.messages)
            {
                UIPanelFactory.Instance.ShowMessage(text, group.isCenter);
                yield return new WaitUntil(() => UIPanelFactory.Instance.IsIdle);
            }

            // extraContent 표시 (손 이미지 등)
            if (group.extraContent != null)
            {
                float duration = group.extraContent == hand1Image ? hand1Duration :
                                 group.extraContent == hand2Image ? hand2Duration :
                                 group.extraDuration;

                group.extraContent.SetActive(true);
                yield return new WaitForSeconds(duration);
                group.extraContent.SetActive(false);
            }
        }
    }
}
