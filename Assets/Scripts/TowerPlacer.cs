using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class TowerData
{
    public GameObject prefab;
    public int price;
    public Sprite preview;
}
/*
public enum TowerPlaceState
{
    BUYING_SPOT, CHOOSING_TOWER
}*/

[RequireComponent(typeof(Player))]
public class TowerPlacer : MonoBehaviour
{
    public int snapIndex = 0;
    public int towerIndex = 0;

    private Player player;

    public GameObject towerPointer;

    public List<TowerData> towerOptions = new List<TowerData>();

    private GameObject actualTowerPointer = null;

    /*public TowerPlaceState placingState = TowerPlaceState.BUYING_SPOT;*/

    void Awake()
    {
        player = GetComponent<Player>();
    }

    public void OnTowerLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnTowerLeft performed");
            MoveTowerLeft();
        }
    }
    public void OnTowerRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnTowerRight performed");
            MoveTowerRight();

        }
    }
    public void OnSpotLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnSpotLeft performed");
            MoveSpotLeft();

        }
    }
    public void OnSpotRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnSpotRight performed");
            MoveSpotRight();

        }
    }
    public void OnTowerClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnTowerClick performed");
            Clicked();

        }
    }
    public void OnSpotClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("OnSpotClick performed");
            var pointer = GameController.Instance.towerPointerParent.GetChild(player.playerIndex).GetComponent<TowerPointerUI>();
            var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
            if(snap.playerOwner == null)
            {
                pointer.SetPanel(pointer.buySpot);
                pointer.buySpot.Find("Price").GetComponent<TMP_Text>().text = "" + snap.price + "$";
                player.playerInput.SwitchCurrentActionMap("Towers");
            }
            else if(snap.tower == null)
            {
                pointer.SetPanel(pointer.selectTower);
                player.playerInput.SwitchCurrentActionMap("Towers");
            }
            else
            {
                pointer.SetPanel(pointer.tower);
                pointer.SetTowerName(snap.tower.name);

            }

        }
    }
    public void OnTowerBackClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("OnTowerBackClick performed");
            BackClicked();
        }
    }
    
    public void BackClicked()
    {
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        var pointer = GameController.Instance.towerPointerParent.GetChild(player.playerIndex).GetComponent<TowerPointerUI>();

        pointer.SetPanel(pointer.selectSpot);
        player.playerInput.SwitchCurrentActionMap("Spot");

    }

    public void Clicked()
    {
        Debug.Log("Clicked");
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        var pointer = GameController.Instance.towerPointerParent.GetChild(player.playerIndex).GetComponent<TowerPointerUI>();

        if (player.money >= snap.price && snap.playerOwner == null)
        {
            Debug.Log("Player " + player.playerIndex + " is buying snap " + snap.name);
            snap.SetColor(player.playerColor);
            snap.playerOwner = player;
            player.money -= snap.price;
            //placingState = TowerPlaceState.CHOOSING_TOWER;
            pointer.SetPanel(pointer.selectTower);
            SetTowerInMenu();
        }
        else if (snap.playerOwner != null && snap.playerOwner.Equals(player) && snap.tower == null && player.money >= towerOptions[towerIndex].price)
        {
            Debug.Log("Player " + player.playerIndex + " is buying " + towerOptions[towerIndex].prefab.name);

            player.money -= towerOptions[towerIndex].price;
            var go = Instantiate(towerOptions[towerIndex].prefab, snap.transform.position, snap.transform.rotation);
            go.name = towerOptions[towerIndex].prefab.name;
            snap.tower = go.GetComponent<Tower>();
            foreach (var p in snap.tower.coloredParts)
            {
                p.SetColor(player.playerColor);
            }
            snap.tower.playerOwner = player.playerIndex;
            pointer.SetPanel(pointer.tower);
            pointer.SetTowerName(snap.tower.name);

            //placingState = TowerPlaceState.CHOOSING_SPOT;
            player.playerInput.SwitchCurrentActionMap("Spot");

            foreach (var pad in snap.nearBoostPads)
            {
                pad.gameObject.SetActive(false);
            }

            

        }
    }

    private void MoveTowerLeft()
    {
        if(towerIndex > 0)
        {
            towerIndex--;
            SetTowerInMenu();
        }
    }
    private void MoveTowerRight()
    {
        if (towerIndex < towerOptions.Count - 1)
        {
            towerIndex++;
            SetTowerInMenu();
        }
    }
    private void SetTowerInMenu()
    {
        var pointer = GameController.Instance.towerPointerParent.GetChild(player.playerIndex).GetComponent<TowerPointerUI>();
       
        var panel = pointer.selectTower;
        panel.Find("Price").GetComponent<TMP_Text>().text = "" + towerOptions[towerIndex].price + "$";
        panel.Find("NamePanel/Name").GetComponent<TMP_Text>().text = towerOptions[towerIndex].prefab.name;
        panel.Find("Preview").GetComponent<Image>().sprite = towerOptions[towerIndex].preview;

        if (towerIndex == towerOptions.Count - 1)
        {
            panel.Find("Right").GetComponent<TMP_Text>().color = Color.gray;
        } 
        else
        {
            panel.Find("Right").GetComponent<TMP_Text>().color = Color.black;
        }

        if (towerIndex == 0)
        {
            panel.Find("Left").GetComponent<TMP_Text>().color = Color.gray;
        }
        else
        {
            panel.Find("Left").GetComponent<TMP_Text>().color = Color.black;
        }

    }

    private void MoveSpotLeft()
    {
        var snaps = GameController.Instance.GetFreeTowerSnaps(0, snapIndex, player);
        if (snaps.Count != 0)
        {
            var origTS = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
            snapIndex = GameController.Instance.IndexOfSnap(snaps[snaps.Count - 1]);
            var nextTS = snaps[snaps.Count - 1];
            SetSnap(nextTS, origTS);
        }
    }

    private void MoveSpotRight()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnaps(snapIndex + 1, count, player);

        if (snaps.Count != 0)
        {
            var origTS = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
            snapIndex = GameController.Instance.IndexOfSnap(snaps[0]);
            var nextTS = snaps[0];
            SetSnap(nextTS, origTS);
        }
    }

    private void SetSnap(TowerSnap targetSnap, TowerSnap previousSnap)
    {
        if (previousSnap != null)
        {
            previousSnap.isOccupied = false;
            //previousSnap.SetPanel(null, -1);
        }

        targetSnap.isOccupied = true;
        var pointer = GameController.Instance.towerPointerParent.GetChild(player.playerIndex).GetComponent<TowerPointerUI>();

        pointer.SetPointer(targetSnap.gameObject);
        if (targetSnap.tower == null)
        {
            pointer.SetPanel(pointer.selectSpot);
        } 
        else
        {
            pointer.SetPanel(pointer.tower);
            pointer.SetTowerName(targetSnap.tower.name);
        }
    }

    public void ClaimRandomSpot()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnaps(0, count, player);
        if(snaps.Count != 0)
        {
            var origTS = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
            var tmpIndex = Random.Range(0, snaps.Count);
            snapIndex = GameController.Instance.IndexOfSnap(snaps[tmpIndex]);
            SetSnap(snaps[tmpIndex], origTS);
        }
    }
}
