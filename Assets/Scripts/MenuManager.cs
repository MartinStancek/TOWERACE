using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Button startButton;

    public Transform mainMenuPanel;
    public Transform loadingPanel;
    public Transform controlsPanel;
    public Transform soundPanel;

    void Start()
    {
        try
        {
            startButton.Select();
        } catch (Exception e)
        {
            //jeble unity 
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

}
