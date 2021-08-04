using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaTower : Tower
{
    public GameObject lightingPrefab;
    private List<LineRenderer> lineRenderrers;
    public Transform head;

    public float strengthMultiplier = 0.2f;
    public float backwardMultiplier = 2000f;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderrers = new List<LineRenderer>();
    }

    void Update()
    {
        var i = 0;
        foreach(var t in targets)
        {
            lineRenderrers[i].positionCount = 2;
            lineRenderrers[i].SetPosition(0, head.position);
            lineRenderrers[i].SetPosition(1, t.position);
            i++;
        }

        if (targets.Count > 0 && Time.time - lastShot > shootEvery)
        {
            lastShot = Time.time;
            foreach (var t in targets)
            {
                var dir = Random.insideUnitSphere.normalized * strengthMultiplier;
                dir.y = 0f;
                var cs = t.GetComponent<CarSphere>();
                cs.carObject.transform.Find("race0").localPosition = dir;
                t.AddForce(cs.carObject.transform.forward * -backwardMultiplier);
                Debug.Log("Adding force in direction: " + dir);
            }
        }
    }

    public override void OnEnemyEnter(Rigidbody target)
    {
        var go = Instantiate(lightingPrefab, transform);
        var lr = go.GetComponent<LineRenderer>();
        lr.startColor = Color.red;//GameController.Instance.players[playerOwner].playerColor;
        lineRenderrers.Add(lr);
    }

    public override void OnEnemyLeaves(Rigidbody target)
    {
        Destroy(lineRenderrers[0].gameObject);
        lineRenderrers.RemoveAt(0);
        target.GetComponent<CarSphere>().carObject.transform.Find("race0").localPosition = Vector3.zero;
    }



}
