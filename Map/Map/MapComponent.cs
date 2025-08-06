using System;
using UnityEngine;

public class MapComponent : MonoBehaviour
{
    public enum MapType { Tutorial, ShallowForest, DeepForest, Town, MiddleBoss, FinalBoss }

    public MapType mapType;
    
    public Transform playerSpawnPoint;
    public Collider2D cameraBounds;
    public WaveSpawner waveSpawner;
    public ZoneSpawner zoneSpawner;
    public PortalTrigger portal;
    public bool isTownMap = false; 
    public Transform rewardSpawnPoint;
    public Transform mapRoot;
    public GameObject backgroundPrefab;

    private void Awake()
    {
        mapRoot = transform;
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        if (portal != null)
        {
            portal.ResetPortal();
            if (!isTownMap)
                portal.gameObject.SetActive(false); // 일반 맵 포탈은 처음에 비활성화
        }

        if (isTownMap)
            return;

        if (waveSpawner != null)
        {
            Debug.Log("[MapComponent] WaveSpawner 실행");
            waveSpawner.StartWave();
        }
        else if (zoneSpawner != null)
        {
            Debug.Log("[MapComponent] ZoneSpawner 실행");
            zoneSpawner.StartZones();
        }
        else
        {
            Debug.LogWarning("[MapComponent] 스포너가 연결되어 있지 않습니다! (Wave or Zone)");
        }
    }




    public void Deactivate()
    {
        if (waveSpawner != null && waveSpawner.TryGetComponent(out EnemyAliveChecker waveChecker))
        {
            waveChecker.Clear();
        }

        if (zoneSpawner != null && zoneSpawner.TryGetComponent(out EnemyAliveChecker zoneChecker))
        {
            zoneChecker.Clear();
        }

        gameObject.SetActive(false);
    }


}