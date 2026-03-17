using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject resultsScreen;
    [SerializeField] GameObject waitingForPlayersScreen;
    [SerializeField] GameObject lobbyScreen; //host/join buttons

    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] Button restartButton;

    void Start()
    {
        restartButton.onClick.AddListener(OnRestartClicked);
        restartButton.interactable = NetworkManager.Singleton.IsServer; //only allow restart for host

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

    void UpdateUI(GameState newState)
    {
        lobbyScreen.SetActive(newState == GameState.Boot);
        waitingForPlayersScreen.SetActive(newState == GameState.WaitingForPlayers);
        startScreen.SetActive(newState == GameState.WaitingToStart);
        gameOverScreen.SetActive(newState == GameState.Finished);
        // timeText.text = $"Time: {RaceManager.Instance.GetFinishTime():F2}s";
        resultsScreen.SetActive(newState == GameState.Results);
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