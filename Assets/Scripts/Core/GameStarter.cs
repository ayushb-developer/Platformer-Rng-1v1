using UnityEngine;
using UnityEngine.InputSystem;

public class GameStarter : MonoBehaviour
{
    void Update()
    {
        if (GameFlow.Instance.State != GameState.WaitingToStart)
            return;

        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        #else
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        #endif
        {
            GameFlow.Instance.StartGame();
        }
    }
}

