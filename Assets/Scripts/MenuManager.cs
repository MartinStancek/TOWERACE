using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    #region Singleton
    private static MenuManager _instance;
    public static MenuManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<MenuManager>();
            }
            return _instance;

        }
    }
    #endregion
    public Button startButton;
    public Button backButton;

    public Transform mainMenuPanel;
    public Transform loadingPanel;
    public Transform controlsPanel;
    public Transform soundPanel;

    public bool isPaused
    {
        get { return Time.timeScale == 0; }
    }

    void Awake()
    {
        _instance = this;
        try
        {
            startButton.Select();
        } catch (Exception e)
        {
            //jeble unity 
        }

        if (GameController.Instance)
        {
            gameObject.SetActive(false);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SetPanel(loadingPanel);
        SceneManager.LoadSceneAsync("MartinScene3");
        Time.timeScale = 1f;

    }

    private void SetPanel(Transform panel)
    {
        mainMenuPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(false);
        if (panel)
        {
            panel.gameObject.SetActive(true);
        }
    }

    public void ContinueGame()
    {
        gameObject.SetActive(false);
        backButton.Select();
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        gameObject.SetActive(true);
        startButton.Select();
        Time.timeScale = 0f;
    }
}
