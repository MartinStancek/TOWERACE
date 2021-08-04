using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeTower : Tower
{
    public float strengthMultiplier = 10f;
    public float backwardMultiplier = 2000f;

    private void Update()
    {
        if (targets.Count > 0 && Time.time - lastShot > shootEvery)
        {
            lastShot = Time.time;
            foreach (var t in targets)
            {
                var cs = t.GetComponent<CarSphere>();

                t.AddForce((transform.position - t.transform.position).normalized * strengthMultiplier);
                t.AddForce(cs.carObject.transform.forward * -backwardMultiplier);
            }
        }
    }
}
