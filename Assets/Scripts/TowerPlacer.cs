using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    public int playerIndex;
    public Color playerColor;

    private int towerIndex = 0;

    private bool isVAxisInUse = false;

    public GameObject towerPrefab;

    private GameObject actualTower = null;

    private bool rightInput = false;
    private bool leftInput = false;
    private float lastLeftClickTime = 0f;
    private float lastRightClickTime = 0f;

    public float clickTime = 0.2f;


    public void OnTowerLeft(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        Debug.Log("Right: "+value);
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


    }

    public void OnTowerClick(InputAction.CallbackContext context)
    {

    }



    public void MoveLeft()
    {
        var snaps = GameController.Instance.GetFreeTowerSnaps(0, towerIndex);
        if (snaps.Count != 0) {
            GameController.Instance.towersSnapParent.transform.GetChild(towerIndex).GetComponent<TowerSnap>().isOccupied = false;
            snaps[snaps.Count - 1].isOccupied = true;
            towerIndex = GameController.Instance.IndexOfSnap(snaps[snaps.Count - 1]);

            actualTower.transform.position = snaps[snaps.Count - 1].transform.position;

            Debug.Log("MoveLeft " + towerIndex + ", " + snaps[snaps.Count - 1].gameObject.name);
        }
    }

    public void MoveRight()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnaps(towerIndex+1, count);

        if (snaps.Count != 0)
        {
            GameController.Instance.towersSnapParent.transform.GetChild(towerIndex).GetComponent<TowerSnap>().isOccupied = false;

            towerIndex = GameController.Instance.IndexOfSnap(snaps[0]);
            snaps[0].isOccupied = true;
            actualTower.transform.position = snaps[0].transform.position;
            Debug.Log("MoveRight " + towerIndex + ", " + snaps[0].gameObject.name);
        }
    }

    public void ClaimRandomSpot()
    {
        var count = GameController.Instance.towersSnapParent.transform.childCount;
        var snaps = GameController.Instance.GetFreeTowerSnaps(0, count);
        if(snaps.Count != 0)
        {
            var snapIndex = Random.Range(0, snaps.Count);
            towerIndex = GameController.Instance.IndexOfSnap(snaps[snapIndex]);
            snaps[snapIndex].isOccupied = true;
            Debug.Log("Claiming number: " + snapIndex);

            actualTower = Instantiate(towerPrefab, snaps[snapIndex].transform.position, snaps[snapIndex].transform.rotation);
            var tower = actualTower.GetComponent<Tower>();
            tower.playerOwner = playerIndex;
            foreach (var mesh in tower.coloredParts)
            {
                var a = mesh.material.color.a;
                mesh.material.color = new Color(playerColor.r, playerColor.g, playerColor.b, a);

            }
        }
    }
}
