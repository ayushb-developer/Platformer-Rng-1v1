using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    [SerializeField] PlayerController player;
    [SerializeField] PlatformPool pool;
    [SerializeField] GameObject finishPlatformPrefab;

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
    int platformsSpawned = 0;
    bool finishSpawned = false;

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
        platformsSpawned = 1;
    }

    void Update()
    {
        // if(finishSpawned) return;

        while(lastPlatformEndX < player.transform.position.x + spawnDistance && !finishSpawned)
        {
            SpawnNextPlatform();
        }
        CleanupOldestPlatform();
    }

    void SpawnNextPlatform()
    {
        if (platformsSpawned >= platformCount-1)
        {
            SpawnFinishPlatform();
            return;
        }
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
        platformsSpawned++;

        Debug.Log($"Spawned Platform {platformsSpawned} at X: {spawnX}, Y: {spawnY}, gap: {gap}, heightOffset: {heightOffset}");
    }

    void SpawnFinishPlatform()
    {
        GameObject finish = Instantiate(finishPlatformPrefab);

        BoxCollider2D collider = finish.GetComponent<BoxCollider2D>();
        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + minGap + halfWidth;
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