using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] Button titleScreenTap;

    void Awake()
    {
        titleScreenTap.onClick.AddListener(OnTapTitleScreen);
    }

    private void OnTapTitleScreen()
    {
        SceneManager.LoadScene(1);
    }
}
