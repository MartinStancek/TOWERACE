using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public Color playerColor;

    public CinemachineVirtualCamera vcam;
    public CarController car;

    public int startMoney = 100;
    public int moneyByRound = 100;
    public float scoreMultilier = 30;
    private int _money = 0;
    public int money
    {
        set
        {
            _money = value;
            moneyVisual.text = "" + value + "$";
        }
        get { return _money; }
    }
    public int stars = 0;

    public string controlScheme;

    public TMP_Text moneyVisual;


    public PlayerInput playerInput;


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
        SetReady(!GameController.Instance.lobbyReadyParent.GetChild(playerIndex).GetChild(0).gameObject.activeInHierarchy);
    }

    public void SetReady(bool value)
    {
        var playerPanel = GameController.Instance.lobbyReadyParent.GetChild(playerIndex);
        var readyPanel = playerPanel.GetChild(0);
        var notReadyPanel = playerPanel.GetChild(1);
        readyPanel.gameObject.SetActive(value);
        notReadyPanel.gameObject.SetActive(!value);

        switch (GameController.Instance.gameMode)
        {
            case GameMode.LOBBY:
                if (LobbyManager.Instance.ReadyPlayersCount() == GameController.Instance.players.Count)
                {
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

