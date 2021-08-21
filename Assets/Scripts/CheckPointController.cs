using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckPointController : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private Transform checkPoints;

    public int lastCheckPointIndex = -1;

    public int lastPassed = -1;


    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        checkPoints = GameController.Instance.checkPonts;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("CheckPoint"))
        {
            var passed = other.transform.GetSiblingIndex();
            if (lastCheckPointIndex + 1 < checkPoints.childCount && checkPoints.GetChild(lastCheckPointIndex + 1).Equals(other.transform))
            {
                lastCheckPointIndex++;
                //Debug.Log("CheckPoint:" + lastCheckPointIndex);

            }
            //Debug.Log("passed: " + passed + ", checkPoints.childCount: " + checkPoints.childCount);
            if (passed < lastPassed)
            {
                //Debug.Log("setting wrongway");
                SetWrongWay(true);
            } 
            else
            {
                //Debug.Log("setting normalway");
                SetWrongWay(false);
            }

            lastPassed = passed < checkPoints.childCount - 1 ? passed : -1;

            if (lastCheckPointIndex + 1 == checkPoints.childCount) 
            {
                lastCheckPointIndex++;
                var playerIndex = GetComponent<CarSphere>().carObject.GetComponentInParent<Player>().playerIndex;
                GameController.Instance.CarFinished(playerIndex);
                Debug.Log("Player " + playerIndex + " finished the race!");
                GetComponent<CarSphere>().carObject.GetComponent<CarController>().isActivated = false;
                /*GetComponent<MSVehicleControllerFree>().handBrakeTrue = true;
                GetComponent<MSVehicleControllerFree>().raceStarted = false; ;
                GetComponent<MSVehicleControllerFree>().isInsideTheCar = false;*/

            }
            GameController.Instance.UpdateCheckPointPanel();
        }
    }

    private void SetWrongWay(bool value)
    {
        var player = GetComponent<CarSphere>().carObject.transform.parent.GetComponent<Player>();
        player.outline.SetWrongWay(value);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.other.CompareTag("Floor"))
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;

            Respawn(true);

        }
    }

    public void Respawn(bool resetCamera)
    {
        Debug.Log("target: " + checkPoints.GetChild(mod(lastCheckPointIndex, checkPoints.childCount)));
        Debug.Log("nextTarget: " + checkPoints.GetChild(mod(lastCheckPointIndex + 1, checkPoints.childCount)));

        Transform target = checkPoints.GetChild(mod(lastCheckPointIndex, checkPoints.childCount));
        Transform nextTarget = checkPoints.GetChild(mod(lastCheckPointIndex + 1, checkPoints.childCount));

        rb.GetComponent<CarSphere>().carObject.transform.position = target.position;
        rb.GetComponent<CarSphere>().carObject.transform.LookAt(nextTarget);

        rb.GetComponent<CarSphere>().isRespawned = true;

        rb.GetComponent<CarSphere>().carObject.GetComponent<CarController>().RestartPostion(target.position, 0.6f);
        rb.GetComponent<CarSphere>().carObject.transform.position = target.position;
        rb.GetComponent<CarSphere>().carObject.transform.LookAt(nextTarget);
        if (resetCamera)
        {
            rb.GetComponent<CarSphere>().vCAM.PreviousStateIsValid = false;
        }
        if (revertRespawnCor != null)
        {
            StopCoroutine(revertRespawnCor);
        }
        revertRespawnCor = StartCoroutine(RevertRespawn());
    }

    int mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }

    Coroutine revertRespawnCor = null;

    private IEnumerator RevertRespawn()
    {
        yield return new WaitForSeconds(1.5f);

        rb.GetComponent<CarSphere>().isRespawned = false;
        revertRespawnCor = null;

    }
}
