using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if(!other.TryGetComponent<PlayerController>(out var player))
            return;

        player.OnReachedFinish();
    }
}