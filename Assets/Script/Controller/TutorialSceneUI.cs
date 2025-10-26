using UnityEngine;

public class TutorialSceneUI : MonoBehaviour
{
    void Start()
    {
        // 메시지를 순서대로 보여줌
        UIPanelFactory.Instance.ShowMessage("소리 위치 훈련을 알려드리겠습니다.");
        UIPanelFactory.Instance.ShowMessage("소리가 나는 공을 선택해 주세요.");
        UIPanelFactory.Instance.ShowMessage("알맞은 공을 선택하면 공이 밝게 빛나게 됩니다.");
        UIPanelFactory.Instance.ShowMessage("소리가 나지 않는 공을 선택해 주세요.");
        UIPanelFactory.Instance.ShowMessage("잘못된 공을 선택하면 공이 회색으로 변합니다.");
        UIPanelFactory.Instance.ShowMessage("공을 선택하지 않고 10초가 지나면, 소리가 나는 공 주변의 색이 점차 변합니다.");
        UIPanelFactory.Instance.ShowMessage("공을 선택하면 변화된 색은 사라지게 됩니다.");
        UIPanelFactory.Instance.ShowMessage("스스로에게 알맞게 들리는 음량으로 조절해 주세요.");
        UIPanelFactory.Instance.ShowMessage("음량 조절 손잡이를 잡아 끌며 음량 조절 완료 후, 훈련을 시작해 주세요.");
        UIPanelFactory.Instance.ShowMessage("음량 조절 손잡이를 잡아 드래그하며 음량 조절 완료해 주세요.");
    }
}
