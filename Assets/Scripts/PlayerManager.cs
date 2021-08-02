using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    private int playerCount = 0;

    private List<Camera> playerCameras = new List<Camera>();

    public LayerMask[] cameraMasks;

    public List<Color> playerColors;

    private void Awake()
    {
        playerCount = 0;
        GameController.Instance.backGroundCamera.gameObject.SetActive(true);
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

        playerCameras.Add(input.camera);
        GameController.Instance.carCameras.Add(input.camera);

        input.camera.gameObject.SetActive(true);

        var body = input.transform.Find("Car/race/body");
        body.GetComponent<MeshRenderer>().materials[0].color = playerColors[playerCount];

        input.transform.position = GameController.Instance.spawnPoints.GetChild(playerCount).position;

        input.camera.cullingMask = cameraMasks[playerCount];
        var p = input.gameObject.GetComponent<Player>();
        p.playerIndex = playerCount;
        p.playerColor = playerColors[playerCount];

        var panel = GameController.Instance.moneyPanel.GetChild(playerCount);
        panel.gameObject.SetActive(true);
        panel.GetComponent<Image>().color = p.playerColor;
        p.moneyVisual = panel.GetComponentInChildren<TMP_Text>();
        p.money = p.startMoney;
        GameController.Instance.lobbyReadyParent.GetChild(playerCount).gameObject.SetActive(true);

        playerCount++;

        input.camera.transform.parent.Find("CMvcam").gameObject.layer = LayerMask.NameToLayer("Cam" + playerCount);
        input.camera.gameObject.layer = LayerMask.NameToLayer("Cam" + playerCount);

        GameController.Instance.players.Add(p);

        GameController.Instance.onStartGame.AddListener(SetPlayerCameraFinal);
        GameController.Instance.playersFinished.Insert(0, p.playerIndex);

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
