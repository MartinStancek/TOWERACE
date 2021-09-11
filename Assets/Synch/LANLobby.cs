using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;

public class LANLobby : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ConnectToHost()
    {
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = MenuManager.Instance.lanConnectIP.text;
        NetworkManager.Singleton.StartClient();
    }
}
