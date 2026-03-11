using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    [SerializeField] PlayerController player;
    [SerializeField] PlatformPool pool;
    [SerializeField] GameObject finishPlatformPrefab;

    [SerializeField] LevelGenerationSettings settings;
    
    float lastPlatformY;
    float lastPlatformEndX;
    int platformsSpawned = 0;
    bool finishSpawned = false;

    Queue<GameObject> activePlatforms = new();

    float SafeGap => player.MaxJumpDistance * settings.gapMultiplier;

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
        platformsSpawned = 1;
    }

    void Update()
    {
        // if(finishSpawned) return;

        while(lastPlatformEndX < player.transform.position.x + settings.spawnDistance && !finishSpawned)
        {
            SpawnNextPlatform();
        }
        CleanupOldestPlatform();
    }

    void SpawnNextPlatform()
    {
        if (platformsSpawned >= settings.platformCount-1)
        {
            SpawnFinishPlatform();
            return;
        }
        float gap = Random.Range(settings.minGap, SafeGap);
        float heightOffset = Random.Range(-settings.maxHeightChange, settings.maxHeightChange);
        
        GameObject newPlatform = pool.GetPlatform();
        BoxCollider2D collider = newPlatform.GetComponent<BoxCollider2D>();

        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + gap + halfWidth;
        float spawnY = lastPlatformY + heightOffset;

        newPlatform.transform.position = new Vector3(spawnX, spawnY, 0);
        activePlatforms.Enqueue(newPlatform);

        lastPlatformEndX = spawnX + halfWidth;
        lastPlatformY = spawnY;
        platformsSpawned++;

        Debug.Log($"Spawned Platform {platformsSpawned} at X: {spawnX}, Y: {spawnY}, gap: {gap}, heightOffset: {heightOffset}");
    }

    void SpawnFinishPlatform()
    {
        GameObject finish = Instantiate(finishPlatformPrefab);

        BoxCollider2D collider = finish.GetComponent<BoxCollider2D>();
        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + settings.minGap + halfWidth;
        float spawnY = lastPlatformY;

        finish.transform.position = new Vector3(spawnX, spawnY, 0);

        finishSpawned = true;
        platformsSpawned++;

        Debug.Log("Finish Platform Spawned");
    }

    void CleanupOldestPlatform()
    {
        while (activePlatforms.Count > 0)
        {
            GameObject platform = activePlatforms.Peek();

            if (platform.transform.position.x < player.transform.position.x - settings.cleanupDistance)
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