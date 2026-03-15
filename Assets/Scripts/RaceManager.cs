using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    List<PlayerController> finishedPlayers = new();

    public IReadOnlyList<PlayerController> FinishedPlayers => finishedPlayers;

    float raceStartTime;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameFlow.Instance.OnStateChanged += HandleState;
    }

    void OnDisable()
    {
        GameFlow.Instance.OnStateChanged -= HandleState;
    }

    void HandleState(GameState state)
    {
        if (state == GameState.Playing)
            raceStartTime = Time.time;
    }

    public int RegisterFinish(PlayerController player)
    {
        if (finishedPlayers.Contains(player))
            return finishedPlayers.IndexOf(player) + 1;

        finishedPlayers.Add(player);

        int placement = finishedPlayers.Count;

        Debug.Log($"{player.name} finished place #{placement}");

        // End race if first player (singleplayer)
        if (placement == 1)
            GameFlow.Instance.FinishGame();

        return placement;
    }

    public float GetFinishTime()
    {
        return Time.time - raceStartTime;
    }
}
