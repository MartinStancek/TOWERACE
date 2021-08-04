using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTower : Tower
{
    public GameObject head;

    public GameObject bulltePrefab;
    public Transform bulletSpawn;

    public float bulletSpeed = 10f;

    void FixedUpdate()
    {
        if (targets.Count > 0)
        {
            head.transform.LookAt(targets[0].GetComponent<CarSphere>().shootTarget);

            if(Time.time - lastShot > shootEvery)
            {
                lastShot = Time.time;

                var bullet = Instantiate(bulltePrefab, bulletSpawn.position, bulletSpawn.rotation);
                var bRB = bullet.GetComponent<Rigidbody>();
                bRB.velocity = bulletSpeed * bullet.transform.forward;
            }
        }
    }
}
