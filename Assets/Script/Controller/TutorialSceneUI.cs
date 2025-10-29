using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSceneUI : MonoBehaviour
{
    //[Header("Hand Images in Canvas")]
    //[SerializeField] private GameObject hand1Image;
    //[SerializeField] private GameObject hand2Image;

    [Header("Hand Durations")]
    [SerializeField] private float hand1Duration = 0.3f;
    [SerializeField] private float hand2Duration = 0.8f;

    [Header("Tutorial Sequence Data")]
    [SerializeField] private MessageGroupData[] tutorialSequence;


    [Header("Only Tutorial UI")]
    [SerializeField] private Button _btn_GotoTrainingScene;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private RectTransform _sliderHandle;

    private GameObject _lastPanel = null;
    private bool _isEnd = false;

    private void Awake()
    {
        //if (hand1Image != null) hand1Image.SetActive(false);
        //if (hand2Image != null) hand2Image.SetActive(false);
        
    }

    private void Start()
    {
        if (UIPanelFactory.Instance == null)
        {
            Debug.LogError("UIPanelFactory가 Scene에 없습니다!");
            return;
        }
        SetTutorialUI();
        StartCoroutine(RunTutorialSequence());
        //test
        //PlayTestMusic();
    }

    private void SetTutorialUI()
    {
        MainSystem.Instance.SoundController.SetAudioVolume(0, _volumeSlider.value);
        _btn_GotoTrainingScene.onClick.AddListener(GotoTrainingStage);
        _volumeSlider.onValueChanged.AddListener(OnControlVolume);
    }

    private void OnControlVolume(float _volume)
    {
        // slider handle 벗어남 방지 로직
        if(_volume < 0.051f)
        {
            _sliderHandle.anchoredPosition = new Vector2(-0.05f + (0.05f - _sliderHandle.anchorMin.x), 0f);
        }
        else
            _sliderHandle.anchoredPosition = new Vector2(-0.05f, 0f);
        MainSystem.Instance.SoundController.SetAudioVolume(0, _volume);
    }

    private void GotoTrainingStage()
    {
        if(_lastPanel != null)
            Destroy(_lastPanel);
        MainSystem.Instance.SoundController.StopMusic();
        SceneLoader.Instance.LoadScene("TrainingStage");
        
    }
    public void PlayTestMusic()
    {
        MainSystem.Instance.SoundController.PlayMusic("WhirlwindOfJoy");
    }

    private IEnumerator RunTutorialSequence()
    {
        bool isLast = false;
        for(int i = 0; i < tutorialSequence.Length; i++)
        {
            isLast = i >= tutorialSequence.Length - 1;
            if (tutorialSequence[i].extraContent.Length > 0)
            {
                for(int j = 0; j < tutorialSequence[i].extraContent.Length; j++)
                {
                    SetExtraContent(tutorialSequence[i].extraContent[j].extraContent, true, 
                    tutorialSequence[i].extraContent[j].pos, Quaternion.Euler(tutorialSequence[i].extraContent[j].rot));
                    tutorialSequence[i].extraContent[j].act_Start?.Invoke();
                
                }
            }
            
            for(int j = 0; j < tutorialSequence[i].messages.Length; j++)
            {
                if(isLast)
                {
                    _lastPanel = UIPanelFactory.Instance.ShowLastMessage(tutorialSequence[i].messages[j], tutorialSequence[i].isCenter);
                    PlayTestMusic();
                    yield break;
                }
                else
                    UIPanelFactory.Instance.ShowMessage(tutorialSequence[i].messages[j], tutorialSequence[i].isCenter);
                yield return new WaitUntil(() => UIPanelFactory.Instance.IsIdle);
                //SetExtraContent(tutorialSequence[i].extraContent[j].extraContent, false, Vector3.zero, Quaternion.identity);
            }
            for(int j = 0; j < tutorialSequence[i].extraContent.Length; j++)
            {
                SetExtraContent(tutorialSequence[i].extraContent[j].extraContent, false, Vector3.zero, Quaternion.identity);        
            }
        }

        // foreach (var group in tutorialSequence)
        // {
        //     // 메시지 표시
        //     foreach (var text in group.messages)
        //     {
        //         UIPanelFactory.Instance.ShowMessage(text, group.isCenter);
        //         if (group.extraContent.Length > 0)
        //         {
        //             for(int i = 0; i < group.extraContent.Length; i++)
        //             {
        //                 SetExtraContent(group.extraContent[i].extraContent, true, group.extraContent[i].pos, Quaternion.Euler(group.extraContent[i].rot));
        //                 group.extraContent[i].act_Start?.Invoke();
        //             //float duration = group.extraContent == hand1Image ? hand1Duration :
        //             //                 group.extraContent == hand2Image ? hand2Duration :
        //             //                 group.extraDuration;
        //             //yield return new WaitForSeconds(duration);
        //             //group.extraContent.SetActive(false);
                    
        //             }
        //         }
        //         yield return new WaitUntil(() => UIPanelFactory.Instance.IsIdle);
        //         for(int i = 0; i < group.extraContent.Length; i++)
        //             SetExtraContent(group.extraContent[i].extraContent, false, Vector3.zero, Quaternion.identity);
        //     }
        //     // TODO: 공, 손 이미지, 슬라이더 등 추가 해야 함. 
        //     // extraContent 표시 (손 이미지 등)
        //     //  if (group.extraContent != null)
        //     //  {
        //     //      float duration = group.extraContent == hand1Image ? hand1Duration :
        //     //                       group.extraContent == hand2Image ? hand2Duration :
        //     //                       group.extraDuration;
        //     //      //group.extraContent.SetActive(true);
        //     //      SetExtraContent(group.extraContent, true, Vector3.forward * 2);
        //     //      yield return new WaitForSeconds(duration);
        //     //      //group.extraContent.SetActive(false);
        //     //      SetExtraContent(group.extraContent, false, Vector3.zero);
        //     //  }
        // }
    }

    private void SetExtraContent(GameObject content, bool isEnable, Vector3 pos, Quaternion rot)
    {
        content.SetActive(isEnable);
        content.transform.position = pos;
        content.transform.rotation = rot;
    }
}
