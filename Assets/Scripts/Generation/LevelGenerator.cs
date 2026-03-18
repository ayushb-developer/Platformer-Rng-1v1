using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

enum PatternStyle
{
    Random,
    Flat,
    StairsUp,
    StairsDown,
    WideGap
}

public class LevelGenerator : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    // [SerializeField] MonoBehaviour runTargetSource;
    [SerializeField] PlatformPool pool;
    [SerializeField] GameObject finishPlatformPrefab;

    [Header("Settings")]
    [SerializeField] LevelGenerationSettings settings;
    [SerializeField] DifficultySettings difficultySettings;
    [SerializeField] ObstacleSettings obstacleSettings;
    
    // PlayerController player;
    Queue<GameObject> activePlatforms = new();

    float lastPlatformY;
    float lastPlatformEndX;
    int platformsSpawned;
    bool finishSpawned;

    PatternStyle currentPattern = PatternStyle.Random;
    int patternRemaining;
    PlayerController leadingPlayer;

    PlayerController GetLeadingPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None); // TODO: optimize by caching references if needed

        PlayerController leader = null;
        float maxX = float.MinValue;

        foreach (var p in players)
        {
            if (!p.IsSpawned) continue;

            float x = p.transform.position.x;

            if (x > maxX)
            {
                maxX = x;
                leader = p;
            }
        }

        return leader;
    }
    float SafeGap => leadingPlayer.MaxJumpDistance * settings.gapMultiplier;
    bool running; // controlled by GameFlow

    // IRunTarget runTarget;


    // Difficulty value (0–1) based on distance travelled
    public float Difficulty
    {
        get
        {
            float distance = leadingPlayer.transform.position.x;
            float normalized = distance / difficultySettings.difficultyRampDistance;
            return difficultySettings.difficultyCurve.Evaluate(normalized);
        }
    }

//     void Awake()
// {
//     runTarget = runTargetSource as IRunTarget;

