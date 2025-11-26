using UnityEngine;

/// <summary>
/// 이 게임 오브젝트와 자식에 포함된 모든 파티클 시스템을 제어합니다.
/// MainSystem의 Pause/Resume 액션에 연동하여 전체 일시정지/재개를 지원합니다.
/// </summary>
public class ParticleController : MonoBehaviour
{

    private ParticleSystem[] _particleSystems;

    private void Awake()
    {
        // 이 게임 오브젝트와 모든 자식 오브젝트의 파티클 시스템을 참조합니다.
        _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    /// <summary>
    /// 모든 파티클을 일시정지합니다.
    /// </summary>
    public void PauseParticles()
    {
        foreach (var ps in _particleSystems)
        {
            if (ps != null && ps.isPlaying)
            {
                ps.Pause();
            }
        }
    }

    /// <summary>
    /// 모든 파티클을 다시 재생합니다.
    /// </summary>
    public void ResumeParticles()
    {
        foreach (var ps in _particleSystems)
        {
            if (ps != null && ps.isPaused)
            {
                ps.Play();
            }
        }
    }
}
