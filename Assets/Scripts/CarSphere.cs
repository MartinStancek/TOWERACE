using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarSphere : MonoBehaviour
{
    public GameObject carObject;

    public Transform shootTarget;

    public bool isRespawned = false;

    public CinemachineVirtualCamera vCAM;

    private CheckPointController res;
    void Start()
    {
        res = GetComponent<CheckPointController>();
    }

    void Update()
    {
        if(Vector3.Angle(-carObject.transform.up, Vector3.down) > 60f)
        {
            Debug.Log("Respawning car because of ground angle");
            res.Respawn(true);
        }
    }

}
