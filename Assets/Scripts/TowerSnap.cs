using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TowerSnap : MonoBehaviour
{
    public bool isOccupied = false;

    public int price = 60;

    public Player playerOwner = null;

    public List<MeshRendererMaterials> coloredParts;

    public Transform buySnapPanel;
    public Transform buyTowerPanel;
    public Transform selectSnapPanel;
    public TMP_Text priceText;

    public Tower tower = null;
    public List<BoostPad> nearBoostPads;


    List<Color> originalcolors = new List<Color>();


    void Start()
    {
        foreach (var cp in coloredParts)
        {
            foreach (var i in cp.materialIndexes)
            {
                originalcolors.Add(cp.renderer.materials[i].color);

            }
        }

        buySnapPanel.LookAt(GameController.Instance.mapCamera.transform);
        buyTowerPanel.LookAt(GameController.Instance.mapCamera.transform);
        selectSnapPanel.LookAt(GameController.Instance.mapCamera.transform);
        SetPanel(null);
        priceText.text = "" + price + "$";
    }


    public void ResetColor()
    {
        var j = 0;
        foreach (var cp in coloredParts)
        {
            cp.SetColor(originalcolors[j++]);
        }
    }

    public void SetColor(Color c)
    {
        var j = 0;
        foreach(var cp in coloredParts)
        {
            cp.SetColor(c);
        }
    }

    public void SetPanel(Transform panel)
    {
        SetPanel(panel, Color.white);
    }
    public void SetPanel(Transform panel, Color color)
    {
        buySnapPanel.gameObject.SetActive(false);
        buyTowerPanel.gameObject.SetActive(false);
        selectSnapPanel.gameObject.SetActive(false);

        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.Find("BackGround").GetComponent<MeshRenderer>().material.color = color;
        }
    }
}
