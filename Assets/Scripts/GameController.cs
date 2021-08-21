using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;

[System.Serializable]
public class SnapUI
{
    public int index;
    public Vector2 position;
    public TowerSnap snap;

    public override string ToString()
    {
        return base.ToString() + "[index=" + index + ", position=" + position + "]";
    }
}
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

    public Transform towerPointerParent;

    public List<int> playersFinished;

    public List<Player> players;

    public Transform checkPonts;

    public Transform spawnPoints;

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

    public List<SnapUI> snapsUI;

    public Transform towerPlacingCountdown;
    public TMP_Text towerPlacingCountdownText;
    private Coroutine towerPlacingCountdownCor;
    public int towerPlacingSeconds = 20;

    public Transform playersOutlineParent;

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
            var position = players.Count - playersFinished.IndexOf(player.playerIndex);
            var targetPositionIndex = position - 1;
            player.outline.positionPlayer.text = "" + position;
            var point = spawnPoints.GetChild(targetPositionIndex);

            cc.RestartPostion(point.position, point.rotation);
            cc.isActivated = false;
            cc.SetCarSkin();
            //cc.SetChickenSkin();

            var cpc = cc.rb.transform.GetComponent<CheckPointController>();
            cpc.lastCheckPointIndex = -1;
            cpc.lastPassed = -1;
            player.playerInput.SwitchCurrentActionMap("Car");

            player.vcam.Follow = player.car.transform;

            var tt = player.GetComponent<TowerPlacer>();

            player.outline.readyPanel.gameObject.SetActive(false);
            player.outline.positionPanel.gameObject.SetActive(true);
            player.outline.gameObject.SetActive(true);


            //tt.placingState = TowerPlaceState.CHOOSING_SPOT;
            //towersSnapParent.transform.GetChild(tt.snapIndex).GetComponent<TowerSnap>().SetPanel(null, -1);
        }
        playersFinished.Clear();


        foreach(Transform t in towerPointerParent)
        {
            t.GetComponent<TowerPointerUI>().SetPanel(null);
        }

        StartCountdown();
        gameMode = GameMode.RACING;
        towerPlacingCountdown.gameObject.SetActive(false);
        if (towerPlacingCountdownCor != null) 
        {
            StopCoroutine(towerPlacingCountdownCor);
        }

        

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
            var cc = player.GetComponentInChildren<CarController>();
            var targetPositionIndex = players.Count - playersFinished.IndexOf(player.playerIndex) - 1;
            var point = spawnPoints.GetChild(targetPositionIndex);
            cc.RestartPostion(point.position, point.rotation);
            cc.isActivated = false;
            cc.SetCarSkin();


            var playerPosition = playersFinished.IndexOf(player.playerIndex);
            var extra_income = (int)((4 - playerPosition) * player.scoreMultilier);

            player.money += (4 * player.moneyByRound) / players.Count + extra_income;
            player.outline.positionPanel.gameObject.SetActive(false);
            player.outline.gameObject.SetActive(false);

        }

        onEndRace.Invoke();
    }

    public void EndRacingResult()
    {
        gameMode = GameMode.TOWER_PLACING;
        for (var i = 0; i < players.Count; i++)
        {

            var p = players[i];
            p.GetComponent<TowerPlacer>().ClaimRandomSpot();
            p.playerInput.SwitchCurrentActionMap("Spot");
            p.outline.readyPanel.gameObject.SetActive(true);
            p.outline.positionPanel.gameObject.SetActive(false);
            p.SetReady(false);


        }
        towerPlacingCountdown.gameObject.SetActive(true);
        towerPlacingCountdownText.text = "" + towerPlacingSeconds;
        towerPlacingCountdownCor = StartCoroutine(SetTowerPlacingCountDown(towerPlacingSeconds - 1, () =>
        {
            towerPlacingCountdownCor = null;
            StartRace();
        }));
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
    private IEnumerator SetTowerPlacingCountDown(int secondsRemain, Action finishAction)
    {
        yield return new WaitForSeconds(1);

        Debug.Log("TowerPlacingCountDown: " + secondsRemain);
        if (secondsRemain > 0)
        {
            towerPlacingCountdownText.text = "" + secondsRemain;
            yield return SetTowerPlacingCountDown(secondsRemain - 1, finishAction);
        }
        else
        {
            finishAction.Invoke();
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
        towerPlacingCountdown.gameObject.SetActive(false);

    }

    public void StartGame()
    {
        joinPanel.SetActive(false);

        transform.Find("InputManager").GetComponent<PlayerInputManager>().DisableJoining();
        onStartGame.Invoke();
        SetupUISnaps();

        //StartRace();
        EndRace();
    }

    private void SetCarCameras(bool value)
    {
        foreach (var cam in carCameras)
        {
            cam.gameObject.SetActive(value);
        }

    }

    public List<TowerSnap> GetFreeTowerSnapsInDirection(int actualSnapIndex, Vector2 direction, Player player)
    {
        var freeTowerSnaps = new List<SnapUI>();
        var actualSnap = towersSnapParent.transform.GetChild(actualSnapIndex);

        foreach (var snapUI in snapsUI)
        {
            if (actualSnapIndex == snapUI.index)
            {
                continue;
            }

            var p = new Vector2(actualSnap.position.x, actualSnap.position.z);
            var p0 = snapsUI[actualSnapIndex].position;
            var dist = 1000f;
            var y_offset = Mathf.Tan(0.25f * Mathf.PI) * dist;
            Vector2 p1, p2;
            if (direction == Vector2.right)
            {
                p1 = new Vector2(p0.x + dist, p0.y + y_offset);
                p2 = new Vector2(p0.x + dist, p0.y - y_offset);
            }
            else if (direction == Vector2.left)
            {
                p1 = new Vector2(p0.x - dist, p0.y + y_offset);
                p2 = new Vector2(p0.x - dist, p0.y - y_offset);
            }
            else if (direction == Vector2.up)
            {
                p1 = new Vector2(p0.x + y_offset, p0.y + dist);
                p2 = new Vector2(p0.x - y_offset, p0.y + dist);
            }
            else if (direction == Vector2.down)
            {
                p1 = new Vector2(p0.x + y_offset, p0.y - dist);
                p2 = new Vector2(p0.x - y_offset, p0.y - dist);
            }
            else
            {
                throw new Exception("Unknown direction vector: " + direction);
            }

            if (PointInTriangle(snapUI.position, p0, p1, p2))
            {
                var snap = snapUI.snap;
                if ((!snap.isOccupied && snap.playerOwner == null) ||
                    (snap.playerOwner != null && snap.playerOwner.Equals(player) && snap.tower == null))
                {
                    freeTowerSnaps.Add(snapUI);
                }
                //snapUI.go.GetComponent<Image>().color = Color.cyan;
            } 
            else
            {
                //snapUI.go.GetComponent<Image>().color = Color.white;
            }

        }

        return freeTowerSnaps
            .OrderBy(e => Vector2.Distance(e.position, snapsUI[actualSnapIndex].position))
            .Select(e => e.snap)
            .ToList();
    }
    public void SetupUISnaps()
    {
        snapsUI = new List<SnapUI>();
        var towerPointerUI = towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();
        for (var i = 0; i < towersSnapParent.transform.childCount; i++)
        {
            var target = towersSnapParent.transform.GetChild(i);
            var pointerRect = towerPointerUI.GetComponent<RectTransform>();

            //this is your object that you want to have the UI element hovering over
            GameObject WorldObject = target.gameObject;

            //first you need the RectTransform component of your canvas
            RectTransform CanvasRect = towerPointerParent.parent.GetComponent<RectTransform>();

            //then you calculate the position of the UI element
            //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

            Vector2 ViewportPosition = mapCamera.WorldToViewportPoint(WorldObject.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * pointerRect.pivot.x)),
            ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * pointerRect.pivot.y)));


            var newSnap = new SnapUI();
            newSnap.index = i;
            newSnap.position = WorldObject_ScreenPosition - towerPointerUI.offset;
            newSnap.snap = target.GetComponent<TowerSnap>();

            //Debug.Log(newSnap.ToString());
            snapsUI.Add(newSnap);
        }
    }
    public List<TowerSnap> GetAllFreeTowerSnapes(Player player)
    {
        var freeTowerSnaps = new List<TowerSnap>();
        foreach (Transform snapT in towersSnapParent.transform)
        {
            var snap = snapT.GetComponent<TowerSnap>();
            if ((!snap.isOccupied && snap.playerOwner == null) ||
                (snap.playerOwner != null && snap.playerOwner.Equals(player) && snap.tower == null))
            {
                freeTowerSnaps.Add(snap);
            }
        }
        return freeTowerSnaps;

    }

    public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
        var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

        if ((s < 0) != (t < 0))
            return false;

        var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

        return A < 0 ?
                (s <= 0 && s + t >= A) :
                (s >= 0 && s + t <= A);
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

    public void UpdateCheckPointPanel()
    {
        var orderedPlayers = players.OrderByDescending(e => e.checkPointController.lastCheckPointIndex).ToList();

        for (var i = 0; i < orderedPlayers.Count; i++)
        {
            var player = orderedPlayers[i];
            player.outline.positionPlayer.text = "" + (i + 1);
        }
    }

}
