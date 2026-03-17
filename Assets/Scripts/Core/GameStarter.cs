using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

