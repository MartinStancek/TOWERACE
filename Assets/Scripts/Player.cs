using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
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
    public bool isReady = false;


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        //playerInput.SwitchCurrentActionMap("Spot");
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
        SetReady(!isReady);
        SoundManager.PlaySound(SoundManager.SoundType.PLAYER_READY);
    }

    public void SetReady(bool value)
    {
        isReady = value;
        outline.SetReady(value); 

        
        


        switch (GameController.Instance.gameMode)
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

