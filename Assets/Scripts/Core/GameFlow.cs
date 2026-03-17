using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Boot,
    WaitingForPlayers,
    WaitingToStart,
    Playing,
    Finished,
    Results
}

[DefaultExecutionOrder(-100)]
public class GameFlow : NetworkBehaviour
{
    public static GameFlow Instance { get; private set; }

    NetworkVariable<GameState> state = new NetworkVariable<GameState>();
    public GameState State => state.Value;

    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // BEFORE network → show lobby
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            OnStateChanged?.Invoke(GameState.Boot);
        }
    }

    // void Start()
    // {
    //     if(IsServer)
    //     {
    //         SetState(GameState.Boot);
    //     }
    // }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += OnStateUpdated;

        if (!IsServer) return;

        // When network starts → move to waiting
        SetState(GameState.WaitingForPlayers);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (playerCount >= 2)
        {
            SetState(GameState.WaitingToStart);
        }
    }
    void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        SetState(GameState.WaitingForPlayers);
    }

    void OnStateUpdated(GameState oldState, GameState newState)
    {
        Debug.Log("Game State changed to: " + newState);
        OnStateChanged?.Invoke(newState);
    }
    void SetState(GameState newState)
    {
        if (!IsServer) return;

        state.Value = newState;
    }

    public void StartGame()
    {
        if(!IsServer) return;

        SetState(GameState.Playing);
    }

    public void FinishGame()
    {
        if(!IsServer) return;
        
        SetState(GameState.Finished);
    }

    public void ShowResults()
    {
        if (!IsServer) return;
        
        SetState(GameState.Results);
    }

    public void Restart()
    {
        if (!IsServer) return;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WaitForPlayers()
    {
        // if(!IsServer) return;
        
        SetState(GameState.WaitingForPlayers);
    }

}