//     if (runTarget == null)
//         Debug.LogError("RunTargetSource must implement IRunTarget");
// }

    // void Start()
    // {
    //     if(!IsServer) return; // only let the server handle generation
    //     InitializeFirstPlatform();
    // }

    void OnEnable()
    {
        GameFlow.Instance.OnStateChanged += HandleState;
    }

    void OnDisable()
    {
        GameFlow.Instance.OnStateChanged -= HandleState;
    }

    void HandleState(GameState state)
    {
        running = state == GameState.Playing;
        if (running && IsServer)
        {
            Debug.Log("Level Generator Starting");
            InitializeFirstPlatform();
        }    
    }

    void InitializeFirstPlatform()
    {
        Debug.Log("Initialize First Platform");
        // GameObject firstPlatform = pool.GetPlatform();
        GameObject firstPlatform = Instantiate(settings.platformPrefab);
        firstPlatform.GetComponent<NetworkObject>().Spawn();
        firstPlatform.transform.position = startPoint.position;
        #if UNITY_EDITOR
        firstPlatform.name = $"First Platform";
        #endif
        activePlatforms.Enqueue(firstPlatform);

        BoxCollider2D collider = firstPlatform.GetComponent<BoxCollider2D>();
        lastPlatformEndX = startPoint.position.x + collider.bounds.extents.x;
        lastPlatformY = startPoint.position.y;

        platformsSpawned = 1;
    }

    void Update()
    {
        if (!IsServer) return; 
        if (!running) return;
        
        leadingPlayer = GetLeadingPlayer();
        if (leadingPlayer == null) return;

        while (lastPlatformEndX < leadingPlayer.transform.position.x + settings.spawnDistance && !finishSpawned)
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

        // GameObject platform = pool.GetPlatform();
        GameObject platform = Instantiate(settings.platformPrefab);

        #if UNITY_EDITOR
        platform.name = $"Platform {platformsSpawned}";
        #endif

        float lengthScale = Random.Range(settings.minPlatformLengthScale, settings.maxPlatformLengthScale);
        platform.transform.localScale = new Vector3(lengthScale, 1f, 1f);

        float halfWidth = lengthScale * 0.5f;
        float spawnX = lastPlatformEndX + gap + halfWidth;

        platform.transform.position = new Vector3(spawnX, spawnY, 0);
        activePlatforms.Enqueue(platform);

        platform.GetComponent<Platform>().SetVisual();
        platform.GetComponent<NetworkObject>().Spawn();

        lastPlatformEndX = spawnX + halfWidth;
        lastPlatformY = spawnY;

        platformsSpawned++;
        TrySpawnPlatformObstacle(platform, halfWidth);
        TrySpawnFlyingEnemy(lastPlatformEndX - gap - lengthScale, spawnX - halfWidth);
//        Debug.Log($"Spawned Platform {platformsSpawned} | X:{spawnX:F2} Y:{spawnY:F2} Gap:{gap:F2}");
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

        if (gap < settings.minGap) 
            gap = settings.minGap;
            
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
        float maxJumpHeight = leadingPlayer.Settings.jumpHeight * 0.8f;

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
        finish.GetComponent<NetworkObject>().Spawn();

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

            if (platform.transform.position.x < leadingPlayer.transform.position.x - settings.cleanupDistance)
            {
                // pool.ReturnPlatform(platform);
                if (platform.TryGetComponent(out NetworkObject netObj))
                {
                    netObj.Despawn();
                }
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
    PatternStyle next;

    do
    {
        next = (PatternStyle)Random.Range(0, 5);
    }
    while (next == PatternStyle.WideGap && currentPattern == PatternStyle.WideGap);

    currentPattern = next;
    patternRemaining = Random.Range(2,5);
}

void TrySpawnPlatformObstacle(GameObject platform, float halfWidth)
{
    float chance = Mathf.Lerp(
        obstacleSettings.basePlatformChance,
        obstacleSettings.maxPlatformChance,
        Difficulty
    );

    if (Random.value > chance) return;

    GameObject prefab = obstacleSettings.platformObstacles[
        Random.Range(0, obstacleSettings.platformObstacles.Length)
    ];

    GameObject obstacle = Instantiate(prefab);
    obstacle.GetComponent<NetworkObject>().Spawn();

    float minX = -halfWidth + obstacleSettings.minEdgeOffset;
    float maxX = halfWidth - obstacleSettings.minEdgeOffset;

    obstacle.transform.SetParent(platform.transform);
    float localX = Random.Range(minX, maxX)/ platform.transform.localScale.x; // adjust for platform scale


    obstacle.transform.localPosition = new Vector3(localX, 0.5f, 0);
    obstacle.GetComponent<Obstacle>().Initialize(leadingPlayer.transform, settings.cleanupDistance);
}

void TrySpawnFlyingEnemy(float previousPlatformEnd, float newPlatformStart)
{
    float chance = Mathf.Lerp(
        obstacleSettings.baseEnemyChance,
        obstacleSettings.maxEnemyChance,
        Difficulty
    );

    if (Random.value > chance) return;

    GameObject prefab = obstacleSettings.flyingEnemies[
        Random.Range(0, obstacleSettings.flyingEnemies.Length)
    ];

    float centerX = (previousPlatformEnd + newPlatformStart) * 0.5f;

    float enemyY = lastPlatformY + obstacleSettings.enemyHeightOffset;

    GameObject enemy = Instantiate(
        prefab,
        new Vector3(centerX, enemyY, 0),
        Quaternion.identity
    );
    enemy.GetComponent<NetworkObject>().Spawn();

    FlyingObstacle script = enemy.GetComponent<FlyingObstacle>();

    script.Initialize(leadingPlayer.transform, obstacleSettings.enemyCleanupDistance);
    script.SetPatrolRange(previousPlatformEnd, newPlatformStart);
}

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        if (leadingPlayer == null) return;

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