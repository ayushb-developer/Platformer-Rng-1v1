using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if(player == null)
            return;

        player.OnReachedFinish();
    }
}