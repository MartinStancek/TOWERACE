using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using MLAPI;
using MLAPI.Transports.SteamP2P;
using System;

public class SteamLobby : MonoBehaviour
{
    public NetworkManager networkManager;

    public GameObject panels;

    private const string HostAddressKey = "HostAddress";

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized) { return; }
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        panels.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 10);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            panels.SetActive(true);
            return; 
        }
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), 
            HostAddressKey, 
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (networkManager.IsHost) { return; }
        var hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);
        networkManager.GetComponent<SteamP2PTransport>().ConnectToSteamID = Convert.ToUInt64(hostAddress);

        networkManager.StartClient();

        panels.SetActive(false);


    }
}
