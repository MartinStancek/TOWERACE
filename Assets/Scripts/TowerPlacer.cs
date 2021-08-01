using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class TowerData
{
    public GameObject prefab;
    public int price;
    public Sprite preview;
}

public enum TowerPlaceState
{
    CHOOSING_SPOT, CHOOSING_TOWER
}

[RequireComponent(typeof(Player))]
public class TowerPlacer : MonoBehaviour
{
    public int snapIndex = 0;
    public int towerIndex = 0;

    private Player player;

    public GameObject towerPointer;

    public List<TowerData> towerOptions = new List<TowerData>();

    private GameObject actualTowerPointer = null;

    private bool rightInput = false;
    private bool leftInput = false;
    private bool clickInput = false;
    private bool backInput = false;
    
    private float lastLeftClickTime = 0f;
    private float lastRightClickTime = 0f;
    private float lastClickTime = 0f;
    private float lastBackClickTime = 0f;

    public float clickTime = 0.2f;

    public TowerPlaceState placingState = TowerPlaceState.CHOOSING_SPOT;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    public void OnTowerLeft(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        //Debug.Log("Right: "+value);
        if(value > 0.5)
        {
            leftInput = true;
        } 
        else
        {
            leftInput = false;
            lastLeftClickTime = 0f;
        }
    }
    public void OnTowerRight(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        if (value > 0.5)
        {
            rightInput = true;
        }
        else
        {
            rightInput = false;
            lastRightClickTime = 0f;
        }
    }
    public void OnTowerClick(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        if (value > 0.5)
        {
            clickInput = true;
        }
        else
        {
            clickInput = false;
            lastClickTime = 0f;
        }
    }
    public void OnTowerBackClick(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        if (value > 0.5)
        {
            backInput = true;
        }
        else
        {
            backInput = false;
            lastBackClickTime = 0f;
        }
    }

    void Update()
    {
        if(GameController.Instance.gameMode != GameMode.TOWER_PLACING)
        {
            return;
        }

        if(leftInput && Time.time - lastLeftClickTime > clickTime)
        {
            MoveLeft();
            lastLeftClickTime = Time.time;
        }
        if (rightInput && Time.time - lastRightClickTime > clickTime)
        {
            MoveRight();
            lastRightClickTime = Time.time;
        }

        if (clickInput && Time.time - lastClickTime > 10f)//musi odkliknut
        {
            Clicked();
            lastClickTime = Time.time;
        }

        if (backInput && Time.time - lastBackClickTime > 10f)//musi odkliknut
        {
            BackClicked();
            lastBackClickTime = Time.time;
        }
    }
    public void BackClicked()
    {
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();

        if (placingState == TowerPlaceState.CHOOSING_TOWER)
        {
            placingState = TowerPlaceState.CHOOSING_SPOT;
            snap.SetPanel(snap.selectSnapPanel, player.playerColor);

        }
    }
    public void Clicked()
    {
        Debug.Log("Clicked");
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        if (player.money >= snap.price && snap.playerOwner == null)
        {
            Debug.Log("Player " + player.playerIndex + " is buying snap " + snap.name);
            snap.SetColor(player.playerColor);
            snap.playerOwner = player;
            player.money -= snap.price;
            placingState = TowerPlaceState.CHOOSING_TOWER;
            snap.SetPanel(snap.buyTowerPanel, player.playerColor);
            SetTowerInMenu();
        }
        else if(snap.playerOwner != null && snap.playerOwner.Equals(player) && placingState == TowerPlaceState.CHOOSING_SPOT)
        {
            Debug.Log("Player " + player.playerIndex + " Entering snap " + snap.name);
            snap.SetPanel(snap.buyTowerPanel, player.playerColor);
            placingState = TowerPlaceState.CHOOSING_TOWER;
            SetTowerInMenu();
        }
        else if(snap.playerOwner != null && snap.playerOwner.Equals(player) && snap.tower == null && player.money >= towerOptions[towerIndex].price)
        {
            Debug.Log("Player " + player.playerIndex + " is buying " + towerOptions[towerIndex].prefab.name);

            player.money -= towerOptions[towerIndex].price;
            var go = Instantiate(towerOptions[towerIndex].prefab, snap.transform.position, snap.transform.rotation);
            snap.tower = go.GetComponent<Tower>();
            foreach(var p in snap.tower.coloredParts)
            {
                p.material.color = player.playerColor;
            }
            snap.tower.playerOwner = player.playerIndex;


        }
    }

    public void MoveLeft()
    {
        switch (placingState)
        {
            case TowerPlaceState.CHOOSING_SPOT:
                MoveSpotLeft();
                break;
            case TowerPlaceState.CHOOSING_TOWER:
                MoveTowerLeft();
                break;
        }

    }
    public void MoveRight()
    {
        switch (placingState)
        {
            case TowerPlaceState.CHOOSING_SPOT:
                MoveSpotRight();
                break;
            case TowerPlaceState.CHOOSING_TOWER:
                MoveTowerRight();
                break;
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
        var snap = GameController.Instance.towersSnapParent.transform.GetChild(snapIndex).GetComponent<TowerSnap>();
        var panel = snap.buyTowerPanel;
        panel.Find("Price").GetComponent<TMP_Text>().text = "" + towerOptions[towerIndex].price + "$";
        panel.Find("Name").GetComponent<TMP_Text>().text = towerOptions[towerIndex].prefab.name;
        panel.Find("TowerPreview").GetComponent<SpriteRenderer>().sprite = towerOptions[towerIndex].preview;

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
        if(previousSnap != null)
        {
            previousSnap.isOccupied = false;
            previousSnap.SetPanel(null);
        }

        targetSnap.isOccupied = true;
        if (targetSnap.playerOwner == null)
        {
            targetSnap.SetPanel(targetSnap.buySnapPanel, player.playerColor);
        }
        else
        {
            targetSnap.SetPanel(targetSnap.selectSnapPanel, player.playerColor);
        }
    }

    public void ClaimRandomSpot()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnaps(0, count, player);
        if(snaps.Count != 0)
        {
            var tmpIndex = Random.Range(0, snaps.Count);
            snapIndex = GameController.Instance.IndexOfSnap(snaps[tmpIndex]);
            snaps[tmpIndex].isOccupied = true;
            snaps[tmpIndex].SetPanel(snaps[tmpIndex].buySnapPanel, player.playerColor);
        }
    }
}
