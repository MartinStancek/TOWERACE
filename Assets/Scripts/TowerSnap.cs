using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

using System;
public class TowerSnap : NetworkBehaviour
{
    [SerializeField]
    public TowerOptions towerOptions;

    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>();
    public NetworkVariable<ulong> occupiedBy = new NetworkVariable<ulong>();

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
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isOccupied.Value = false;
            occupiedBy.Value = 0;
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

            BuyTowerClientRPC(no.NetworkObjectId, towerIndex);
        }catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [ClientRpc]
    public void BuyTowerClientRPC(ulong towerId, int towerIndex)
    {
        foreach (var pad in nearBoostPads)
        {
            pad.gameObject.SetActive(false);
        }
        StartCoroutine(InitTower(towerId, towerIndex));
    }

    private IEnumerator InitTower(ulong towerId, int towerIndex)
    {
        yield return 1;
        var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[towerId].GetComponent<Tower>();

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
