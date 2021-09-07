using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using MLAPI;
using MLAPI.Transports.SteamP2P;
using System;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public NetworkManager networkManager;

    public GameObject introPanel;
    public GameObject lobbyPanel;
    public GameObject findLobbyPanel;

    public TMP_InputField lobbyNameField;


    public GameObject steamPlayerPrefabUI;
    public RectTransform steamLobbyPlayerParent;

    public TMP_Text steamLobbiesLoadingText;
    public RectTransform steamLobbiesParent;
    public GameObject steamLobbyPrefabUI;

    private const string HostAddressKey = "HostAddress";
    private const string GameStartedKey = "GameStarted";
    private const string LobbyNameKey = "LobbyName";

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdated;
    protected Callback<LobbyMatchList_t> lobbyMatchListUpdated;
    
    private CSteamID currentLobbyId;

    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized) { return; }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
        lobbyChatUpdated = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdated);
        lobbyMatchListUpdated = Callback<LobbyMatchList_t>.Create(onLobbyMatchListUpdated);
    }

    public void CreateLobby()
    {
        if (String.IsNullOrEmpty(lobbyNameField.text))
        {
            LeanTween.scale(lobbyNameField.gameObject, Vector3.one * 1.3f, 0.1f).setOnComplete(() => { LeanTween.scale(lobbyNameField.gameObject, Vector3.one, 0.1f); });
            return;
        }
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 10);
        Debug.Log("CreateLobby was called");
        SetPanel(null);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            SetPanel(introPanel);
            return;
        }
        SetPanel(lobbyPanel);

        Debug.Log("OnLobbyCreated was called");

        SteamMatchmaking.SetLobbyData(
            currentLobbyId,
            HostAddressKey,
            SteamUser.GetSteamID().ToString());

    }

    public void StartGame()
    {
        if (!SteamManager.Initialized) { return; }
        if (!SteamMatchmaking.GetLobbyOwner(currentLobbyId).Equals(SteamUser.GetSteamID())) { return; }

        Debug.Log("StartGame was called");
        SetPanel(null);

        networkManager.StartHost();
        SteamMatchmaking.SetLobbyData(currentLobbyId, GameStartedKey, "true");

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        if (SteamMatchmaking.GetLobbyOwner(currentLobbyId).Equals(SteamUser.GetSteamID()))
        {
            Debug.Log("Setting Lobby name to: " + lobbyNameField.text);
            SteamMatchmaking.SetLobbyData(currentLobbyId, LobbyNameKey, lobbyNameField.text);
        }

        Debug.Log("OnLobbyEntered was called");
        SetPanel(lobbyPanel);
        UpdatePlayers();
    }

    private void OnLobbyDataUpdated(LobbyDataUpdate_t callback)
    {
        if (!callback.m_ulSteamIDLobby.Equals(currentLobbyId)) { return; }
        Debug.Log("OnLobbyDataUpdated was called");

        var gameStarted = SteamMatchmaking.GetLobbyData(
            currentLobbyId,
            GameStartedKey);

        if (gameStarted != null && gameStarted.Equals("true"))
        {
            Debug.Log("joining to game!");

            var hostAddress = SteamMatchmaking.GetLobbyData(
                currentLobbyId,
                HostAddressKey);
            networkManager.GetComponent<SteamP2PTransport>().ConnectToSteamID = Convert.ToUInt64(hostAddress); // or callback.m_ulSteamIDLobby
            networkManager.StartClient();
            SetPanel(null);

        }
    }

    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(currentLobbyId);
        currentLobbyId = CSteamID.Nil;
    }


    private void SetPanel(GameObject panel)
    {
        introPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        findLobbyPanel.SetActive(false);
        if (panel)
        {
            panel.SetActive(true);
        }

    }

    public void FindLobbies()
    {
        SteamMatchmaking.RequestLobbyList();
        steamLobbiesLoadingText.gameObject.SetActive(true);
        SetPanel(findLobbyPanel);

        for(var i = 1; i< steamLobbiesParent.childCount; i++)
        {
            Destroy(steamLobbiesParent.GetChild(i).gameObject);
        }

    }

    private void onLobbyMatchListUpdated(LobbyMatchList_t callback)
    {
        steamLobbiesLoadingText.gameObject.SetActive(false);
        var count = callback.m_nLobbiesMatching;

        var index = 0;
        for (var i = 0; i < count; i++)
        {
            var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            var lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, LobbyNameKey);
            if (!string.IsNullOrEmpty(lobbyName))
            {
                var panel = Instantiate(steamLobbyPrefabUI, steamLobbiesParent).transform;
                panel.GetComponentInChildren<TMP_Text>().text = lobbyName;
                panel.GetComponentInChildren<Button>().onClick.AddListener(() => { SteamMatchmaking.JoinLobby(lobbyId); });
                panel.localPosition = new Vector3(0f, -30f * index, 0f);
                index++;
            }
        }
        steamLobbiesParent.sizeDelta = new Vector2(steamLobbyPlayerParent.sizeDelta.x, 30 * index);

    }

    private void OnLobbyChatUpdated(LobbyChatUpdate_t callback)
    {
        Debug.Log("OnLobbyChatUpdated was called");
        UpdatePlayers();
    }

    private void UpdatePlayers()
    {
        var count = SteamMatchmaking.GetNumLobbyMembers(currentLobbyId);

        while(steamLobbyPlayerParent.childCount < count)
        {
            Debug.Log("Instantiate new player UI element");
            Instantiate(steamPlayerPrefabUI, steamLobbyPlayerParent);
        }

        for(var i = 0; i< steamLobbyPlayerParent.childCount - count; i++)
        {
            Debug.Log("Destroing old player UI element");
            Destroy(steamLobbyPlayerParent.GetChild(steamLobbyPlayerParent.childCount - 1).gameObject);
        }
        steamLobbyPlayerParent.sizeDelta = new Vector2(steamLobbyPlayerParent.sizeDelta.x, 30 * count);
        for (var i = 0; i< count; i++)
        {
            var playerId = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyId, i);
            var playerName = SteamFriends.GetFriendPersonaName(playerId);
            var panel = steamLobbyPlayerParent.GetChild(i);
            panel.GetComponentInChildren<TMP_Text>().text = playerName;
            panel.localPosition = new Vector3(0f, -30f * i, 0f);
        }

    }

}
