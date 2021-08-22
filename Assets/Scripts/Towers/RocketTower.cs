using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketTower : Tower
{
    public Transform head;
    public float rotationSpeed = 10f;
    List<CarSphere> unshootTargets = new List<CarSphere>();

    public GameObject rocketBulletPrefab;
    public Transform bulletSpawn;

    private void Update()
    {
        if(unshootTargets.Count > 0)
        {
            var rot = head.rotation.eulerAngles;
            rot.y = unshootTargets[0].carObject.transform.rotation.eulerAngles.y + 180;
            var targetRot = Quaternion.Euler(rot);
            head.rotation = Quaternion.Lerp(head.rotation, targetRot, Time.deltaTime * rotationSpeed);
            if (Quaternion.Angle(head.rotation, targetRot) <= 2)
            {
                var rocket = Instantiate(rocketBulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
                rocket.GetComponent<RocketBullet>().target = unshootTargets[0];
                unshootTargets.RemoveAt(0);

                Debug.Log("shoot");
            }
        }
    }
    public override void OnEnemyEnter(Rigidbody target)
    {
        SoundManager.PlaySound(SoundManager.SoundType.TOWER_MISSLE_START);

        unshootTargets.Add(target.GetComponent<CarSphere>());
    }
    public override void OnEnemyLeaves(Rigidbody target)
    {
        var leaveCar = target.GetComponent<CarSphere>();

        if (unshootTargets.Contains(leaveCar))
        {
            unshootTargets.Remove(leaveCar);
        }
    }

}
