using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshRendererMaterials
{
    public ParticleSystem particles;
    public MeshRenderer renderer;
    public List<int> materialIndexes;

    public void SetColor(Color color)
    {
        if (renderer != null)
        {
            foreach (var i in materialIndexes)
            {
                var matA = renderer.materials[i].color.a;
                renderer.materials[i].color = new Color(color.r, color.g, color.b, matA);
            }
        }
        
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = color;
        }

    }
}
