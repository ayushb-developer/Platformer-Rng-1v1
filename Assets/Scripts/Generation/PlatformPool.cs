using System.Collections.Generic;
using UnityEngine;

public class PlatformPool : MonoBehaviour
{
    [SerializeField] GameObject platformPrefab;
    [SerializeField] int initialPoolSize = 50;

    Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject platform = Instantiate(platformPrefab);
            platform.SetActive(false);
            pool.Enqueue(platform);
        }
    }

    public GameObject GetPlatform()
    {
        if (pool.Count > 0)
        {
            GameObject platform = pool.Dequeue();
            platform.SetActive(true);
            return platform;
        }

        // fallback if pool empty
        return Instantiate(platformPrefab);
    }

    public void ReturnPlatform(GameObject platform)
    {
        platform.SetActive(false);
        pool.Enqueue(platform);
    }
}