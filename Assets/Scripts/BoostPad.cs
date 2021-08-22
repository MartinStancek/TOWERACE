using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float accelBoost = 18;
    public float boostForSeconds = 1f;
    void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        //Debug.Log("BoostPad trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere"))
        {
            StartCoroutine(ActivateBoost(t.GetComponent<CarSphere>().carObject.GetComponent<CarController>()));
        }
    }

    private IEnumerator ActivateBoost(CarController car)
    {
        car.forwardAccel = accelBoost;
        SoundManager.PlaySound(SoundManager.SoundType.BOOST);
        yield return new WaitForSeconds(boostForSeconds);
        car.forwardAccel = car.originalAccel;
    }

}
