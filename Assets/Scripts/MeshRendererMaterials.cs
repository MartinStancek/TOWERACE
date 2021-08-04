using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshRendererMaterials
{
    public MeshRenderer renderer;
    public List<int> materialIndexes;

    public void SetColor(Color color)
    {
        foreach (var i in materialIndexes)
        {
            var matA = renderer.materials[i].color.a;
            renderer.materials[i].color = new Color(color.r, color.g, color.b, matA);
        }
    }
}
