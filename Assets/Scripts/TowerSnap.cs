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

    public Tower tower = null;
    public List<BoostPad> nearBoostPads;

    public List<InputImage> inputImages;


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
}
