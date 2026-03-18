using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;

public class NetworkLauncher : MonoBehaviour
{
    // [SerializeField] GameObject playerPrefab;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField ipAddressField;
    [SerializeField] TMP_Text hostIPText;

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
        hostIPText.gameObject.SetActive(true);
        hostIPText.text = "Host : " + GetLocalIP();
        var t = NetworkManager.Singleton.GetComponent<UnityTransport>();

        Debug.Log($"[HOST] Starting on {t.ConnectionData.Address}:{t.ConnectionData.Port}");

        NetworkManager.Singleton.StartHost();
        // GameFlow.Instance.WaitForPlayers();
    }


    string GetLocalIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "Not Found";
    }

    private void Client()
    {   
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string ip = "127.0.0.1";
#if UNITY_EDITOR
        transport.ConnectionData.Address = "127.0.0.1";
#else

        if (!string.IsNullOrEmpty(ipAddressField.text))
            transport.ConnectionData.Address = ipAddressField.text;
#endif
        Debug.Log($"[JOIN] Trying IP: {ip}:7777");
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