using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public class NetworkLauncher : MonoBehaviour
{
    // [SerializeField] GameObject playerPrefab;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField ipAddressField;
    [SerializeField] TMP_Text hostIPText;
    // void Awake()
    // {
    //     hostButton.onClick.AddListener(Host);
    //     joinButton.onClick.AddListener(Client);
    // }
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(ipAddressField.text));
        // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    async void JoinRelay(string joinCode)
    {
        Debug.Log($"Joining with code: {joinCode}");
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }

    async void CreateRelay()
    {
        Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log($"Join code: {joinCode}");
        hostIPText.gameObject.SetActive(true);
        hostIPText.text = "Host : " + joinCode;

        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
    }
    
    // async void OnApplicationQuit()
    // {
    //     if (NetworkManager.Singleton != null)
    //     {
    //         if (NetworkManager.Singleton.IsHost)
    //             NetworkManager.Singleton.StopHost();
    //         else if (NetworkManager.Singleton.IsClient)
    //             NetworkManager.Singleton.StopClient();
    //     }
    // }
    // void OnDestroy()
    // {
    //     if (NetworkManager.Singleton != null)
    //     {
    //         // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    //     }
    // }

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
        string ip = ipAddressField.text;
#if UNITY_EDITOR
        transport.ConnectionData.Address = "127.0.0.1";
#else
        if (!string.IsNullOrEmpty(ipAddressField.text))
            transport.ConnectionData.Address = ip;
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