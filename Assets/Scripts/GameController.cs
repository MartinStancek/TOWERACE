using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;


public enum GameMode
{
    PLAYER_JOINING, TOWER_PLACING, RACING, TOWER_CHOOSING
}
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

    [Tooltip("Vyjadrenie, kolko sa bude cakat po prvom aute, ktore prislo do ciela v sekundach")]
    public int waitingTime = 10;

    [HideInInspector]
    public List<Camera> carCameras = new List<Camera>();
    public Camera mapCamera;
    public Camera backGroundCamera;

    public TMP_Text countDownText;

    public GameObject resultsPanel;
    public GameObject joinPanel;
    public Button startGameButton;
    public Button startRaceButton;

    public TMP_Text[] resultTexts;

    public GameObject towersSnapParent;


    private List<int> playersFinished;

    public List<TowerPlacer> players;

    public Transform checkPonts;

    public Transform spawnPoints;

    [HideInInspector]
    public GameMode gameMode = GameMode.TOWER_PLACING;

    public void StartRace()
    {
        SetCarCameras(true);
        mapCamera.gameObject.SetActive(false);
        if (players.Count == 3)
        {
            backGroundCamera.gameObject.SetActive(true);
        }
        
        countDownText.gameObject.SetActive(true);
        resultsPanel.SetActive(false);
        foreach(var t in resultTexts)
        {
            t.text = "";
        }
        playersFinished.Clear();

        foreach(var player in players)
        {
            var cc = player.GetComponentInChildren<CarController>();
            cc.RestartPostion();
            cc.isActivated = false;
        }
        
        StartCountdown();
        gameMode = GameMode.RACING;
    }
    public void EndRace()
    {
        SetCarCameras(false);
        mapCamera.gameObject.SetActive(true);
        backGroundCamera.gameObject.SetActive(false);

        countDownText.gameObject.SetActive(false);
        resultsPanel.SetActive(true);

        gameMode = GameMode.TOWER_PLACING;

        foreach (var player in players)
        {
            player.ClaimRandomSpot();

            var cc = player.GetComponentInChildren<CarController>();
            cc.RestartPostion();
            cc.isActivated = false;
        }
    }

    private void StartCountdown()
    {
        StartCoroutine(SetCountdownText(3));
        StartCoroutine(SetCountdownText(2));
        StartCoroutine(SetCountdownText(1));
        StartCoroutine(SetCountdownText(0));
        StartCoroutine(RemoveCountDownText());
    }

    public void CarFinished(int playerIndex)
    {
        if (playersFinished.Count == 0)
        {
            countDownText.gameObject.SetActive(true);
            countDownText.text = "" + waitingTime;
            StartCoroutine(SetEndRaceCountDown(waitingTime - 1, EndRace));
        }
        if (playersFinished.Count < 3)
        {
            resultTexts[playersFinished.Count].text = "Player " + playerIndex;
        }
        playersFinished.Add(playerIndex);

    }
    private IEnumerator SetEndRaceCountDown(int secondsRemain, Action finishAction)
    {
        yield return new WaitForSeconds(1);
        Debug.Log("EndRaceCountDown: " + secondsRemain);
        if (secondsRemain > 0)
        {
            countDownText.text = "" + secondsRemain;
            yield return SetEndRaceCountDown(secondsRemain - 1, finishAction);
        } 
        else
        {
            finishAction.Invoke();
        }
    }
    private IEnumerator SetCountdownText(int secondsRemain)
    {
        yield return new WaitForSeconds(3-secondsRemain);
        if(secondsRemain == 0)
        {
            Debug.Log("GO!!!" );
            countDownText.text = "GO!";

            foreach (var player in players)
            {
                var cc = player.GetComponentInChildren<CarController>();
                cc.isActivated = true;
            }

        }
        else
        {
            countDownText.text = "" + secondsRemain;
            Debug.Log(secondsRemain);
        }
        if(secondsRemain == 1)
        {

            /*var vehs = MSSceneControllerFree.Instance.vehicles;
            for (var i = 0; i < vehs.Length; i++)
            {
                vehs[i].GetComponent<MSVehicleControllerFree>().EnterInVehicle();
            }*/
        }
    }
    private IEnumerator RemoveCountDownText()
    {
        yield return new WaitForSeconds(4);
        countDownText.gameObject.SetActive(false);
    }


    void Start()
    {
        playersFinished = new List<int>();
        SetCarCameras(false);
        mapCamera.gameObject.SetActive(false);
        backGroundCamera.gameObject.SetActive(false);
        countDownText.gameObject.SetActive(false);
        resultsPanel.SetActive(false);
        joinPanel.SetActive(true);
        startGameButton.gameObject.SetActive(true);
        startRaceButton.gameObject.SetActive(false);

    }

    public void StartGame()
    {
        joinPanel.SetActive(false);
        gameMode = GameMode.TOWER_PLACING;

        foreach (var p in players)
        {
            p.ClaimRandomSpot();

            var cc = p.GetComponentInChildren<CarController>();
            cc.isActivated = false;
            cc.RestartPostion();
        }
        startRaceButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(false);

        SetCarCameras(false);
        mapCamera.gameObject.SetActive(true);



    }

    private void SetCarCameras(bool value)
    {
        foreach (var cam in carCameras)
        {
            cam.gameObject.SetActive(value);
        }

    }

    public List<TowerSnap> GetFreeTowerSnaps(int fromIndex, int toIndex)
    {
        var freeTowerSnaps = new List<TowerSnap>();
        for(var i = fromIndex; i< toIndex; i++)
        {
            var snap = towersSnapParent.transform.GetChild(i).GetComponent<TowerSnap>();
            if (!snap.isOccupied)
            {
                freeTowerSnaps.Add(snap);

            }
        }

        return freeTowerSnaps;
    }

    public int IndexOfSnap(TowerSnap snap)
    {
        var i = 0;
        foreach(Transform s in towersSnapParent.transform)
        {
            if (s.Equals(snap.transform))
            {
                return i;
            }
            i++;
        }
        throw new Exception("Unable to find snap in towerSnapParent");
    }

}
