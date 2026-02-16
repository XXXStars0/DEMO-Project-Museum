using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public Button retryButton;
    public Button nextLevelButton;
    public Button menuButton;
    public GameObject timeOutText;

    private string _nextLevelScene;
    private string _mainMenuScene;

    void Awake()
    {
        retryButton.onClick.AddListener(OnRetry);
        nextLevelButton.onClick.AddListener(OnNextLevel);
        menuButton.onClick.AddListener(OnMenu);
    }

    public void ShowResults(string nextLevel, string mainMenu)
    {
        _nextLevelScene = nextLevel;
        _mainMenuScene = mainMenu;
    }

    public void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextLevel()
    {
        if (!string.IsNullOrEmpty(_nextLevelScene))
        {
            SceneManager.LoadScene(_nextLevelScene);
        }
        else
        {
            Debug.LogWarning("Next Level scene name is not set!");
        }
    }

    public void OnMenu()
    {
        SceneManager.LoadScene(_mainMenuScene);
    }
}