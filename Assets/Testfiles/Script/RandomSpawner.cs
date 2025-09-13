using UnityEngine;
using System.Collections;
using Stage.Config;

public class RandomSpawner : MonoBehaviour
{
    [Tooltip("보통 Main Camera. 비워두면 Start()에서 자동으로 찾음")]
    public Camera headCamera;

    [Tooltip("랜덤으로 생성할 프리팹")]
    public GameObject targetPrefab;

    [Tooltip("스포너가 따를 스테이지 설정 (StageConfig 에셋)")]
    public StageConfig stage;

    [Tooltip("생성된 오브젝트를 이 오브젝트의 자식으로 둘지")]
    public bool parentUnderSpawner = true;

    private Vector3 _basePos;
    private GameObject _current;

    void Start()
    {
        if (headCamera == null) headCamera = Camera.main;
        if (headCamera == null) {
            Debug.LogError("Head Camera를 지정하세요.");
            enabled = false;
            return;
        }

        if (targetPrefab == null) {
            Debug.LogError("Target Prefab을 지정하세요.");
            enabled = false;
            return;
        }

        if (stage == null)
        {
            Debug.LogError("Stage 에셋을 지정하세요.");
            enabled = false;
            return;
        }

        // 시작 시점의 카메라 위치를 기준점으로 고정
        // TODO: 사용자가 정면을 맞췄을 때 (ex. 게임 시작 버튼 누름) 기준으로 고정
        _basePos = headCamera.transform.position;

        // 첫 오브젝트를 생성하고, 이후부터는 코루틴으로 respawn
        SpawnOne();
        StartCoroutine(RespawnLoop());
    }

  private IEnumerator RespawnLoop()
    {
        while (true)
        {
            if (_current == null)
            {
                bool needDelay = stage.respawnDelay > 0f && 
                                (!stage.spawnImmediatelyAtStart && Time.frameCount < 3 
                                || stage.spawnImmediatelyAtStart);
                if (needDelay)
                    yield return new WaitForSeconds(stage.respawnDelay);

                SpawnOne();
            }
            yield return null;
        }
    }

  private void SpawnOne()
    {
        float r  = stage.SampleDistance();
        float az = stage.SampleAzimuth();
        float el = stage.SampleElevation();

        Vector3 worldDir = SphericalToCartesian(1f, az, el);
        Vector3 worldPos = _basePos + worldDir * r;

        Transform parent = parentUnderSpawner ? transform : null;
        _current = Instantiate(targetPrefab, worldPos, Quaternion.identity, parent);

        Destroy(_current, stage.targetLifetime);
    }

    public static Vector3 SphericalToCartesian(float r, float azDeg, float elDeg)
    {
        float az = azDeg * Mathf.Deg2Rad;
        float el = elDeg * Mathf.Deg2Rad;

        float x = r * Mathf.Cos(el) * Mathf.Sin(az);
        float y = r * Mathf.Sin(el);
        float z = r * Mathf.Cos(el) * Mathf.Cos(az);
        return new Vector3(x, y, z);
    }
}
