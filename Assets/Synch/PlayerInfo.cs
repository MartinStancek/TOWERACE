using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using Steamworks;
using System;
using UnityEngine.InputSystem;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariableString Name = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private Player player;

    private int networkType;

    private void Awake()
    {
        Name.OnValueChanged += (prevValue, newValue) =>
        {
            if(networkType == MenuManager.LAN)
            {
                NetworkManager.Singleton.GetComponent<LANLobby>().UpdatePlayers();
            }
        };
    }

    public override void NetworkStart()
    {
        networkType = MenuManager.Instance.GetNetworkType();
        if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
        {
            switch (networkType)
            {
                case MenuManager.LAN:
                    Name.Value = MenuManager.Instance.lanName.text;
                    break;
                case MenuManager.STEAM:
                    Name.Value = SteamFriends.GetPersonaName();
                    break;
                default:
                    throw new Exception("Unknown networkValue " + networkType);
            }

        }
    }

    [ClientRpc]
    public void SpawnCarClientRpc()
    {
        Debug.Log("RPC SpawnCarClientRpc");
        try
        {

            var go = Instantiate(GameController.Instance.playerPrefab);
            go.GetComponent<NetworkObject>().Spawn();
            GameController.Instance.GetComponentInChildren<PlayerManager>().OnPlayerJoined(go.GetComponent<PlayerInput>());
            //GameController.Instance.StartRace();
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
