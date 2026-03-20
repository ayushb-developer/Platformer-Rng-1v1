using UnityEngine;
[CreateAssetMenu(menuName = "GameSettings/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    public float baseSpeed = 6f;
    public float maxSpeed = 12f;

    [Header("Jump")]
    public float jumpHeight = 3f;
    public float timeToApex = 0.35f;
    [Space]
    public float deathY = -10f;
}
