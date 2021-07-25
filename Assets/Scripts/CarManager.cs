using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public GameObject checkPoints;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private int lastCheckPointIndex = -1;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.gameObject.layer & LayerMask.NameToLayer("CheckPoint")) != 0)
        {
            if (lastCheckPointIndex + 1 < checkPoints.transform.childCount && checkPoints.transform.GetChild(lastCheckPointIndex + 1).Equals(other.transform))
            {
                lastCheckPointIndex++;
                //Debug.Log("CheckPoint:" + lastCheckPointIndex);
            }

            if (lastCheckPointIndex + 1 == checkPoints.transform.childCount) 
            {
                lastCheckPointIndex++;
                GameController.Instance.CarFinished();
                Debug.Log("Player "+GetComponent<MSVehicleControllerFree>().playerIndex+" finished the race!");
                GetComponent<MSVehicleControllerFree>().handBrakeTrue = true;
                GetComponent<MSVehicleControllerFree>().raceStarted = false; ;
                GetComponent<MSVehicleControllerFree>().isInsideTheCar = false;

            }

        }
    }

    public void RestartCar()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        lastCheckPointIndex = -1;
    }
}
