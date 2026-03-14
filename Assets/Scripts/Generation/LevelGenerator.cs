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

    [Header("Settings")]
    [SerializeField] LevelGenerationSettings settings;
    [SerializeField] DifficultySettings difficultySettings;

    Queue<GameObject> activePlatforms = new();

    float lastPlatformY;
    float lastPlatformEndX;
    int platformsSpawned;
    bool finishSpawned;

    PatternStyle currentPattern = PatternStyle.Random;
    int patternRemaining;

    float SafeGap => player.MaxJumpDistance * settings.gapMultiplier;

    // Difficulty value (0–1) based on distance travelled
    public float Difficulty
    {
        get
        {
            float distance = player.transform.position.x;
            float normalized = distance / difficultySettings.difficultyRampDistance;
            return difficultySettings.difficultyCurve.Evaluate(normalized);
        }
    }

    void Start()
    {
        InitializeFirstPlatform();
    }

    void InitializeFirstPlatform()
    {
        GameObject firstPlatform = pool.GetPlatform();
        firstPlatform.transform.position = startPoint.position;

        activePlatforms.Enqueue(firstPlatform);

        BoxCollider2D collider = firstPlatform.GetComponent<BoxCollider2D>();
        lastPlatformEndX = startPoint.position.x + collider.bounds.extents.x;
        lastPlatformY = startPoint.position.y;

        platformsSpawned = 1;
    }

    void Update()
    {
        while (lastPlatformEndX < player.transform.position.x + settings.spawnDistance && !finishSpawned)
        {
            SpawnNextPlatform();
        }

        CleanupOldestPlatform();
    }

    void SpawnNextPlatform()
    {
        if (patternRemaining <= 0)
            PickPattern();

        patternRemaining--;

        if (platformsSpawned >= settings.platformCount - 1)
        {
            SpawnFinishPlatform();
            return;
        }

        float gap = GenerateGap();
        float heightOffset = GenerateHeightOffset();

        // Terrain smoothing to avoid sudden spikes
        heightOffset = Mathf.Lerp(0f, heightOffset, 0.6f);

        float spawnY = CalculateSafeHeight(heightOffset);

        GameObject platform = pool.GetPlatform();

        float lengthScale = Random.Range(settings.minPlatformLengthScale, settings.maxPlatformLengthScale);
        platform.transform.localScale = new Vector3(lengthScale, 1f, 1f);

        float halfWidth = lengthScale * 0.5f;
        float spawnX = lastPlatformEndX + gap + halfWidth;

        platform.transform.position = new Vector3(spawnX, spawnY, 0);
        activePlatforms.Enqueue(platform);

        lastPlatformEndX = spawnX + halfWidth;
        lastPlatformY = spawnY;

        platformsSpawned++;

        Debug.Log($"Spawned Platform {platformsSpawned} | X:{spawnX:F2} Y:{spawnY:F2} Gap:{gap:F2}");
    }

    float GenerateGap()
    {
        float maxGap = Mathf.Lerp(
            SafeGap * difficultySettings.gapMultiplierAtMinDifficulty,
            SafeGap,
            Difficulty
        );

        float gap = Random.Range(settings.minGap, maxGap);

        if (currentPattern == PatternStyle.WideGap)
            gap = Mathf.Lerp(maxGap * 0.7f, maxGap, Random.value);

        return gap;
    }

    float GenerateHeightOffset()
    {
        float heightRange = Mathf.Lerp(
            settings.maxHeightChange * difficultySettings.heightMultiplierAtMinDifficulty,
            settings.maxHeightChange,
            Difficulty
        );

        return currentPattern switch
        {
            PatternStyle.Flat => 0f,
            PatternStyle.StairsUp => heightRange * 0.5f,
            PatternStyle.StairsDown => -heightRange * 0.5f,
            _ => Random.Range(-heightRange, heightRange)
        };
    }

    float CalculateSafeHeight(float heightOffset)
    {
        float maxJumpHeight = player.Settings.jumpHeight * 0.8f;

        float spawnY = lastPlatformY + heightOffset;

        return Mathf.Clamp(
            spawnY,
            lastPlatformY - maxJumpHeight,
            lastPlatformY + maxJumpHeight
        );
    }

    void SpawnFinishPlatform()
    {
        GameObject finish = Instantiate(finishPlatformPrefab);

        BoxCollider2D collider = finish.GetComponent<BoxCollider2D>();
        float halfWidth = collider.bounds.extents.x;

        float spawnX = lastPlatformEndX + settings.minGap + halfWidth;

        finish.transform.position = new Vector3(spawnX, lastPlatformY, 0);

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
                platform.transform.localScale = Vector3.one; // reset pooled platform
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

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 28;
        style.alignment = TextAnchor.MiddleCenter;
        style.padding = new RectOffset(10, 10, 10, 10);

        GUILayout.BeginArea(new Rect(10, 10, 260, 60));
        GUILayout.Box($"Difficulty: {Difficulty:F2}", style, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.EndArea();
    }
#endif
}