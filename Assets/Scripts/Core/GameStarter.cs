using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Starts the race when server player taps the screen or presses space. Also listens for other clients requesting to start the game and starts it if requested.
/// </summary>
public class GameStarter : MonoBehaviour
{
    void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
    return;
        if (GameFlow.Instance.State != GameState.WaitingToStart)
            return;

        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        #else
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        #endif
        {
            TryStartGame();
        }
    }

    private void TryStartGame()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            GameFlow.Instance.StartGame();
        }
        else
        {
            RequestStartServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestStartServerRpc()
    {
        GameFlow.Instance.StartGame();
    }
}

