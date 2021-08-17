using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenStop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        //Debug.Log("BoostPad trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere"))
        {

            var cc = t.GetComponent<CarSphere>().carObject.GetComponent<CarController>();
            cc.SetCarSkin();
        }
    }
}
