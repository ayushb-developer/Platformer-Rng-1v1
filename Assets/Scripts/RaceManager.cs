using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance { get; private set; }

    List<PlayerController> finishedPlayers = new();
    Dictionary<ulong, bool> alivePlayers = new();

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
        if (!IsServer) return;

        if (state == GameState.Playing)
            raceStartTime = Time.time;
    }

    public void RegisterPlayer(ulong clientId)
    {
        // if (!IsServer) return;
        alivePlayers[clientId] = true;
    }

    public void RegisterDeath(ulong clientId)
    {
        alivePlayers[clientId] = false;
        CheckRaceEnd();
    }

    void CheckRaceEnd()
    {
        foreach (var alive in alivePlayers.Values)
        {
            if (alive) return; // still someone alive
        }

        Debug.Log("All players finished or dead. Ending race.");
        GameFlow.Instance.FinishGame();
    }

    public int RegisterFinish(PlayerController player)
    {
        if (!IsServer) return -1;
        if (player == null) return -1;

        if (finishedPlayers.Contains(player))
            return finishedPlayers.IndexOf(player) + 1;

        finishedPlayers.Add(player);

        int placement = finishedPlayers.Count;

        Debug.Log($"{player.name} finished place #{placement}");

        if (placement == 1)
            GameFlow.Instance.FinishGame();

        return placement;
    }

    public float GetFinishTime()
    {
        return Time.time - raceStartTime;
    }

    public int GetPlacement(PlayerController player)
    {
        if (player == null) return -1;

        // If already finished → use finish order
        if (finishedPlayers.Contains(player))
            return finishedPlayers.IndexOf(player) + 1;

        // Otherwise calculate live rank
        int rank = 1;

        foreach (var other in GetAllPlayers())
        {
            if (other == player) continue;

            if (other.transform.position.x > player.transform.position.x)
                rank++;
        }

        return rank;
    }
   List<PlayerController> GetAllPlayers()
    {
        return new List<PlayerController>(
            FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
        );
    }

    void Update()
    {
        if(!IsServer) return;

        var players = GetAllPlayers();

        float maxX = float.MinValue;
        PlayerController leader = null;

        foreach (var p in players)
        {
            if (p.transform.position.x > maxX)
            {
                maxX = p.transform.position.x;
                leader = p;
            }
        }

        LeadingPlayer = leader;
    }

    NetworkVariable<ulong> leadingClientId = new();

    public PlayerController LeadingPlayer
    {
        get
        {
            foreach (var p in GetAllPlayers())
            {
                if (p.OwnerClientId == leadingClientId.Value)
                    return p;
            }
            return null;
        }
        set
        {
            if (value == null)
            {
                leadingClientId.Value = 0;
            }
            else
            {
                leadingClientId.Value = value.OwnerClientId;
            }
        }
    }
}