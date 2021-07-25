using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Singleton
    private static GameController _instance;
    public static GameController Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<GameController>();
            }
            return _instance;

        }
    }
    #endregion

    public Camera[] carCameras;
    public Camera mapCamera;

    public void StartGame()
    {
        SetCarCameras(true);
        mapCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        SetCarCameras(false);
        mapCamera.gameObject.SetActive(true);

    }

    private void SetCarCameras(bool value)
    {
        for (var i = 0; i < carCameras.Length; i++)
        {
            carCameras[i].gameObject.SetActive(value);
        }
    }
}
