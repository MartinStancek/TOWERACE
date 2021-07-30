using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSnap : MonoBehaviour
{
    public bool isOccupied = false;

    public List<MeshRendererMaterials> coloredParts;

    public Transform buyPanel;

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

        buyPanel.LookAt(GameController.Instance.mapCamera.transform);
        buyPanel.gameObject.SetActive(false);
    }


    public void ResetColor()
    {
        var j = 0;
        foreach (var cp in coloredParts)
        {
            foreach (var i in cp.materialIndexes)
            {
                cp.renderer.materials[i].color = originalcolors[j++];
            }
        }
    }

    public void SetColor(Color c)
    {
        var j = 0;
        foreach(var cp in coloredParts)
        {
            foreach(var i in cp.materialIndexes)
            {
                cp.renderer.materials[i].color = c;
            }
        }
    }
}
