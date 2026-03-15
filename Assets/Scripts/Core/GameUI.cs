using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject resultsScreen;

    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] Button restartButton;

    void Start()
    {
        restartButton.onClick.AddListener(() => GameFlow.Instance.Restart());
    } 
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