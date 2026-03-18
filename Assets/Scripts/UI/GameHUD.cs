using TMPro;
using Unity;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text rankText;
    PlayerController player;

    void Update()
    {
        if (GameFlow.Instance.State != GameState.Playing) return;

        player = PlayerController.LocalPlayer;
        if (player == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        distanceText.text = $"{player.transform.position.x:F0}m";

        int rank = RaceManager.Instance.GetPlacement(player);
        rankText.text = $"#{rank}";
        // if (IsOwner)

    }

    // public static PlayerController FindLocalPlayer()
    // {
    //     foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
    //     {
    //         if (player.IsOwner)
    //             return player;
    //     }

    //     return null;
    // }
}