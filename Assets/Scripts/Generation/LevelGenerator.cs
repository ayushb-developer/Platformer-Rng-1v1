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
    [SerializeField] DifficultySettings difficultySettings;
    public float Difficulty
    {
        get
        {
            float distance = player.transform.position.x;

            float normalized = distance / difficultySettings.difficultyRampDistance;

            return difficultySettings.difficultyCurve.Evaluate(normalized);
        }
    }
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

        float maxGap = Mathf.Lerp(
            SafeGap * difficultySettings.gapMultiplierAtMinDifficulty,
            SafeGap,
            Difficulty
            );
        float gap = Random.Range(settings.minGap, maxGap);

        float heightRange = Mathf.Lerp(
            settings.maxHeightChange * difficultySettings.heightMultiplierAtMinDifficulty,
            settings.maxHeightChange,
            Difficulty
        );

        float heightOffset = Random.Range(-heightRange, heightRange);
        GameObject newPlatform = pool.GetPlatform();
        BoxCollider2D collider = newPlatform.GetComponent<BoxCollider2D>();

        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + gap + halfWidth;
        float spawnY = lastPlatformY + heightOffset;
        float maxSafeJumpHeight = player.Settings.jumpHeight * 0.8f;
        spawnY = Mathf.Clamp(
            spawnY,
            lastPlatformY - maxSafeJumpHeight,
            lastPlatformY + maxSafeJumpHeight
        );

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