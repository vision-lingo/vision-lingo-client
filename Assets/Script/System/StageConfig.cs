using UnityEngine;

namespace Stage.Config
{
    [CreateAssetMenu(menuName = "Stage Config", fileName = "Stage_")]
    public class StageConfig : ScriptableObject
    {
        [Header("Stage")]
        [Tooltip("식별용 이름 (예: T1, T2 ...)")]
        public string stageId = "T1";

        [Header("Distance")]
        [Min(0f)] public float minDistance = 1.0f;
        [Min(0f)] public float maxDistance = 2.0f;

        [Header("Azimuth")]
        [Range(-180f, 180f)] public float minAzimuth = -60f;
        [Range(-180f, 180f)] public float maxAzimuth = 60f;

        [Header("Elevation")]
        [Range(-90f, 90f)] public float minElevation = -10f;
        [Range(-90f, 90f)] public float maxElevation = 20f;

        [Header("Spwan")]
        [Min(0.1f)] public float targetLifetime = 3.0f;
        [Min(0f)]   public float respawnDelay  = 0.5f;
        public bool spawnImmediatelyAtStart = true; // 시작하자마자 첫 오브젝트 생성 여부

        public float SampleDistance()
        {
            return Random.Range(minDistance, maxDistance);
        }

        public float SampleAzimuth()
        {
            return Random.Range(minAzimuth, maxAzimuth);
        }

        public float SampleElevation()
        {
            return Random.Range(minElevation, maxElevation);
        }

        private void OnValidate()
        {
            if (maxDistance < minDistance) 
                maxDistance = minDistance + 0.01f;

            if (minAzimuth > maxAzimuth) 
            { 
                var t = minAzimuth; 
                minAzimuth = maxAzimuth; 
                maxAzimuth = t; 
            }

            if (minElevation > maxElevation) 
            { 
                var t = minElevation; 
                minElevation = maxElevation; 
                maxElevation = t; 
            }
        }
    }
}