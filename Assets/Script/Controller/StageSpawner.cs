using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageSpawner : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("보통 Main Camera. 비워두면 StageController에서 기준 위치를 설정")]
    public Camera HeadCamera;

    [Tooltip("생성할 프리팹")]
    public GameObject TargetPrefab;

    [Header("Stage / Spawn")]
    [Tooltip("스테이지 번호 (1~6)")]
    [Range(1, 6)]
    public int StageNumber = 1;

    [Tooltip("타겟까지의 기본 거리 (m)")]
    public float Distance = 30.0f;

    [Tooltip("스폰 기준을 시작 시 사용자가 바라본 방향(카메라)으로 할지, 월드 고정(Z+)로 할지")]
    public bool UseCameraBasis = true;

    [Tooltip("생성된 오브젝트를 이 오브젝트의 자식으로 둘지 여부")]
    public bool ParentUnderSpawner = true;

    [Header("Debug")]
    [Tooltip("스폰 시 좌표/각도 로그 출력")]
    public bool LogOnSpawn = true;

    private Vector3 _basePos;
    private Quaternion _baseRot;

    [System.Serializable]
    public class StagePosition
    {
        [Tooltip("스테이지 번호 (1~6)")]
        public int StageNumber;

        [Tooltip("이 스테이지에서 생성될 구의 각도 목록 (azimuth, elevation)")]
        public List<Vector2> Angles = new List<Vector2>(); // x=azimuth, y=elevation
    }

    [Header("Stage Data")]
    [Tooltip("스테이지별 오브젝트 각도 설정")]
    [SerializeField]
    private StagePosition[] StagePositions;

    void Start()
    {
        // 기본적으로 StageController가 카메라 기준을 설정하므로
        // 여기서는 초기값만 보조로 세팅
        if (HeadCamera == null) HeadCamera = Camera.main;
        if (HeadCamera != null)
        {
            _basePos = HeadCamera.transform.position;
            _baseRot = HeadCamera.transform.rotation;
        }
        else
        {
            Debug.LogWarning("[StageSpawner] HeadCamera가 설정되지 않았습니다.");
        }

        if (TargetPrefab == null)
        {
            Debug.LogError("[StageSpawner] Target Prefab을 지정하세요.");
            enabled = false;
        }
    }

    public List<GameObject> SpawnSet()
    {
        if (TargetPrefab == null)
        {
            Debug.LogError("[StageSpawner] TargetPrefab이 지정되지 않았습니다.");
            return new List<GameObject>();
        }

        // 현재 StageNumber에 맞는 StagePosition 찾기
        StagePosition stageData = null;
        foreach (var s in StagePositions)
        {
            if (s.StageNumber == StageNumber)
            {
                stageData = s;
                break;
            }
        }

        if (stageData == null)
        {
            Debug.LogError($"[StageSpawner] Stage {StageNumber}는 정의되어 있지 않습니다.");
            return new List<GameObject>();
        }

        var spawned = new List<GameObject>();

        for (int i = 0; i < stageData.Angles.Count; i++)
        {
            float az = stageData.Angles[i].x;
            float el = stageData.Angles[i].y;

            Vector3 localDir = SphericalToCartesian(1f, az, el);
            Vector3 worldDir = UseCameraBasis ? (_baseRot * localDir) : localDir;
            Vector3 worldPos = _basePos + worldDir * Distance;

            Transform parent = ParentUnderSpawner ? transform : null;
            var obj = Instantiate(TargetPrefab, worldPos, Quaternion.identity, parent);
            spawned.Add(obj);

            if (LogOnSpawn)
            {
                Debug.Log(
                    $"[StageSpawner] Stage {StageNumber} object idx {i} | az={az}°, el={el}°, r={Distance}m " +
                    $"| localDir={localDir} | worldPos={worldPos}"
                );
            }
        }

        return spawned;
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

    // 스테이지 번호 바꾸기
    public void SetStage(int stage)
    {
        bool found = false;
        foreach (var s in StagePositions)
        {
            if (s.StageNumber == stage)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogError($"[StageSpawner] Stage {stage} 없음");
            return;
        }

        StageNumber = stage;
    }

    // 카메라 기준 재설정 (라운드/스테이지 시작 시 호출)
    public void RebaseFromCamera(Vector3 camPos, Quaternion camRot)
    {
        _basePos = camPos;
        _baseRot = camRot;
    }
}
