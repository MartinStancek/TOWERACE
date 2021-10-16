using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem;
using MLAPI.NetworkVariable;
using MLAPI;
using MLAPI.Messaging;
using System.Linq;

public class Player : NetworkBehaviour
{

    public List<MeshRendererMaterials> coloredParts;

    public int playerIndex;
    public Color playerColor;

    public CinemachineVirtualCamera vcam;
    public CarController car;
    public CheckPointController checkPointController;

    public int startMoney = 100;
    public int moneyByRound = 100;
    public float scoreMultilier = 30;
    private int _money = 0;
    public int money
    {
        set
        {
            _money = value;
            towerPlacer.SetMoney(value);
        }
        get { return _money; }
    }
    public int stars = 0;

    public string controlScheme;

    public TowerPointerUI towerPlacer;

    public bool isCustomPlayer = false;

    public PlayerInput playerInput;
    public PlayerOutline outline;
    public PlayerUIVisual playerUIVisual;

    public bool isReady = false;

    private NetworkVariable<bool> IsReadySynch = new NetworkVariable<bool>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, false);

    private PlayerInfo playerInfo;

    private void OnEnable()
    {
        IsReadySynch.OnValueChanged += ReadyChanged;
    }

    private void OnDisable()
    {
        IsReadySynch.OnValueChanged += ReadyChanged;
    }

    private void ReadyChanged(bool oldVal, bool newVal)
    {
        SetReady(newVal);
    }

    public override void NetworkStart()
    {
        if (IsOwner)
        {
            playerInfo = PlayerInfo.Local;
            GameController.Instance.onStartRace.AddListener(RaceStartInit);
            GameController.Instance.onEndRace.AddListener(EndRaceInit);
            GameController.Instance.onEndRace.AddListener(EndRacingResultInit);
        }
        else
        {
            Debug.Log("OtherPlayer");

            playerInput.enabled = false;
            car.enabled = false;
            car.rb.isKinematic = true;
            playerInfo = GameObject.FindObjectsOfType<PlayerInfo>().Where(e => e.OwnerClientId.Equals(OwnerClientId)).FirstOrDefault();
            car.carCamera.gameObject.SetActive(false);
            vcam.gameObject.SetActive(false);
        }
        playerInput = GetComponent<PlayerInput>();
        //playerInput.SwitchCurrentActionMap("Spot");

        var count = GameController.Instance.playerUIParent.childCount;
        var go = Instantiate(GameController.Instance.playerUIPrefab, GameController.Instance.playerUIParent);
        var offset = -5 - count * 40 - 5 * count;
        go.transform.localPosition = new Vector3(0f, offset, 0f);
        playerUIVisual = go.GetComponent<PlayerUIVisual>();
        playerUIVisual.playerName.text = playerInfo.Name.Value;
    }

    private void RaceStartInit()
    {
        /*TODO
        var cc = GetComponentInChildren<CarController>();

        var position = players.Count - playersFinished.IndexOf(player.playerIndex);
        var targetPositionIndex = position - 1;
        player.outline.positionPlayer.text = "" + position;
        var point = spawnPoints.GetChild(targetPositionIndex);


        cc.RestartPostion(point.position, point.rotation);*/
        car.isActivated = false;
        car.SetCarSkin();
        //cc.SetChickenSkin();
        outline.countDownPanel.gameObject.SetActive(true);
        if (playerInput && playerInput.currentActionMap != null)
        {
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap("Car");
            playerInput.currentActionMap.Enable();
        }

        vcam.Follow = car.transform;

        var tt = GetComponent<TowerPlacer>();

        outline.readyPanel.gameObject.SetActive(false);
        outline.positionPanel.gameObject.SetActive(true);
        outline.gameObject.SetActive(true);
        car.carCamera.gameObject.SetActive(true);

    }

    private void EndRaceInit()
    {
        /*var cc = player.GetComponentInChildren<CarController>();
        var targetPositionIndex = players.Count - playersFinished.IndexOf(player.playerIndex) - 1;
        var point = spawnPoints.GetChild(targetPositionIndex);
        cc.RestartPostion(point.position, point.rotation);*/
        car.isActivated = false;
        car.SetCarSkin();

        outline.countDownPanel.gameObject.SetActive(false);

        /*
        var playerPosition = playersFinished.IndexOf(player.playerIndex);
        var extra_income = (int)((4 - playerPosition) * player.scoreMultilier);

        player.money += (4 * player.moneyByRound) / players.Count + extra_income;*/
        outline.positionPanel.gameObject.SetActive(false);
        outline.gameObject.SetActive(false);
        car.carCamera.gameObject.SetActive(false);

    }

    private void EndRacingResultInit()
    {
        GetComponent<TowerPlacer>().ClaimRandomSpot();
        if (playerInput.currentActionMap != null)
        {
            playerInput.SwitchCurrentActionMap("Spot");
        }
        outline.readyPanel.gameObject.SetActive(true);
        outline.positionPanel.gameObject.SetActive(false);
        SetReady(false);

    }

    void Awake()
    {
        PlayerManager.Instance.OnPlayerJoined(playerInput);
    }
 
    public void OnPlayerReady(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Ready");
            ToggleReady();
        }
    }


    public void ToggleReady()
    {
        IsReadySynch.Value = !IsReadySynch.Value;
        SoundManager.PlaySound(SoundManager.SoundType.PLAYER_READY);
    }

    public void SetReady(bool value)
    {
        isReady = value;
        if (playerUIVisual != null)
        {
            playerUIVisual.SetReady(value);
        }
        switch (GameController.Instance.gameMode.Value)
        {
            case GameMode.LOBBY:
                if (LobbyManager.Instance.ReadyPlayersCount() == GameController.Instance.players.Count)
                {
                    Debug.Log("Checking player ready");
                    LobbyManager.Instance.LobbyPlayersReady();
                }
                else
                {
                    LobbyManager.Instance.ResetLobbyPlayersReady();
                }
                break;
            case GameMode.TOWER_PLACING:
                if (LobbyManager.Instance.ReadyPlayersCount() == GameController.Instance.players.Count)
                {
                    GameController.Instance.StartRace();
                }
                break;
        }
    }
}

