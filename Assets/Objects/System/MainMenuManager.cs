using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string firstLevelSceneName = "Scene_Level_0";

    [Header("Tutorial")]
    public GameObject menuMain;
    public GameObject tutorialPanel;

    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            menuMain.SetActive(false);
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            menuMain.SetActive(true);
        }
    }

    //
    public void OnQuitButtonPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}