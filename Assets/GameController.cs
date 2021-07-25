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

        var vehs = MSSceneControllerFree.Instance.vehicles;
        for (var i = 0; i< vehs.Length; i++)
        {
            vehs[i].GetComponent<MSVehicleControllerFree>().raceStarted = false;
            vehs[i].GetComponent<MSVehicleControllerFree>().isInsideTheCar = false;
        }
        StartCountdown();

    }
    private void StartCountdown()
    {
        StartCoroutine(SetCountdownText(3));
        StartCoroutine(SetCountdownText(2));
        StartCoroutine(SetCountdownText(1));
        StartCoroutine(SetCountdownText(0));
    }
    private IEnumerator SetCountdownText(int secondsRemain)
    {
        yield return new WaitForSeconds(3-secondsRemain);
        if(secondsRemain == 0)
        {
            Debug.Log("GO!!!" );
            var vehs = MSSceneControllerFree.Instance.vehicles;
            for (var i = 0; i < vehs.Length; i++)
            {
                vehs[i].GetComponent<MSVehicleControllerFree>().raceStarted = true;
            }
        }
        else
        {
            Debug.Log(secondsRemain);
        }
        if(secondsRemain == 1)
        {
            var vehs = MSSceneControllerFree.Instance.vehicles;
            for (var i = 0; i < vehs.Length; i++)
            {
                vehs[i].GetComponent<MSVehicleControllerFree>().EnterInVehicle();
            }
        }
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
