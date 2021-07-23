using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraMovement : MonoBehaviour
{
    public Transform targetCameraPoint;

    public float offset = 15f;

    /*[SerializeField]*/
    private float speed = 2f;
    /*[SerializeField]*/ private float maximumOffset = 0.1f;


    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        var camera = Physics.OverlapSphere(targetCameraPoint.position + Vector3.back * offset, maximumOffset).Where(e => e.gameObject.Equals(Camera.main.gameObject)).FirstOrDefault();
        if (!camera)
        {
            transform.position = Vector3.Lerp(transform.position, targetCameraPoint.position + Vector3.back * offset, Time.deltaTime * speed);
        }
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetCameraPoint.rotation, speed * Time.deltaTime);

    }
}
