using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public GameObject head;

    public GameObject bulltePrefab;
    public Transform bulletSpawn;

    public float bulletSpeed = 10f;

    public float shootEvery = 1f;
    private float lastShot = 0f;

    public int playerOwner = 0;

    public List<MeshRenderer> coloredParts = new List<MeshRenderer>();

    private List<Rigidbody> targets = new List<Rigidbody>();

    void FixedUpdate()
    {
        if (targets.Count > 0)
        {
            head.transform.LookAt(targets[0].transform);

            if(Time.time - lastShot > shootEvery)
            {
                lastShot = Time.time;

                var bullet = Instantiate(bulltePrefab, bulletSpawn.position, bulletSpawn.rotation);
                var bRB = bullet.GetComponent<Rigidbody>();
                bRB.velocity = bulletSpeed * bullet.transform.forward;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        Debug.Log("Tower trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere") )
        {
            var cs = t.GetComponent<CarSphere>().carObject;
            var p = cs.GetComponentInParent<Player>();
            if (p.playerIndex != playerOwner)
            {
                targets.Add(t);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        Debug.Log("Tower trigger exit: " + other.transform.name);
        if (t && t.tag.Equals("CarSphere") && targets.Contains(t))
        {
            targets.Remove(t);
        }
    }
}
