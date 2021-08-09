using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBullet : MonoBehaviour
{
    public CarSphere target;
    public float changeOnZ = 20f;

    public float maxSpeed = 100f;
    public float speedAcceleration = 0.1f;
    private float speed = 0f;
    public float rotationSpeed = 0.1f;

    private bool targetPlayer;

    public Transform visual;
    public Transform rayOrigin;
    public float rayDistance = 0.5f;

    public float radius = 10f;
    public float hitIntensity = 100f;

    private void Update()
    {
        speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * speedAcceleration);
        if (targetPlayer)
        {
            var originalRotation = visual.rotation;
            visual.LookAt(target.transform);
            var toRotation = visual.rotation;

            visual.rotation = Quaternion.Lerp(originalRotation, toRotation, rotationSpeed * Time.deltaTime);
            visual.position += visual.forward * speed * Time.deltaTime;
        } 
        else
        {
            visual.position += visual.forward * speed * Time.deltaTime;
            if (visual.localPosition.z > changeOnZ - 2)
            {
                targetPlayer = true;
            }
        }
        Debug.DrawRay(rayOrigin.position, visual.forward, Color.red, rayDistance);
        if (Physics.Raycast(rayOrigin.position, visual.forward, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("triggered By: " + hit.transform.name);
            var inRadius = Physics.OverlapSphere(hit.point, radius);
            foreach(var obj in inRadius)
            {
                Debug.Log("Object In Radius: " + obj.name);
                if (obj.CompareTag("CarSphere"))
                {
                    var dir = (hit.point - obj.transform.position).normalized * hitIntensity;
                    Debug.Log("Pushing car in " + dir);

                    obj.GetComponent<Rigidbody>().AddForce(dir);
                }
            }
            Destroy(gameObject);
        }
    }
}
