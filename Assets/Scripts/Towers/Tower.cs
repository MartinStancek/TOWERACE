using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public int playerOwner = 0;

    public float shootEvery = 1f;
    protected float lastShot = 0f;

    public List<MeshRendererMaterials> coloredParts = new List<MeshRendererMaterials>();

    protected List<Rigidbody> targets = new List<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        Debug.Log("Tower trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere"))
        {
            var cs = t.GetComponent<CarSphere>().carObject;
            var p = cs.GetComponentInParent<Player>();
            if (p.playerIndex != playerOwner)
            {
                targets.Add(t);
                OnEnemyEnter(t);
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
            OnEnemyLeaves(t);
        }
    }

    public virtual void OnEnemyEnter(Rigidbody target)
    {

    }
    public virtual void OnEnemyLeaves(Rigidbody target)
    {

    }

}
