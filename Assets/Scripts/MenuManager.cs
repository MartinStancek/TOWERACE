using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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

    public Slider soundSlider;
    public Slider musicSlider;

    [HideInInspector]
    public UnityEvent onGamePaused;
    [HideInInspector]
    public UnityEvent onGameResumed;
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
            Time.timeScale = 1f;

        }
        soundSlider.value = PlayerPrefs.GetFloat("sound", SoundManager.soundDefaultValue);
        musicSlider.value = PlayerPrefs.GetFloat("music", SoundManager.musicDefaultValue);

        soundSlider.onValueChanged.AddListener((value) => OnSliderChanged("sound", value));
        musicSlider.onValueChanged.AddListener((value) => OnSliderChanged("music", value));

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
        onGameResumed.Invoke();

    }

    public void PauseGame()
    {
        gameObject.SetActive(true);
        startButton.Select();
        Time.timeScale = 0f;
        onGamePaused.Invoke();
    }

    public void Menu()
    {
        SetPanel(loadingPanel);
        var operation = SceneManager.LoadSceneAsync("MainMenu");
        operation.completed += (a) => {
            Time.timeScale = 1f;
        };
    }

    public void OnSliderChanged(string name, float value)
    {
        PlayerPrefs.SetFloat(name, value);
        switch (name)
        {
            case "sound":
                SoundManager.SetSoundVolume(value);
                break;
            case "music":
                SoundManager.SetMusicVolume(value);
                break;
        }
    }
}
