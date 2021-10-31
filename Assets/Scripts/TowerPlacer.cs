using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

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

    public TowerOptions towerOptions;

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
            MoveSpot(Vector2.left);

        }
    }
    public void OnSpotRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnSpotRight performed");
            MoveSpot(Vector2.right);

        }
    }
    public void OnSpotUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnSpotLeft performed");
            MoveSpot(Vector2.up);

        }
    }
    public void OnSpotDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Debug.Log("OnSpotRight performed");
            MoveSpot(Vector2.down);

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
            OnSpotClick();
        }
    }

    public void OnSpotClick()
    {
        Debug.Log("OnSpotClick performed");
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        if (snap.playerOwner == null)
        {
            pointer.SetPanel(pointer.buySpot);
            pointer.snapPrice.text = "" + snap.price + " $";
            if (player.playerInput.currentActionMap != null)
            {
                player.playerInput.SwitchCurrentActionMap("Towers");
            }
        }
        else if (snap.tower == null)
        {
            pointer.SetPanel(pointer.selectTower);
            if (player.playerInput.currentActionMap != null)
            {
                player.playerInput.SwitchCurrentActionMap("Towers");
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
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();

        pointer.SetPanel(pointer.selectSpot);
        player.playerInput.SwitchCurrentActionMap("Spot");

    }

    public void Clicked()
    {
        Debug.Log("Clicked");
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();

        if (player.money.Value >= snap.price && snap.playerOwner == null)
        {
            Debug.Log("Player " + player.playerIndex + " is buying snap " + snap.name);
            SoundManager.PlaySound(SoundManager.SoundType.MONEY_SPEND);
            snap.BuySpotServerRPC(player.OwnerClientId);
            //placingState = TowerPlaceState.CHOOSING_TOWER;
            pointer.SetPanel(pointer.selectTower);
            SetTowerInMenu();
        }
        else if (snap.playerOwner != null && snap.playerOwner.Equals(player) && snap.tower == null && player.money.Value >= towerOptions.data[towerIndex].price)
        {
            Debug.Log("Player " + player.playerIndex + " is buying " + towerOptions.data[towerIndex].prefab.name);
            SoundManager.PlaySound(SoundManager.SoundType.MONEY_SPEND);
            /*
            */

            snap.BuyTowerServerRPC(player.OwnerClientId, towerIndex);
            /*

            */

            pointer.SetTowerName(towerOptions.data[towerIndex].prefab.name);

            //placingState = TowerPlaceState.CHOOSING_SPOT;
            if (player.playerInput.currentActionMap != null)
            {
                player.playerInput.SwitchCurrentActionMap("Spot");
            }

            ClaimRandomSpot();


        }
    }

    public void MoveTowerLeft()
    {
        if(towerIndex > 0)
        {
            towerIndex--;
            SetTowerInMenu();
        }
    }
    public void MoveTowerRight()
    {
        if (towerIndex < towerOptions.data.Count - 1)
        {
            towerIndex++;
            SetTowerInMenu();
        }
    }
    private void SetTowerInMenu()
    {
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();

        pointer.towerPrice.text = "" + towerOptions.data[towerIndex].price + " $";
        pointer.towerName.text = towerOptions.data[towerIndex].prefab.name;
        pointer.towerPreview.sprite = towerOptions.data[towerIndex].preview;


        var disableCol = Color.white;
        disableCol.a = 0.5f;
        if (towerIndex == towerOptions.data.Count - 1)
        {
            pointer.towerRight.color = disableCol;
        } 
        else
        {
            pointer.towerRight.color = Color.white;
        }

        if (towerIndex == 0)
        {
            pointer.towerLeft.color = disableCol;
        }
        else
        {
            pointer.towerLeft.color = Color.white;
        }

    }

    public void MoveSpot(Vector2 direction)
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnapsInDirection(snapIndex, direction, player);

        //Debug.Log("Moving in direction: " + direction + "actualIndex: " + snapIndex + "SnapsCount: " + snaps.Count);

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
            previousSnap.UnlockServerRPC();
            //previousSnap.SetPanel(null, -1);
        }

        targetSnap.LockServerRPC(PlayerInfo.Local.OwnerClientId);
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();

        pointer.SetPointer(targetSnap.gameObject);
        if (targetSnap.tower == null)
        {
            pointer.SetPanel(pointer.selectSpot);
        } 
    }

    public void ClaimRandomSpot()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetAllFreeTowerSnapes(player);
        var pointer = GameController.Instance.towerPointerParent.GetChild(0).GetComponent<TowerPointerUI>();

        if (snaps.Count != 0)
        {
            var origTS = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
            var tmpIndex = Random.Range(0, snaps.Count);
            snapIndex = GameController.Instance.IndexOfSnap(snaps[tmpIndex]);
            SetSnap(snaps[tmpIndex], origTS);
        } 
        else
        {
            pointer.SetPanel(pointer.soldOut);
        }
    }
}
