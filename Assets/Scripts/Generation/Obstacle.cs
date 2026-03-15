using UnityEngine;

public class Obstacle : MonoBehaviour
{
    protected Transform cleanupReference;
    protected float cleanupDistance;

    protected virtual void Update()
    {
        CleanupCheck();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.TryGetComponent<PlayerController>(out var player)) return;

        Debug.Log("Player hit obstacle");
        player.OnHitObstacle();
    }

    void CleanupCheck()
    {
        if (cleanupReference == null) return;

        if (transform.position.x < cleanupReference.position.x - cleanupDistance)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Transform cleanupRef, float cleanupDist)
    {
        cleanupReference = cleanupRef;
        cleanupDistance = cleanupDist;
    }
}
