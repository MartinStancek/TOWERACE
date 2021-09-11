using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.Transports.SteamP2P;
using MLAPI.Transports;
using MLAPI.Transports.UNET;
using TMPro;

public class MenuManager : MonoBehaviour
{
    private const int STEAM = 0;
    private const int LAN = 1;

    public static float botDificultyDefaultValue = 0.8f;
    public static float networkDefaultValue = STEAM;
    public static string networkLANconnectIPValue = "127.0.0.1";

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
    public Transform optionsPanel;
    public Transform towersPanel;
    public Transform steamPanel;
    public Transform lanPanel;

    public Slider soundSlider;
    public Slider musicSlider;
    public Slider botDificultySlider;
    public TMP_Dropdown networkDropdown;
    public TMP_InputField lanConnectIP;

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
        botDificultySlider.value = PlayerPrefs.GetFloat("bot_dificulty", MenuManager.botDificultyDefaultValue);


        soundSlider.onValueChanged.AddListener((value) => SaveValue("sound", value));
        musicSlider.onValueChanged.AddListener((value) => SaveValue("music", value));
        botDificultySlider.onValueChanged.AddListener((value) => SaveValue("bot_dificulty", value));
    }
    void Start()
    {
        networkDropdown.onValueChanged.AddListener((value) => SaveValue("network", value));

        var networkValue = (int)PlayerPrefs.GetFloat("network", MenuManager.networkDefaultValue);
        if (networkValue == STEAM && !SteamManager.Initialized)
        {
            networkValue = LAN;
        }
        networkDropdown.SetValueWithoutNotify(networkValue);
        HandleNetwork(networkValue);
        lanConnectIP.text = PlayerPrefs.GetString("connect_to", networkLANconnectIPValue);
        lanConnectIP.onValueChanged.AddListener(value => PlayerPrefs.SetString("connect_to", value));
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

    public void SetPanel(Transform panel)
    {
        mainMenuPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(false);
        controlsPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);
        towersPanel.gameObject.SetActive(false);
        steamPanel.gameObject.SetActive(false);
        lanPanel.gameObject.SetActive(false);
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

    public void SaveValue(string name, float value)
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
            case "bot_dificulty":
                break;
            case "network":
                if (value == STEAM && !SteamManager.Initialized)
                {
                    networkDropdown.SetValueWithoutNotify(LAN);
                    HandleNetwork(LAN);
                } 
                else
                {
                    HandleNetwork((int)value);
                }
                break;
        }
    }
    private void HandleNetwork(int value)
    {

        NetworkTransport transport = NetworkManager.Singleton.gameObject.GetComponent<NetworkTransport>();
        if (transport != null)
        {
            Destroy(transport);
            transport = null;
        }
        switch (value)
        {
            case STEAM:
                transport = NetworkManager.Singleton.gameObject.AddComponent<SteamP2PTransport>();
                break;
            case LAN:
                transport = NetworkManager.Singleton.gameObject.AddComponent<UNetTransport>();
                break;
            default:
                throw new Exception("Unknown network value: " + value);
        }
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;

    }

    public void OnlineButtonClicked()
    {
        var mode = networkDropdown.value;
        switch (mode)
        {
            case STEAM:
                SetPanel(steamPanel);
                break;
            case LAN:
                SetPanel(lanPanel);
                break;
        }
    }
}
