using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings/Obstacle Settings")]
public class ObstacleSettings : ScriptableObject
{
    [Header("Platform Obstacles")]
    public GameObject[] platformObstacles;
    [Range(0,1)] public float basePlatformChance = 0.25f;
    [Range(0,1)] public float maxPlatformChance = 0.6f;

    [Header("Flying Enemies")]
    public GameObject[] flyingEnemies;
    [Range(0,1)] public float baseEnemyChance = 0.15f;
    [Range(0,1)] public float maxEnemyChance = 0.4f;

    [Header("Placement")]
    public float minEdgeOffset = 0.6f;
    public float enemyHeightOffset = 2f;
}