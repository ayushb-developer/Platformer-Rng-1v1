using UnityEngine;

public class Obstacle : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col)
    {
        PlayerController player = col.collider.GetComponent<PlayerController>();
        if( player== null) return;

        Debug.Log("Player hit spike");
        player.OnHitObstacle();
    }
}