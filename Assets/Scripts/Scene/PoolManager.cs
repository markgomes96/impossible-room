using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    // *** Singleton
    public static PoolManager instance;

    private void Awake()
    {
        instance = this;
        SetupPools();
    }
    // ***

    public List<Pool> objectPools;
    public Dictionary<string, Queue<GameObject>> poolDict;

    void SetupPools()
    {
        poolDict = new Dictionary<string, Queue<GameObject>>();
        // spawn all gameobject in each pool
        foreach (Pool pool in objectPools)
        {
            // create queue of gameobjects
            Queue<GameObject> objectPool = new Queue<GameObject>();
            // fill queue of spawns of prefab
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
                // add object as child
                obj.transform.parent = transform;
            }
            // add queue to dictionary
            poolDict.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // check if tag exist
        if (!poolDict.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " does not exist.");
            return null;
        }
        // get next object in queue
        GameObject objectSpawn = poolDict[tag].Dequeue();
        objectSpawn.SetActive(true);
        objectSpawn.transform.position = position;
        objectSpawn.transform.rotation = rotation;
        // place object at end of queue
        poolDict[tag].Enqueue(objectSpawn);

        return objectSpawn;
    }
}
