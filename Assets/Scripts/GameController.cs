using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public enum GameMode
{
    LOBBY, PLAYER_JOINING, TOWER_PLACING, RACING, TOWER_CHOOSING
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
    public Button startRaceButton;

    public TMP_Text[] resultTexts;

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
    public GameMode gameMode = GameMode.LOBBY;

    private Coroutine readyCor = null;

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
        foreach (var t in resultTexts)
        {
            t.text = "";
        }

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
            towersSnapParent.transform.GetChild(tt.snapIndex).GetComponent<TowerSnap>().SetPanel(null);
        }
        playersFinished.Clear();

        foreach(Transform t in lobbyReadyParent)
        {
            t.gameObject.SetActive(false);
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
        var i = 0;

        for (i = 0; i < playersFinished.Count; i++)
        {
            var p = players.Where(e => e.playerIndex == playersFinished[i]).FirstOrDefault();
            p.money += (int)((4 - i) * p.scoreMultilier);
        }

        FixFinishedPlayersCount();

        i = 0;
        foreach (var player in players)
        {
            player.GetComponent<TowerPlacer>().ClaimRandomSpot();

            var cc = player.GetComponentInChildren<CarController>();
            var targetPositionIndex = players.Count - playersFinished.IndexOf(player.playerIndex) - 1;
            cc.RestartPostion(spawnPoints.GetChild(targetPositionIndex).position);
            cc.isActivated = false;

            player.money += player.moneyByRound;
        }
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
        if (playersFinished.Count < 4) // len 3 miesta na "podiu" su
        {
            resultTexts[playersFinished.Count - 1].text = "Player " + playerIndex;
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
        if (secondsRemain == 1)
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
        countDownText.gameObject.SetActive(false);
        resultsPanel.SetActive(false);
        joinPanel.SetActive(true);
        startRaceButton.gameObject.SetActive(false);

    }

    public void LobbyPlayersReady()
    {
        countDownText.gameObject.SetActive(true);
        countDownText.text = "" + 3;

        readyCor = StartCoroutine(SetEndLobbyCountDonw(2, StartGame, ()=>
        {
            countDownText.gameObject.SetActive(false);

        }));
    }
    public void ResetLobbyPlayersReady()
    {
        if(readyCor != null)
        {
            StopCoroutine(readyCor);
        }
        countDownText.gameObject.SetActive(false);
    }

    public int ReadyPlayersCount()
    {
        int count = 0;
        foreach(Transform t in lobbyReadyParent)
        {
            if (t.GetChild(0).gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }
    private IEnumerator SetEndLobbyCountDonw(int secondsRemain, Action finishAction, Action reverseAction)
    {

        yield return new WaitForSeconds(1);
        if (ReadyPlayersCount() < players.Count)
        {
            reverseAction.Invoke();
        }
        else
        {
            Debug.Log("EndLobbyCountDonw: " + secondsRemain);
            if (secondsRemain > 0)
            {
                countDownText.text = "" + secondsRemain;
                yield return SetEndLobbyCountDonw(secondsRemain - 1, finishAction, reverseAction);
            }
            else
            {
                finishAction.Invoke();
            }
        }
    }

    public void StartGame()
    {
        joinPanel.SetActive(false);

        StartRace();
        onStartGame.Invoke();
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
        for(var i = fromIndex; i< toIndex; i++)
        {
            var snap = towersSnapParent.transform.GetChild(i).GetComponent<TowerSnap>();
            if (!snap.isOccupied && (snap.playerOwner == null || snap.playerOwner.Equals(player)))
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
