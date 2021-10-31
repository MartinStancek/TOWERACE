using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI.Messaging;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using System;
public class TowerSnap : NetworkBehaviour
{
    [SerializeField]
    public TowerOptions towerOptions;

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

    [ServerRpc(RequireOwnership = false)]
    public void BuyTowerServerRPC(ulong playerId, int towerIndex)
    {
        try
        {
            var player = Player.FindById(playerId);
            player.money.Value -= towerOptions.data[towerIndex].price;

            var go = Instantiate(towerOptions.data[towerIndex].prefab, transform.position, transform.rotation);
            var no = go.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(playerId);
            ;

            BuyTowerClientRPC(playerId, no.NetworkObjectId, towerIndex);
        }catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [ClientRpc]
    public void BuyTowerClientRPC(ulong playerId, ulong towerId, int towerIndex)
    {
        foreach (var pad in nearBoostPads)
        {
            pad.gameObject.SetActive(false);
        }

        var tower = NetworkSpawnManager.SpawnedObjects[towerId].GetComponent<Tower>();


        GameObject go;
        if (towerIndex == 4)
        {
            tower.transform.SetParent(transform);
        }

        tower.name = towerOptions.data[towerIndex].prefab.name;
        this.tower = tower;
        foreach (var p in tower.coloredParts)
        {
            p.SetColor(playerOwner.playerColor);
        }
        tower.playerOwner = playerOwner.playerIndex;

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
        foreach (var cp in coloredParts)
        {
            cp.SetColor(c);
        }
    }
}
