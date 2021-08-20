using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    private int playerCount = 0;

    private List<Camera> playerCameras = new List<Camera>();

    public LayerMask[] cameraMasks;

    public List<Color> playerColors;

    public GameObject customKeyboardPlayerPrefab;
    private bool customPlayerJoined = false;
    private void Awake()
    {
        playerCount = 0;
        GameController.Instance.backGroundCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame && !customPlayerJoined)
        {
            //var pim = GetComponent<PlayerInputManager>();
            var go = Instantiate(customKeyboardPlayerPrefab);
            //OnPlayerJoined(go.GetComponent<PlayerInputCustom>());
            customPlayerJoined = true;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame || Gamepad.all.Where(e => e.startButton.wasPressedThisFrame).ToList().Count > 0)
        {
            if (MenuManager.Instance.isPaused)
            {
                MenuManager.Instance.ContinueGame();
            }
            else
            {
                MenuManager.Instance.PauseGame();

            }
        }
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        if (playerCount == 0)
        {
            input.camera.rect = new Rect(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
            GameController.Instance.backGroundCamera.rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 0.5f));

        }

        if (playerCount == 1)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
            input.camera.rect = new Rect(new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
            GameController.Instance.backGroundCamera.rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        }
        
        if (playerCount == 2)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
            playerCameras[1].rect = new Rect(new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
            input.camera.rect = new Rect(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            GameController.Instance.backGroundCamera.rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f));

        }
        /* TODO test*/
        if (playerCount == 3)
        {
            input.camera.rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f));
        }

        foreach(var d in input.devices)
        {
            Debug.Log("player " + playerCount + "controller: " + d.displayName);
        }

        playerCameras.Add(input.camera);
        GameController.Instance.carCameras.Add(input.camera);

        input.camera.gameObject.SetActive(true);

        input.transform.position = GameController.Instance.spawnPoints.GetChild(playerCount).position;

        input.camera.cullingMask = cameraMasks[playerCount];
        var p = input.gameObject.GetComponent<Player>();
        p.playerIndex = playerCount;
        p.playerColor = playerColors[playerCount];
        if (p.isCustomPlayer)
        {
            p.controlScheme = "Keyboard2";
        }
        else
        {
            p.controlScheme = input.currentControlScheme;
        }
        Debug.Log("PlayerJoined with scheme: \"" + p.controlScheme + "\"");

        foreach (var r in p.coloredParts)
        {
            r.SetColor(p.playerColor);
        }

        var panel = GameController.Instance.towerPointerParent.GetChild(playerCount);
        panel.gameObject.SetActive(true);
        var tpUI = panel.GetComponent<TowerPointerUI>();
        tpUI.SetColor(p.playerColor);
        p.towerPlacer = tpUI;
        p.money = p.startMoney;

        playerCount++;

        input.camera.transform.parent.Find("CMvcam").gameObject.layer = LayerMask.NameToLayer("Cam" + playerCount);
        input.camera.gameObject.layer = LayerMask.NameToLayer("Cam" + playerCount);

        GameController.Instance.players.Add(p);
        GameController.Instance.lobbyReadyParent.GetChild(p.playerIndex).gameObject.SetActive(true);
        var PosPanell = GameController.Instance.racePositionParent.GetChild(p.playerIndex);
        PosPanell.GetComponent<Image>().color = p.playerColor;
        foreach(Transform PosPanel in GameController.Instance.racePositionParent)
        {
            PosPanel.Find("PlayerCount").GetComponent<TMP_Text>().text = "" + GameController.Instance.players.Count;
        }

        GameController.Instance.onStartGame.AddListener(SetPlayerCameraFinal);
        GameController.Instance.playersFinished.Insert(0, p.playerIndex);

        GameController.Instance.UpdateCheckPointPanel();
    }


    private void SetPlayerCameraFinal()
    {
        if (playerCount == 1)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f));
            GameController.Instance.backGroundCamera.gameObject.SetActive(false);

        }

        if (playerCount == 2)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
            playerCameras[1].rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 0.5f));
            GameController.Instance.backGroundCamera.gameObject.SetActive(false);

            GameController.Instance.backGroundCamera.rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        }
        if (playerCount == 3)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
            playerCameras[1].rect = new Rect(new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
            playerCameras[2].rect = new Rect(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            GameController.Instance.backGroundCamera.gameObject.SetActive(true);
        }

        if (playerCount == 4)
        {
            playerCameras[0].rect = new Rect(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
            playerCameras[1].rect = new Rect(new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
            playerCameras[2].rect = new Rect(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            playerCameras[3].rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f));
            GameController.Instance.backGroundCamera.gameObject.SetActive(false);
        }

    }


}
