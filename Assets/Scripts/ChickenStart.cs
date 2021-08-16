using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenStart : MonoBehaviour
{
    private int carPassed = 0;

    public Transform barParent;

    private Transform targetBar;

    public Color passColor;
    private Color originalcolor;

    private void Start()
    {
        GameController.Instance.onStartGame.AddListener(StartGameInit);
        GameController.Instance.onEndRace.AddListener(RoundRestart);
    }

    private void StartGameInit()
    {

        targetBar = barParent.Find(""+GameController.Instance.players.Count+"Bar");
        targetBar.gameObject.SetActive(true);

        originalcolor = targetBar.GetChild(0).GetComponent<MeshRenderer>().material.color;
    }

    private void RoundRestart()
    {
        foreach(Transform t in targetBar)
        {
            t.GetComponent<MeshRenderer>().material.color = originalcolor;
        }
        carPassed = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponent<Rigidbody>();
        //Debug.Log("BoostPad trigger: " + other.transform.name);

        if (t && t.tag.Equals("CarSphere"))
        {
            targetBar.GetChild(carPassed).GetComponent<MeshRenderer>().material.color = passColor;
            carPassed++;
            if(carPassed > GameController.Instance.players.Count)
            {
                RoundRestart();
            }
            if (carPassed == GameController.Instance.players.Count)
            {
                var cc = t.GetComponent<CarSphere>().carObject.GetComponent<CarController>();
                cc.SetChickenSkin();
            }
        }
    }
}
