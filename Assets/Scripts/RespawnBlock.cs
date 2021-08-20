using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBlock : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("CarSphere"))
        {
            collision.collider.GetComponent<CheckPointController>().Respawn();
        }
    }
}
