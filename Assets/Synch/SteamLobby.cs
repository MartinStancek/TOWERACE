using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Unity.Netcode;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Netcode.Transports;

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
    protected Callback<LobbyChatMsg_t> lobbyChatMsgRecieved;

    private CSteamID currentLobbyId;
    private List<CSteamID> lobbyList;
    private int lastLobbyRequested = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized) { return; }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
        lobbyChatUpdated = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdated);
        lobbyMatchListUpdated = Callback<LobbyMatchList_t>.Create(OnLobbyMatchListUpdated);
        lobbyChatMsgRecieved = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsgRecieved);
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

    }

    public void StartGame()
    {
        if (!SteamManager.Initialized) { return; }
        if (!IsCurrentUserOwner()) { return; }

        Debug.Log("StartGame was called");
        SetPanel(null);
        SteamMatchmaking.SetLobbyData(currentLobbyId, GameStartedKey, "true");
        networkManager.StartHost();
        var progress = NetworkManager.Singleton.SceneManager.LoadScene("MartinScene3", UnityEngine.SceneManagement.LoadSceneMode.Single);
        /*progress.OnComplete += (timeOut) =>
        {*/
            Debug.Log("On complete");
            StartCoroutine(SpawnCarsWithDelay());
/*
        };*/
    }

    IEnumerator SpawnCarsWithDelay()
    {

        yield return new WaitForSeconds(5f);
        foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("foreach loop");
            var go = Instantiate(GameController.Instance.playerPrefab);
            go.GetComponent<NetworkObject>().SpawnWithOwnership(c.ClientId);
        }
    }


    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        if (IsCurrentUserOwner())
        {
            Debug.Log("Setting Lobby name to: " + lobbyNameField.text);
            SteamMatchmaking.SetLobbyData(currentLobbyId, LobbyNameKey, lobbyNameField.text);
            SteamMatchmaking.SetLobbyData(currentLobbyId, GameStartedKey, "false");
            SteamMatchmaking.SetLobbyData(currentLobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        }

        Debug.Log("OnLobbyEntered was called");
        SetPanel(lobbyPanel);
        UpdatePlayers();
    }

    private void OnLobbyDataUpdated(LobbyDataUpdate_t callback)
    {
        if (!currentLobbyId.Equals(new CSteamID(callback.m_ulSteamIDLobby)))
        {
            Debug.Log("Updating Lobby " + callback.m_ulSteamIDLobby + "DataCount: " + SteamMatchmaking.GetLobbyDataCount(new CSteamID(callback.m_ulSteamIDLobby)));

            UpdateLobbies(new CSteamID(callback.m_ulSteamIDLobby));
            return;
        }

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
            networkManager.GetComponent<SteamNetworkingTransport>().ConnectToSteamID = Convert.ToUInt64(hostAddress); // or callback.m_ulSteamIDLobby
            networkManager.StartClient();
            SetPanel(null);

        }
    }

    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(currentLobbyId);
        currentLobbyId = CSteamID.Nil;
        SetPanel(introPanel);
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
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose);
        //SteamMatchmaking.AddRequestLobbyListStringFilter(GameStartedKey, "false", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();
        steamLobbiesLoadingText.gameObject.SetActive(true);
        SetPanel(findLobbyPanel);

        for(var i = 1; i< steamLobbiesParent.childCount; i++)
        {
            Destroy(steamLobbiesParent.GetChild(i).gameObject);
        }
        steamLobbiesParent.sizeDelta = new Vector2(steamLobbyPlayerParent.sizeDelta.x, 30 * (steamLobbiesParent.childCount));

    }

    private void OnLobbyMatchListUpdated(LobbyMatchList_t callback)
    {
        steamLobbiesLoadingText.gameObject.SetActive(false);
        var count = callback.m_nLobbiesMatching;
        lobbyList = new List<CSteamID>();
        for (var i = 0; i < count; i++)
        {
            var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyList.Add(lobbyId);


        }
        lastLobbyRequested = 0;
        SteamMatchmaking.RequestLobbyData(lobbyList[lastLobbyRequested]);



    }

    private void OnLobbyChatUpdated(LobbyChatUpdate_t callback)
    {
        Debug.Log("OnLobbyChatUpdated was called");
        UpdatePlayers();

    }

    private void UpdateLobbies(CSteamID lobbyChanged)
    {
        if(SteamMatchmaking.GetLobbyDataCount(lobbyChanged)> 0)
        {
            string key;
            string value;
            SteamMatchmaking.GetLobbyDataByIndex(lobbyChanged, 0, out key, 10000, out value, 10000);
            Debug.Log("key: "+ key+ ", value: "+value);

        }
        var lobbyName = SteamMatchmaking.GetLobbyData(lobbyChanged, LobbyNameKey);
        if (!string.IsNullOrEmpty(lobbyName))
        {
            var panel = Instantiate(steamLobbyPrefabUI, steamLobbiesParent).transform;
            panel.GetComponentInChildren<TMP_Text>().text = lobbyName;
            panel.GetComponentInChildren<Button>().onClick.AddListener(() => { SteamMatchmaking.JoinLobby(lobbyChanged); });
            panel.localPosition = new Vector3(0f, -30f * (steamLobbiesParent.childCount - 2), 0f);
        }
        steamLobbiesParent.sizeDelta = new Vector2(steamLobbyPlayerParent.sizeDelta.x, 30 * (steamLobbiesParent.childCount - 1));
        lastLobbyRequested += 1;
        if (lastLobbyRequested < lobbyList.Count)
        {
            SteamMatchmaking.RequestLobbyData(lobbyList[lastLobbyRequested]);
        }
    }

    private void UpdatePlayers()
    {
        var count = SteamMatchmaking.GetNumLobbyMembers(currentLobbyId);

        while(steamLobbyPlayerParent.childCount < count)
        {
            Debug.Log("Instantiate new player UI element");
            Instantiate(steamPlayerPrefabUI, steamLobbyPlayerParent);
        }

        for (var i = 0; i < steamLobbyPlayerParent.childCount - count; i++)
        {
            Debug.Log("Destroing old player UI element");
            Destroy(steamLobbyPlayerParent.GetChild(steamLobbyPlayerParent.childCount - 1).gameObject);
        }
        steamLobbyPlayerParent.sizeDelta = new Vector2(steamLobbyPlayerParent.sizeDelta.x, 30 * count + 1);
        for (var i = 0; i< count; i++)
        {
            var playerId = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyId, i);
            var playerName = SteamFriends.GetFriendPersonaName(playerId);
            var panel = steamLobbyPlayerParent.GetChild(i);
            panel.GetComponentInChildren<TMP_Text>().text = playerName;
            panel.GetComponentInChildren<Button>().interactable = IsCurrentUserOwner();
            panel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            panel.GetComponentInChildren<Button>().onClick.AddListener(()=> { 
                SteamMatchmaking.SendLobbyChatMsg(
                    currentLobbyId, 
                    BitConverter.GetBytes((int)EChatEntryType.k_EChatEntryTypeWasKicked)
                        .Concat(BitConverter.GetBytes(playerId.m_SteamID)).ToArray(), 
                    300);
                panel.GetComponentInChildren<Button>().interactable = false;
            });

            panel.localPosition = new Vector3(0f, -30f * i, 0f);
        }

    }

    private void OnLobbyChatMsgRecieved(LobbyChatMsg_t callback)
    {
        Debug.Log("calltype: " + ((int)callback.m_eChatEntryType));
        if ((int)callback.m_eChatEntryType == (int)EChatEntryType.k_EChatEntryTypeChatMsg) 
        {
            byte[] msgData = new byte[300];
            CSteamID user;
            EChatEntryType type;
            SteamMatchmaking.GetLobbyChatEntry(currentLobbyId, (int)callback.m_iChatID, out user, msgData, 300, out type);

            var msgType = BitConverter.ToInt32(msgData, 0);
            var msgUserToKick = BitConverter.ToUInt64(msgData, 0 + sizeof(int));
            Debug.Log("msgtype: " + msgType + ", msgUser: " + msgUserToKick);
            if(msgType == (int)EChatEntryType.k_EChatEntryTypeWasKicked && SteamUser.GetSteamID().Equals(new CSteamID(msgUserToKick)))
            {
                Debug.Log("I was kicked from the session :(");
                LeaveLobby();
            }
        }
    }

    public void InviteFriends()
    {
        if (!currentLobbyId.Equals(CSteamID.Nil)){
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyId);
        }
    }

    private bool IsCurrentUserOwner() => SteamMatchmaking.GetLobbyOwner(currentLobbyId).Equals(SteamUser.GetSteamID());

}
