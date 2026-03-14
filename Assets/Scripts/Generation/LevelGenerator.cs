using System.Collections.Generic;
using UnityEngine;

enum PatternStyle
{
    Random,
    Flat,
    StairsUp,
    StairsDown,
    WideGap
}

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

    PatternStyle currentPattern = PatternStyle.Random;
    int patternRemaining = 0;

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
        if (patternRemaining <= 0)
        {
            PickPattern();
        }

        patternRemaining--;

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

        if (currentPattern == PatternStyle.WideGap)
        {
            gap = Mathf.Lerp(maxGap * 0.7f, maxGap, Random.value);
        }

        float heightRange = Mathf.Lerp(
            settings.maxHeightChange * difficultySettings.heightMultiplierAtMinDifficulty,
            settings.maxHeightChange,
            Difficulty
        );
        var heightOffset = currentPattern switch
        {
            PatternStyle.Flat => 0f,
            PatternStyle.StairsUp => heightRange * 0.5f,
            PatternStyle.StairsDown => -heightRange * 0.5f,
            _ => Random.Range(-heightRange, heightRange),
        };
        GameObject newPlatform = pool.GetPlatform();
        float lengthScale = Random.Range(settings.minPlatformLengthScale, settings.maxPlatformLengthScale);
        newPlatform.transform.localScale = new Vector3(lengthScale, 1f, 1f);

        // BoxCollider2D collider = newPlatform.GetComponent<BoxCollider2D>();
        // float halfWidth = collider.bounds.extents.x;

        float halfWidth = lengthScale * 0.5f;

        float spawnX = lastPlatformEndX + gap + halfWidth;
        float spawnY = lastPlatformY + heightOffset;
        float maxSafeJumpHeight = player.Settings.jumpHeight * 0.8f;
        heightOffset = Mathf.Lerp(0, heightOffset, 0.6f); // terrain smoothingt to avoid spikes
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
                platform.transform.localScale = Vector3.one;
                activePlatforms.Dequeue();
            }
            else
            {
                break;
            }
        }
    }


    void PickPattern()
    {
        currentPattern = (PatternStyle)Random.Range(0, 5);
        patternRemaining = Random.Range(2, 5);
    }
}