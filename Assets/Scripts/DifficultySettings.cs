using UnityEngine;  

[CreateAssetMenu(menuName = "GameSettings/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Distance Scaling")]
    public float difficultyRampDistance = 200f;

    [Header("Gap Difficulty")]
   public float gapMultiplierAtMinDifficulty = 0.6f;

    [Header("Height Difficulty")]
    public float heightMultiplierAtMinDifficulty = 0.4f;

    [Header("Speed")]
    public float maxSpeedMultiplier = 1.5f;

    [Header("Curve")]
    public AnimationCurve difficultyCurve;
}
