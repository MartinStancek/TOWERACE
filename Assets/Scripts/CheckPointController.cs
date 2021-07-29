using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private Transform checkPoints;

    public int lastCheckPointIndex = -1;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        checkPoints = GameController.Instance.checkPonts;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.gameObject.layer & LayerMask.NameToLayer("CheckPoint")) != 0)
        {
            if (lastCheckPointIndex + 1 < checkPoints.childCount && checkPoints.GetChild(lastCheckPointIndex + 1).Equals(other.transform))
            {
                lastCheckPointIndex++;
                //Debug.Log("CheckPoint:" + lastCheckPointIndex);
            }

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

        }
    }

    public void RestartCar()
    {

    }
}
