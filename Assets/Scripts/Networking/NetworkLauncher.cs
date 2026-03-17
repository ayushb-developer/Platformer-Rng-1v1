using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;

    void Awake()
    {
        hostButton.onClick.AddListener(Host);
        joinButton.onClick.AddListener(Client);
    }
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void Host()
    {
        NetworkManager.Singleton.StartHost();
        // GameFlow.Instance.WaitForPlayers();
    }

    private void Client()
    {
        NetworkManager.Singleton.StartClient();
        // GameFlow.Instance.WaitForPlayers();
    }

    void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // GameObject player = Instantiate(playerPrefab);
        // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}