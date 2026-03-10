using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject platformPrefab;
    [SerializeField] Transform startPoint;
    [SerializeField] PlayerController player;

    [Header("Level Settings")]
    [SerializeField] int platformCount = 40;

    [Header("Gap Settings")]
    [SerializeField] float minGap = 2f;
    [SerializeField] float gapMultiplier = 0.8f;

    [Header("Height Settings")]
    [SerializeField] float maxHeightChange = 2f;

    // float PlayerSpeed => player.PlayerVelocity;

    Vector3 lastPlatformPosition;
    float lastPlatformEndX;


    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Debug.Log("Generating Level...");
        lastPlatformPosition = startPoint.position;

        float maxJumpDistance = player.MaxJumpDistance();
        float safeGap = maxJumpDistance * gapMultiplier;
        
        GameObject firstPlatform = Instantiate(platformPrefab, startPoint.position, Quaternion.identity);
        BoxCollider2D firstCollider = firstPlatform.GetComponent<BoxCollider2D>();

        lastPlatformEndX = startPoint.position.x + firstCollider.bounds.extents.x;
        
        for (int i = 0; i < platformCount; i++)
        {
            float gap = Random.Range(minGap, safeGap);
            float heightOffset = Random.Range(-maxHeightChange, maxHeightChange);
            
            
            GameObject newPlatform = Instantiate(platformPrefab);

            BoxCollider2D collider = newPlatform.GetComponent<BoxCollider2D>();

            float halfWidth = collider.bounds.extents.x;

            float spawnX = lastPlatformEndX + gap + halfWidth;
            float spawnY = lastPlatformPosition.y + heightOffset;

            newPlatform.transform.position = new Vector3(spawnX, spawnY, 0);

            lastPlatformEndX = spawnX + halfWidth;
                
            // Vector3 spawnPos = new Vector3(
            //     lastPlatformPosition.x + gap,
            //     lastPlatformPosition.y + heightOffset,
            //     0
            // );

            // GameObject newPlatform = Instantiate(
            //     platformPrefab,
            //     spawnPos,
            //     Quaternion.identity
            // );
            // lastPlatformPosition = spawnPos;

            Debug.Log("Platform Spawned");
        }
    }

    // void OnDrawGizmos()
    // {
    //     if(!Application.isPlaying) return;

    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(startPoint.position, 
    //         startPoint.position + Vector3.right * player.MaxJumpDistance(player.PlayerVelocityX));
    // }
    // void SpawnPlatform(Vector3 position)
    // {
    //     Instantiate(platformPrefab, position, Quaternion.identity);
    // }
}