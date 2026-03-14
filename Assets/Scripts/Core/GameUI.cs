using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject resultsScreen;
    void OnEnable()
    {
        GameFlow.Instance.OnStateChanged += UpdateUI;
    }
    void OnDisable()
    {
        GameFlow.Instance.OnStateChanged -= UpdateUI;
    }
    void UpdateUI(GameState newState)
    {
        startScreen.SetActive(newState == GameState.WaitingToStart);
        gameOverScreen.SetActive(newState == GameState.Finished);
        resultsScreen.SetActive(newState == GameState.Results);
    }
}