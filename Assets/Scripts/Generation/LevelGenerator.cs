using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    [SerializeField] PlayerController player;
    [SerializeField] PlatformPool pool;

    [Header("Level Settings")]
    [SerializeField] int platformCount = 40;
    [SerializeField] float cleanupDistance = 20f;
    [SerializeField] float spawnDistance = 30f;

    [Header("Gap Settings")]
    [SerializeField] float minGap = 2f;
    [SerializeField] float gapMultiplier = 0.8f;

    [Header("Height Settings")]
    [SerializeField] float maxHeightChange = 2f;
    
    float lastPlatformY;
    float lastPlatformEndX;
    // List<GameObject> activePlatforms = new();
    Queue<GameObject> activePlatforms = new();

    float SafeGap => player.MaxJumpDistance * gapMultiplier;

    void Start()
    {
        InitalizeFirstPlatform();
    }

    void InitalizeFirstPlatform()
    {
        GameObject firstPlatform = pool.GetPlatform();
        firstPlatform.transform.position = startPoint.position;
        activePlatforms.Enqueue(firstPlatform);
        
        BoxCollider2D firstCollider = firstPlatform.GetComponent<BoxCollider2D>();
        lastPlatformEndX = startPoint.position.x + firstCollider.bounds.extents.x;
        lastPlatformY = startPoint.position.y;
    }

    void Update()
    {
        if(lastPlatformEndX < player.transform.position.x + spawnDistance)
        {
            SpawnNextPlatform();
        }
        CleanupOldestPlatform();
    }

    void SpawnNextPlatform()
    {
        float gap = Random.Range(minGap, SafeGap);
        float heightOffset = Random.Range(-maxHeightChange, maxHeightChange);
        
        GameObject newPlatform = pool.GetPlatform();
        BoxCollider2D collider = newPlatform.GetComponent<BoxCollider2D>();

        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + gap + halfWidth;
        float spawnY = lastPlatformY + heightOffset;

        newPlatform.transform.position = new Vector3(spawnX, spawnY, 0);
        activePlatforms.Enqueue(newPlatform);

        lastPlatformEndX = spawnX + halfWidth;
        lastPlatformY = spawnY;
        Debug.Log("Platform Spawned");
    }

    void CleanupOldestPlatform()
    {
        while (activePlatforms.Count > 0)
        {
            GameObject platform = activePlatforms.Peek();

            if (platform.transform.position.x < player.transform.position.x - cleanupDistance)
            {
                pool.ReturnPlatform(platform);
                activePlatforms.Dequeue();
            }
            else
            {
                break;
            }
        }
    }
}