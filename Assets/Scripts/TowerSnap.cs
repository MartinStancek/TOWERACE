using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI.Messaging;
using MLAPI;
using MLAPI.NetworkVariable;
public class TowerSnap : NetworkBehaviour
{

    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly }, false);
    public NetworkVariable<ulong> occupiedBy = new NetworkVariable<ulong>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly }, 0);

    public int price = 60;

    public Player playerOwner = null;

    public List<MeshRendererMaterials> coloredParts;

    public Tower tower = null;
    public List<BoostPad> nearBoostPads;

    public List<InputImage> inputImages;


    List<Color> originalcolors = new List<Color>();


    void Start()
    {
        foreach (var cp in coloredParts)
        {
            foreach (var i in cp.materialIndexes)
            {
                originalcolors.Add(cp.renderer.materials[i].color);

            }
        }

        if (NetworkManager.Singleton.IsServer)
        {
            GameController.Instance.onRacingResultEnd.AddListener(() => { isOccupied.Value = false; });
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void LockServerRPC(ulong player)
    {
        isOccupied.Value = true;
        occupiedBy.Value = player;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockServerRPC()
    {
        isOccupied.Value = false;
        occupiedBy.Value = 0;

    }

    [ServerRpc(RequireOwnership = false)]
    public void BuySpotServerRPC(ulong playerId)
    {
        playerOwner = Player.FindById(playerId);
        playerOwner.money.Value -= price;
        BuySpotClientClientRPC(playerId);


    }

    [ClientRpc]
    public void BuySpotClientClientRPC(ulong playerId)
    {
        playerOwner = Player.FindById(playerId);
        SetColor(playerOwner.playerColor);

    }

    public void ResetColor()
    {
        var j = 0;
        foreach (var cp in coloredParts)
        {
            cp.SetColor(originalcolors[j++]);
        }
    }

    public void SetColor(Color c)
    {
        var j = 0;
        foreach(var cp in coloredParts)
        {
            cp.SetColor(c);
        }
    }
}
