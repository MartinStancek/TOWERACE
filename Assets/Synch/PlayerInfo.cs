using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Linq;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    #region local_Singleton
    private static PlayerInfo _local;
    public static PlayerInfo Local
    {
        get
        {
            if (!_local)
            {
                _local = GameObject.FindObjectsOfType<PlayerInfo>().Where(e=>e.IsOwner).FirstOrDefault();
            }
            return _local;

        }
    }
    #endregion

    public NetworkVariable<FixedString32Bytes> Name = new NetworkVariable<FixedString32Bytes>();

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

    public override void OnNetworkSpawn()
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
}
