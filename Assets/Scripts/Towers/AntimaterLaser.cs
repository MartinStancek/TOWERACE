using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntimaterLaser : MonoBehaviour
{
    AntimaterTower tower;
    private void Start()
    {
        tower = transform.parent.parent.parent.parent.GetComponent<AntimaterTower>();
    }
    void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        //Debug.Log("BoostPad trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere") && tower.GetTargets().Contains(t) && !t.GetComponent<CarSphere>().isRespawned)
        {
            t.GetComponent<CheckPointController>().Respawn();
            tower.destructionEffect.transform.position = t.position;
            tower.destructionEffect.Play();
        }
    }
}
