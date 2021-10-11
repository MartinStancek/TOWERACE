using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.Connection;
using MLAPI.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LANLobby : MonoBehaviour
{
    public GameObject playerLANLobbyPrefab;
    public RectTransform lanLobbyPlayerParent;

    public GameObject introPanel;
    public GameObject lobbyPanel;

    public void StartHost()
    {
        if (!CanConnect(true)) { return; }

        NetworkManager.Singleton.StartHost();
        SetPanel(lobbyPanel);
    }

    private void SetPanel(GameObject panel)
    {
        introPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        if (panel)
        {
            panel.SetActive(true);
        }
    }

    public void LobbyReady()
    {
        Debug.Log("Lobby Ready");
        var progress = NetworkSceneManager.SwitchScene("MartinScene3");
        progress.OnComplete += (timeOut) =>
        {
            Debug.Log("On complete");
            foreach(var c in NetworkManager.Singleton.ConnectedClientsList)
            {
                Debug.Log("foreach loop");

                c.PlayerObject.GetComponent<PlayerInfo>().SpawnCarClientRpc();
            }
            
        };
    }

    public void ConnectToHost()
    {
        if (!CanConnect()) { return; }

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = MenuManager.Instance.lanConnectIP.text;
        NetworkManager.Singleton.StartClient();
        SetPanel(lobbyPanel);

    }

    public void LeaveServer()
    {
        NetworkManager.Singleton.StopClient();
        SetPanel(introPanel);
    }

    private bool CanConnect(bool isHost = false)
    {
        if (MenuManager.Instance.lanName.text == "")
        {
            LeanTween.scale(MenuManager.Instance.lanName.gameObject, Vector3.one * 1.3f, 0.1f).setOnComplete(() => { LeanTween.scale(MenuManager.Instance.lanName.gameObject, Vector3.one, 0.1f); });
            return false;
        }
        if (!isHost && MenuManager.Instance.lanConnectIP.text == "")
        {
            LeanTween.scale(MenuManager.Instance.lanConnectIP.gameObject, Vector3.one * 1.3f, 0.1f).setOnComplete(() => { LeanTween.scale(MenuManager.Instance.lanConnectIP.gameObject, Vector3.one, 0.1f); });
            return false;
        }
        return true;
    }

    public void UpdatePlayers()
    {
        var count = NetworkManager.Singleton.ConnectedClientsList.Count;
        for (var j = lanLobbyPlayerParent.childCount - 1; j >= 0; j--)
        {
            Destroy(lanLobbyPlayerParent.GetChild(j).gameObject);
        }
        lanLobbyPlayerParent.sizeDelta = new Vector2(lanLobbyPlayerParent.sizeDelta.x, 30 * count + 1);

        if (NetworkManager.Singleton.IsServer)
        {
            for (var i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                var playerObj = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject;
                var name = playerObj.GetComponent<PlayerInfo>().Name.Value;
                var panel = Instantiate(playerLANLobbyPrefab, lanLobbyPlayerParent);


                panel.GetComponentInChildren<TMP_Text>().text = name;
                panel.GetComponentInChildren<Button>().interactable = false; /*true; // nefunguje :(
                panel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                panel.GetComponentInChildren<Button>().onClick.AddListener(()=>
                {
                    NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                    UpdatePlayers();
                });*/

                panel.transform.localPosition = new Vector3(0f, -30f * i, 0f);
            }
        } 
        else
        {
            var players = FindObjectsOfType<PlayerInfo>();
            for (var i = 0; i < players.Length; i++)
            {
                var playerObj = players[i];
                var name = playerObj.GetComponent<PlayerInfo>().Name.Value;
                var panel = Instantiate(playerLANLobbyPrefab, lanLobbyPlayerParent);


                panel.GetComponentInChildren<TMP_Text>().text = name;
                panel.GetComponentInChildren<Button>().interactable = false;

                panel.transform.localPosition = new Vector3(0f, -30f * i, 0f);
            }
        }

    }
}
