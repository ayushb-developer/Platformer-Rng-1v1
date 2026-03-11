using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings/Level Generation Settings")]
public class LevelGenerationSettings : ScriptableObject
{
    [Header("Platform Spawn")]
    public float spawnDistance = 30f;
    public float cleanupDistance = 20f;
    public int platformCount = 20;

    [Header("Gap")]
    public float minGap = 2f;
    public float gapMultiplier = 0.8f;

    [Header("Height")]
    public float maxHeightChange = 2f;
}
