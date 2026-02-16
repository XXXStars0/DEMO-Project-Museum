using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for loading scenes

public class LevelManager : MonoBehaviour
{
    [Header("Level Config")]
    public int level;

    [Header("Scene Management")]
    public string nextLevelSceneName;
    public string mainMenuSceneName = "Scene_MainMenu";

    [Header("UI References")]
    public GameObject tutorialPanel;
    public GameObject UI_HUD;
    public GameObject Item_HUD;
    public GameObject UI_Bubbles;
    public CheckPointManager checkPointManager;
    public GameObject UI_result;
    public ResultsPanelUI resultsPanelUI;

    [Header("Pause References")]
    public GameObject UI_Pause;
    public Button Btn_Pause;
    public Image pauseButtonImage;
    public Sprite pauseIcon;
    public Sprite resumeIcon;
    public TextMeshProUGUI pauseText;

    [Header("Other References")]
    public PlayerController playerController;

    public static Transform BubbleCanvas { get; private set; }

    private bool _levelEnded = false;
    private bool _isPaused = false;

    void Awake()
    {
        if (UI_Bubbles != null)
        {
            UI_Bubbles.SetActive(true);
            BubbleCanvas = UI_Bubbles.transform;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        OpenTutorial();
        UI_HUD.SetActive(false);
        UI_result.SetActive(false);
        UI_Pause.SetActive(false);
        Item_HUD.SetActive(true);
        UI_Pause.GetComponent<PausePanelUI>().SetMainMenu(mainMenuSceneName);


        //Need Redo
        pauseText.text = $"[{playerController.keyPause}]";

        Btn_Pause.onClick.AddListener(OnPauseButtonPressed);
    }

    void Update()
    {
        if (!_levelEnded && Input.GetKeyDown(playerController.keyPause))
        {
            TogglePause();
        }

        if (_isPaused || _levelEnded) return;

        bool allTasksComplete = checkPointManager.AreAllTasksComplete;

        if (allTasksComplete)
        {
            EndLevel(true);
        }
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            
            Time.timeScale = 0f; 
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            UI_HUD.SetActive(true);
            Time.timeScale = 1f;
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        UI_Bubbles.SetActive(!_isPaused);
        Item_HUD.SetActive(!_isPaused);
        UI_Pause.SetActive(_isPaused);

        Time.timeScale = _isPaused ? 0f : 1f;

        if (pauseButtonImage != null)
        {
            if (_isPaused)
            {
                pauseButtonImage.sprite = resumeIcon;
            }
            else
            {
                pauseButtonImage.sprite = pauseIcon;
            }
        }
    }

    public void OnPauseButtonPressed()
    {
        TogglePause();
    }

    void EndLevel(bool allTaskFinish)
    {
        _levelEnded = true;
        Time.timeScale = 0f;

        //Debug.Log("Level Ended!");
        UI_HUD.SetActive(false);
        UI_Bubbles.SetActive(false);

        UI_result.SetActive(true);

        resultsPanelUI.ShowResults(nextLevelSceneName, mainMenuSceneName);
    }
}