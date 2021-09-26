using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;

public class LANLobby : MonoBehaviour
{
    public void StartHost()
    {
        if (!CanConnect(true)) { return; }
        
        NetworkManager.Singleton.StartHost();
    }

    public void ConnectToHost()
    {
        if (!CanConnect()) { return; }

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = MenuManager.Instance.lanConnectIP.text;
        NetworkManager.Singleton.StartClient();
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
}
