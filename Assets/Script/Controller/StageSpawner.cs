using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageSpawner : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("보통 Main Camera. 비워두면 Start()에서 자동으로 찾음")]
    public Camera headCamera;

    [Tooltip("생성할 프리팹")]
    public GameObject targetPrefab;

    [Header("Stage / Spawn")]
    [Tooltip("스테이지 번호 (1~6)")]
    [Range(1, 6)]
    public int stageNumber = 1;

    [Tooltip("타겟까지의 기본 거리")]
    public float distance = 30.0f;

    [Tooltip("생성된 구를 유지할 시간(초) — 0 이하면 파괴하지 않음")]
    public float targetLifetime = -1f;

    [Tooltip("세트가 모두 사라진 후 다음 세트를 띄우기 전 대기(초)")]
    public float respawnDelay = 2f;

    [Tooltip("스폰 기준을 시작 시 사용자가 바라본 방향(카메라)으로 할지, 월드 고정(Z+)로 할지")]
    public bool useCameraBasis = true;

    [Tooltip("생성된 오브젝트를 이 오브젝트의 자식으로 둘지")]
    public bool parentUnderSpawner = true;

    [Header("Debug")]
    [Tooltip("스폰 시 좌표/각도 로그 출력")]
    public bool logOnSpawn = true;

    private Vector3 _basePos;
    private Quaternion _baseRot;
    private readonly List<GameObject> _instances = new List<GameObject>();

    private readonly Dictionary<int, List<(float az, float el)>> _stagePositions =
        new Dictionary<int, List<(float, float)>>
    {
        // T1: 전방 8개 (-80°~80°), el=0
        {1, new List<(float,float)>{(-80,0),(-60,0),(-40,0),(-20,0),(20,0),(40,0),(60,0),(80,0)} },

        // T2: 전방 왼쪽 (-45°), el = -40,-10,10,40
        {2, new List<(float,float)>{(-45,-40),(-45,-10),(-45,10),(-45,40)} },

        // T3: 전방 오른쪽 (45°), el = -40,-10,10,40
        {3, new List<(float,float)>{(45,-40),(45,-10),(45,10),(45,40)} },

        // T4: 후방 8개 (-100°~100°), el=0
        {4, new List<(float,float)>{(-100,0),(-120,0),(-140,0),(-160,0),(160,0),(140,0),(120,0),(100,0)} },

        // T5: 후방 왼쪽 (-135°), el = -40,-10,10,40
        {5, new List<(float,float)>{(-135,-40),(-135,-10),(-135,10),(-135,40)} },

        // T6: 후방 오른쪽 (135°), el = -40,-10,10,40
        {6, new List<(float,float)>{(135,-40),(135,-10),(135,10),(135,40)} },
    };

    void Start()
    {
        if (headCamera == null) headCamera = Camera.main;
        if (headCamera == null)
        {
            Debug.LogError("[StageSpawner] Head Camera를 지정하세요.");
            enabled = false;
            return;
        }
        if (targetPrefab == null)
        {
            Debug.LogError("[StageSpawner] Target Prefab을 지정하세요.");
            enabled = false;
            return;
        }
        if (!_stagePositions.ContainsKey(stageNumber))
        {
            Debug.LogError($"[StageSpawner] Stage {stageNumber}는 정의되어 있지 않습니다.");
            enabled = false;
            return;
        }

        // 시작 시점의 카메라 위치를 기준점으로 고정
        // TODO: 사용자가 정면을 맞췄을 때 (ex. 게임 시작 버튼 누름) 기준으로 고정
        _basePos = headCamera.transform.position;
        _baseRot = headCamera.transform.rotation;

        // 첫 오브젝트를 생성하고, 이후부터는 코루틴으로 respawn
        SpawnSet();
        StartCoroutine(RespawnLoop());
    }

    private IEnumerator RespawnLoop()
    {
        while (true)
        {
            _instances.RemoveAll(i => i == null);
            if (_instances.Count == 0)
            {
                if (respawnDelay > 0f)
                    yield return new WaitForSeconds(respawnDelay);
                SpawnSet();
            }
            yield return null;
        }
    }

    private void SpawnSet()
    {
        var list = _stagePositions[stageNumber];

        for (int i = 0; i < list.Count; i++)
        {
            var (az, el) = list[i];

            Vector3 localDir = SphericalToCartesian(1f, az, el);
            Vector3 worldDir = useCameraBasis ? (_baseRot * localDir) : localDir;
            Vector3 worldPos = _basePos + worldDir * distance;

            Transform parent = parentUnderSpawner ? transform : null;
            var obj = Instantiate(targetPrefab, worldPos, Quaternion.identity, parent);
            _instances.Add(obj);

            if (targetLifetime > 0f)
                Destroy(obj, targetLifetime);

            if (logOnSpawn)
            {
                Debug.Log(
                    $"[StageSpawner] Stage {stageNumber} object idx {i} | az={az}°, el={el}°, r={distance}m " +
                    $"| localDir={localDir} | worldPos={worldPos}"
                );
            }
        }

        // 랜덤으로 오브젝트 하나의 색상을 지정
        // TODO: 정답 오브젝트에 대해 별도 프리팹 만들어서 소리 & 빛 효과 지정
        if (_instances.Count > 0)
        {
            int randIdx = Random.Range(0, _instances.Count);
            GameObject specialObj = _instances[randIdx];

            Renderer rend = specialObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(rend.material);
                rend.material.color = Color.green;
            }
        }
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