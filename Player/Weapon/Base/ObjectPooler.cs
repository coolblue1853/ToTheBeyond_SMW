using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // 오브젝트 풀 관리자 
    public static ObjectPooler instance;
    private Dictionary<GameObject, Queue<GameObject>> _pools = new(); // 현재 있는 모든 풀 딕셔너리 
    private Dictionary<GameObject, List<GameObject>> _activeObjects = new(); // 현재 있는 모든 활성화된 풀에서 나온 오브젝트들 

    private void Awake()
    {
        instance = this;
    }

    // 풀 생성 
    public static void CreatePool(GameObject prefab, int size)
    {
        if (!instance._pools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = new();
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            instance._pools[prefab] = pool;
            instance._activeObjects[prefab] = new();
        }
    }

    private static GameObject SpawnInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!instance._pools.ContainsKey(prefab)) CreatePool(prefab, 5);
        var pool = instance._pools[prefab];
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        instance._activeObjects[prefab].Add(obj); // 추적용 리스트에 등록
        return obj;
    }

    // 풀에서 생성 
    public static GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        => SpawnInternal(prefab, position, rotation);

    public static GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation, int count)
    {
        if (!instance._pools.ContainsKey(prefab)) CreatePool(prefab, count);
        return SpawnInternal(prefab, position, rotation);
    }

    public static GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj = SpawnInternal(prefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localRotation = rotation;
        return obj;
    }


    // 풀로 반환 
    public static void ReturnToPool(GameObject prefab, GameObject obj)
    {
        if(obj == null)
            return;
        
        obj.SetActive(false);

        if (!instance._pools.ContainsKey(prefab))
        {
            instance._pools[prefab] = new Queue<GameObject>();
            instance._activeObjects[prefab] = new List<GameObject>();
        }

        if (instance._activeObjects[prefab].Contains(obj))
            instance._activeObjects[prefab].Remove(obj);

        instance._pools[prefab].Enqueue(obj);
    }
    
    /// 해당 프리팹으로 활성화된 모든 오브젝트를 풀로 반환
    public static void ReturnAllActiveObjects(GameObject prefab)
    {
        if (instance._activeObjects.TryGetValue(prefab, out var list))
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var obj = list[i];
                ReturnToPool(prefab, obj);
            }
            list.Clear(); // 안전하게 비움
        }
    }
    
    /// 모든 프리팹에 대해 활성화된 오브젝트 전부 반환
    public static void ReturnAllActiveObjects()
    {
        foreach (var kvp in instance._activeObjects)
        {
            var prefab = kvp.Key;
            ReturnAllActiveObjects(prefab);
        }
    }

    public static void DestroyPool(GameObject prefab)
    {
        if (instance._pools.TryGetValue(prefab, out var pool))
        {
            foreach (var obj in pool)
            {
                Destroy(obj);
            }
            instance._pools.Remove(prefab);
        }

        if (instance._activeObjects.TryGetValue(prefab, out var activeList))
        {
            foreach (var obj in activeList)
            {
                Destroy(obj);
            }
            instance._activeObjects.Remove(prefab);
        }
    }
}
