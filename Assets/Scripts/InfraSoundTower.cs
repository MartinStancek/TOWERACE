using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfraSoundTower : Tower
{
    public Transform head;
    public float strengthMultiplier = 40000f;
    public float backwardMultiplier = 40000f;
    private void Update()
    {
        if (Time.time - lastShot > shootEvery)
        {
            lastShot = Time.time;

            foreach(var rb in targets)
            {
                var cs = rb.GetComponent<CarSphere>();

                rb.AddForce((rb.transform.position - transform.position).normalized * strengthMultiplier);
                rb.AddForce(cs.carObject.transform.forward * -backwardMultiplier);

            }
        }

        if(targets.Count > 0)
        {
            head.LookAt(targets[0].transform.position);
        }
    }
}
