using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Netcode;
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
public class GameController : NetworkBehaviour
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

    public Camera mapCamera;

    public TMP_Text countDownText;


    public GameObject towersSnapParent;

    public Transform towerPointerParent;

    public List<int> playersFinished = new List<int>();
    public List<int> playersFinishedOld;

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
    public UnityEvent onEndRaceLate;

    [HideInInspector]
    public UnityEvent onRacingResultEnd;

    public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>();

    public List<SnapUI> snapsUI;

    public GameObject towerPlacingCountdownBar;
    public Transform towerPlacingCountdown;
    public TMP_Text towerPlacingCountdownText;
    private Coroutine towerPlacingCountdownCor;
    public int towerPlacingSeconds = 20;

    public Transform playersOutlineParent;

    public List<ParticleSystem> finishLineConfety;

    public GameObject playerPrefab;

    public Transform playerUIParent;
    public GameObject playerUIPrefab;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gameMode.Value = GameMode.LOBBY;
        }
    }


    void Awake()
    {
        if (NetworkManager.Singleton == null || !(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            SceneManager.LoadScene("MainMenu");
            _instance = null;
        }

        gameMode.OnValueChanged += (oldVal, newVal) =>
        {
            if (oldVal == GameMode.LOBBY && newVal == GameMode.RACING)
            {
                Debug.Log("OnStartGameEvent + onStartRaceEvent");
                onStartGame.Invoke();
                onStartRace.Invoke();
            }
            else if (oldVal == GameMode.RACING && newVal == GameMode.RACING_RESULT)
            {
                Debug.Log("onEndRaceEvent");
                onEndRace.Invoke();
                onEndRaceLate.Invoke();
            }
            else if (oldVal == GameMode.RACING_RESULT && newVal == GameMode.TOWER_PLACING)
            {
                Debug.Log("onRacingResultEndEvent");
                onRacingResultEnd.Invoke();
            }
            else if (oldVal == GameMode.TOWER_PLACING && newVal == GameMode.RACING)
            {
                Debug.Log("onStartRaceEvent");
                onStartRace.Invoke();
            }
        };
    }

    void Start()
    {
        mapCamera.gameObject.SetActive(false);
        countDownText.gameObject.SetActive(false);
        towerPlacingCountdown.gameObject.SetActive(false);
        onStartGame.AddListener(SetupUISnaps);

        onStartRace.AddListener(() =>
        {
            playersFinished.Clear();
            mapCamera.gameObject.SetActive(false);
            foreach (Transform t in towerPointerParent)
            {
                t.GetComponent<TowerPointerUI>().SetPanel(null);
            }

            SoundManager.PlaySound(SoundManager.SoundType.RACE_COUNTDOWN);
            StartCountdown();
            towerPlacingCountdown.gameObject.SetActive(false);
            if (towerPlacingCountdownCor != null)
            {
                StopCoroutine(towerPlacingCountdownCor);
            }
        });

        onEndRace.AddListener(() =>
        {
            mapCamera.gameObject.SetActive(true);
            countDownText.gameObject.SetActive(false);
            FixFinishedPlayersCount();
            playersFinishedOld = new List<int>(playersFinished);
        });

        onRacingResultEnd.AddListener(() =>
        {
            towerPlacingCountdownText.gameObject.SetActive(true);
            towerPlacingCountdown.gameObject.SetActive(true);
            towerPlacingCountdownText.text = "" + towerPlacingSeconds;
            towerPlacingCountdownCor = StartCoroutine(SetTowerPlacingCountDown(towerPlacingSeconds - 1, () =>
            {
                towerPlacingCountdownCor = null;
                StartRace();
            }));
        });
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Start Game Server");
            gameMode.Value = GameMode.RACING;
            //Invoke("TowerPlaceOnStart", 1f);
        }
    }
    private void TowerPlaceOnStart()
    {
        gameMode.Value = GameMode.RACING_RESULT;
    }

    public void StartRace()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Start Race Server");

            gameMode.Value = GameMode.RACING;
        }

    }
    public void EndRace()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("End Race Server");

            gameMode.Value = GameMode.RACING_RESULT;
        }
    }

    public void EndRacingResult()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("End Racing Result Server");

            gameMode.Value = GameMode.TOWER_PLACING;
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
        StartCoroutine(SetCountdownText(1.8f));
        StartCoroutine(SetCountdownText(1.2f));
        StartCoroutine(SetCountdownText(0.6f));
        StartCoroutine(SetCountdownText(0));
        StartCoroutine(RemoveCountDownText());
    }

    public void CarFinished(int playerIndex)
    {
        playersFinished.Add(playerIndex);
        SoundManager.PlaySound(SoundManager.SoundType.PLAYER_FINISHED);
        foreach (var ps in finishLineConfety)
        {
            ps.Play();
        }
        if (playersFinished.Count == 1)
        {
            countDownText.gameObject.SetActive(true);
            countDownText.text = "" + waitingTime;
            StartCoroutine(SetEndRaceCountDown(waitingTime - 1, EndRace));
        }

    }
    private IEnumerator SetEndRaceCountDown(int secondsRemain, Action finishAction)
    {
        var lastFinishDelay = 1.5f;
        if (playersFinished.Count == players.Count)
        {
            StartCoroutine(InvokeLate(lastFinishDelay, finishAction));
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
                StartCoroutine(InvokeLate(lastFinishDelay, finishAction));
            }
        }
    }
    private IEnumerator InvokeLate(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();

    }

    private IEnumerator SetTowerPlacingCountDown(int secondsRemain, Action finishAction)
    {
        yield return new WaitForSeconds(1);

        Debug.Log("TowerPlacingCountDown: " + secondsRemain);
        if (secondsRemain > 0)
        {
            towerPlacingCountdownText.text = "" + secondsRemain;
            if (secondsRemain < 5)
            {
                LeanTween.scale(towerPlacingCountdownBar.gameObject, Vector3.one * 1.4f, 0.1f)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(towerPlacingCountdownBar.gameObject, Vector3.one, 0.1f);
                    }
                    );
            }
            yield return SetTowerPlacingCountDown(secondsRemain - 1, finishAction);
        }
        else
        {
            finishAction.Invoke();
        }
    }
    private IEnumerator SetCountdownText(float secondsRemain)
    {
        yield return new WaitForSeconds(1.8f - secondsRemain);
        if (secondsRemain == 0)
        {
            Debug.Log("GO!!!");

            foreach (var player in players)
            {
                var cc = player.GetComponentInChildren<CarController>();
                cc.isActivated = true;
                player.outline.countDownText.text = "GO!";
                LeanTween.scale(player.outline.countDownText.gameObject, Vector3.one * 1.4f, 0.1f)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(player.outline.countDownText.gameObject, Vector3.one, 0.1f);
                    }
                    );
            }

        }
        else
        {
            foreach (var player in players)
            {
                player.outline.countDownText.text = "" + Mathf.RoundToInt(secondsRemain * (1 / 0.6f));
                player.outline.countDownText.transform.localScale = Vector3.one;
                LeanTween.scale(player.outline.countDownText.gameObject, Vector3.one * 1.4f, 0.1f)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(player.outline.countDownText.gameObject, Vector3.one, 0.1f);
                    }
                    );
            }

            Debug.Log(secondsRemain);
        }
    }
    private IEnumerator RemoveCountDownText()
    {
        yield return new WaitForSeconds(2.4f);
        foreach (var p in players)
        {
            p.outline.countDownPanel.gameObject.SetActive(false);
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
                if ((!snap.isOccupied.Value && snap.playerOwner == null) ||
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
            if ((!snap.isOccupied.Value && snap.playerOwner == null) ||
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
        var orderedPlayers = players
            .OrderByDescending(e => e.checkPointController.lastCheckPointIndex)
            .ThenBy(e => e.checkPointController.lastCheckPointTime)
            .ToList();

        for (var i = 0; i < orderedPlayers.Count; i++)
        {
            var player = orderedPlayers[i];
            player.outline.positionPlayer.text = "" + (i + 1);
        }
    }

}
