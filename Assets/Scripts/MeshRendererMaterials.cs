using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshRendererMaterials
{
    public ParticleSystem particles;
    public Renderer renderer;
    public List<int> materialIndexes;
    public string colorField;

    public void SetColor(Color color)
    {
        if (renderer != null)
        {
            foreach (var i in materialIndexes)
            {
                var matA = renderer.materials[i].color.a;
                renderer.materials[i].color = new Color(color.r, color.g, color.b, matA);
            }

            if (!string.IsNullOrEmpty(colorField))
            {
                var matA = renderer.material.GetColor(colorField).a;

                renderer.material.SetColor(colorField, new Color(color.r, color.g, color.b, matA));
            }
        }



        if (particles != null)
        {
            var main = particles.main;
            var matA = main.startColor.color.a;

            main.startColor = new Color(color.r, color.g, color.b, matA);
        }

    }
}
