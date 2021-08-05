using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;

public enum GameMode
{
    LOBBY, TOWER_PLACING, RACING, RACING_RESULT
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

    public GameObject joinPanel;

    public GameObject towersSnapParent;
    public Transform lobbyReadyParent;

    public List<int> playersFinished;

    public List<Player> players;

    public Transform checkPonts;

    public Transform spawnPoints;

    public Transform moneyPanel;

    [HideInInspector]
    public UnityEvent onStartGame;

    [HideInInspector]
    public UnityEvent onStartRace;

    [HideInInspector]
    public UnityEvent onEndRace;

    [HideInInspector]
    public UnityEvent onRacingResultEnd;

    [HideInInspector]
    public GameMode gameMode = GameMode.LOBBY;

    public void StartRace()
    {
        SetCarCameras(true);
        mapCamera.gameObject.SetActive(false);
        if (players.Count == 3)
        {
            backGroundCamera.gameObject.SetActive(true);
        }

        countDownText.gameObject.SetActive(true);

        foreach (var player in players)
        {
            var cc = player.GetComponentInChildren<CarController>();
            var targetPositionIndex = players.Count - playersFinished.IndexOf(player.playerIndex) - 1;
            cc.RestartPostion(spawnPoints.GetChild(targetPositionIndex).position);
            cc.isActivated = false;

            cc.rb.transform.GetComponent<CheckPointController>().lastCheckPointIndex = -1;

            player.vcam.Follow = player.car.transform;

            var tt = player.GetComponent<TowerPlacer>();
            tt.placingState = TowerPlaceState.CHOOSING_SPOT;
            towersSnapParent.transform.GetChild(tt.snapIndex).GetComponent<TowerSnap>().SetPanel(null, -1);
        }
        playersFinished.Clear();

        foreach (Transform t in lobbyReadyParent)
        {
            t.gameObject.SetActive(false);
        }

        StartCountdown();
        gameMode = GameMode.RACING;

        onStartRace.Invoke();
    }
    public void EndRace()
    {
        SetCarCameras(false);
        mapCamera.gameObject.SetActive(true);
        backGroundCamera.gameObject.SetActive(false);
        countDownText.gameObject.SetActive(false);

        gameMode = GameMode.RACING_RESULT;

        FixFinishedPlayersCount();

        foreach (var player in players)
        {
            player.GetComponent<TowerPlacer>().ClaimRandomSpot();

            var cc = player.GetComponentInChildren<CarController>();
            var targetPositionIndex = players.Count - playersFinished.IndexOf(player.playerIndex) - 1;
            cc.RestartPostion(spawnPoints.GetChild(targetPositionIndex).position);
            cc.isActivated = false;
            player.SetReady(false);

            var playerPosition = playersFinished.IndexOf(player.playerIndex);
            var extra_income = (int)((4 - playerPosition) * player.scoreMultilier);

            player.money += player.moneyByRound + extra_income;
        }

        onEndRace.Invoke();
    }

    public void EndRacingResult()
    {
        gameMode = GameMode.TOWER_PLACING;
        for (var i = 0; i < players.Count; i++)
        {
            lobbyReadyParent.GetChild(i).gameObject.SetActive(true);
        }

        onRacingResultEnd.Invoke();
    }

    private void FixFinishedPlayersCount()
    {
        //add players to finished player to complete list for next round spawning cars in correct order
        while (playersFinished.Count < players.Count)
        {
            var nextPlayer = players.Where(e => !playersFinished.Contains(e.playerIndex))
                .OrderBy(e => e.car.rb.GetComponent<CheckPointController>().lastCheckPointIndex)
                .FirstOrDefault();
            playersFinished.Add(nextPlayer.playerIndex);
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
        playersFinished.Add(playerIndex);

        if (playersFinished.Count == 1)
        {
            countDownText.gameObject.SetActive(true);
            countDownText.text = "" + waitingTime;
            StartCoroutine(SetEndRaceCountDown(waitingTime - 1, EndRace));
        }

    }
    private IEnumerator SetEndRaceCountDown(int secondsRemain, Action finishAction)
    {
        if (playersFinished.Count == players.Count)
        {
            finishAction.Invoke();
        }
        else
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
    }
    private IEnumerator SetCountdownText(int secondsRemain)
    {
        yield return new WaitForSeconds(3 - secondsRemain);
        if (secondsRemain == 0)
        {
            Debug.Log("GO!!!");
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
        countDownText.gameObject.SetActive(false);
        joinPanel.SetActive(true);

    }

    public void StartGame()
    {
        joinPanel.SetActive(false);

        transform.Find("InputManager").GetComponent<PlayerInputManager>().DisableJoining();
        onStartGame.Invoke();
        StartRace();
    }

    private void SetCarCameras(bool value)
    {
        foreach (var cam in carCameras)
        {
            cam.gameObject.SetActive(value);
        }

    }

    public List<TowerSnap> GetFreeTowerSnaps(int fromIndex, int toIndex, Player player)
    {
        var freeTowerSnaps = new List<TowerSnap>();
        for (var i = fromIndex; i < toIndex; i++)
        {
            var snap = towersSnapParent.transform.GetChild(i).GetComponent<TowerSnap>();
            if ((!snap.isOccupied && snap.playerOwner == null) || (snap.playerOwner != null && snap.playerOwner.Equals(player)))
            {
                freeTowerSnaps.Add(snap);

            }
        }

        return freeTowerSnaps;
    }

    public int IndexOfSnap(TowerSnap snap)
    {
        var i = 0;
        foreach (Transform s in towersSnapParent.transform)
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
