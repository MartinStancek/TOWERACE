using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public Color playerColor;

    private int towerIndex = 0;

    private bool isVAxisInUse = false;

    public GameObject towerPrefab;

    private GameObject actualTower = null;


    private void Update()
    {
        if (GameController.Instance.gameMode != GameMode.TOWER_PLACING)
        {
            return;
        }
        var vInput = Input.GetAxis("Horizontal" + playerIndex);

        if (vInput != 0)
        {
            if (isVAxisInUse == false)
            {
                if(vInput < 0)
                {
                    MoveLeft();
                }
                if (vInput > 0)
                {
                    MoveRight();
                }
                isVAxisInUse = true;
            }
        }
        if (vInput == 0)
        {
            isVAxisInUse = false;
        }
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
            foreach (var mesh in actualTower.GetComponent<Tower>().coloredParts)
            {
                mesh.material.color = playerColor;
            }
        }
    }
}
