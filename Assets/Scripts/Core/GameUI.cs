using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    [SerializeField] GameObject startScreen;
    // [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject resultsScreen;
    [SerializeField] GameObject waitingForPlayersScreen;
    [SerializeField] GameObject lobbyScreen; //host/join buttons
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] GameObject spectateText;

    [SerializeField] Button restartButton;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        restartButton.onClick.AddListener(OnRestartClicked);
        spectateText.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.OnStateChanged += UpdateUI;

            // Force initial sync
            UpdateUI(GameFlow.Instance.State);
        }
    }

    void OnDestroy()
    {
        if (GameFlow.Instance != null)
            GameFlow.Instance.OnStateChanged -= UpdateUI;
    }

    void UpdateUI(GameState newGameState)
    {
        lobbyScreen.SetActive(newGameState == GameState.Boot);
        waitingForPlayersScreen.SetActive(newGameState == GameState.WaitingForPlayers);
        startScreen.SetActive(newGameState == GameState.WaitingToStart);
        // gameOverScreen.SetActive(newGameState == GameState.Finished);
        // timeText.text = $"Time: {RaceManager.Instance.GetFinishTime():F2}s";
        resultsScreen.SetActive(newGameState == GameState.Results);
        // restartButton.interactable = NetworkManager.Singleton.IsServer; //only allow restart for host

        if( newGameState == GameState.Finished)
        {
            ShowLocalResult();
        }
        // {
        //     bool localPlayerWon = RaceManager.Instance.GetWinner() == PlayerController.LocalPlayer;
        //     winScreen.SetActive(localPlayerWon);
        //     loseScreen.SetActive(!localPlayerWon);
        // }
        // else
        // {
        //     winScreen.SetActive(false);
        //     loseScreen.SetActive(false);
        // }
    }

    public void ShowLocalResult()
    {
        var player = PlayerController.LocalPlayer;

        if (player == null) return;

        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        spectateText.SetActive(false);

        switch (player.MyPlayerState)
        {
            case PlayerState.Finished:
                winScreen.SetActive(true);
                spectateText.SetActive(true);
                break;

            case PlayerState.Dead:
                loseScreen.SetActive(true);
                spectateText.SetActive(true);
                break;

            // default:
            //     spectateText.SetActive(true);
            //     break;
        }
    }

    void OnRestartClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameFlow.Instance.Restart();
        }
        else
        {
            RequestRestartServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestRestartServerRpc()
    {
        GameFlow.Instance.Restart();
    }
}