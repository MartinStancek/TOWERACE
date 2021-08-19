using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntimaterTower : Tower
{
    public Transform lasers;

    public float speed = 10f;

    public ParticleSystem destructionEffect;

    void Update()
    {
        lasers.localEulerAngles += new Vector3(0f, speed * Time.deltaTime, 0f);
    }

    public List<Rigidbody> GetTargets()
    {
        return targets;
    }
}
