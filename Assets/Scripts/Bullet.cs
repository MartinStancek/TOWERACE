using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool isDestroing = false;

    public float destroyAfter = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet hit: " + collision.transform.name);
        if (!isDestroing)
        {
            StartCoroutine(DestroyAfter(destroyAfter));
        }
        isDestroing = true;
    }

    private IEnumerator DestroyAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}
