using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Boot,
    WaitingToStart,
    Playing,
    Finished,
    Results
}

[DefaultExecutionOrder(-100)]
public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    public GameState State { get; private set; }

    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        SetState(GameState.WaitingToStart);
    }

    public void SetState(GameState newState)
    {
        State = newState;
        Debug.Log("Game State changed to: " + newState);
        OnStateChanged?.Invoke(newState);
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    public void FinishGame()
    {
        SetState(GameState.Finished);
    }

    public void ShowResults()
    {
        SetState(GameState.Results);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
