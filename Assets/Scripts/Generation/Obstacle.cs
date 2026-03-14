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
        PlayerController player = col.collider.GetComponent<PlayerController>();
        if (player == null) return;

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
