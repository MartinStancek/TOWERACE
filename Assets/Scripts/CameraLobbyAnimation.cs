using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraLobbyAnimation : MonoBehaviour
{
    private float animationSpeed = 1f;

    private Rect targetRect = Rect.zero;
    private Rect fromRect = Rect.zero;

    private Camera _goCamera = null;
    private Camera goCamera
    {
        get { 
            if(_goCamera == null)
            {
                _goCamera = GetComponent<Camera>();
            }
            return _goCamera; }
    }



    void Update()
    {
        if(targetRect != Rect.zero && fromRect != Rect.zero)
        {
            goCamera.rect = new Rect(Vector2.Lerp(goCamera.rect.position, targetRect.position, animationSpeed * Time.deltaTime),
                Vector2.Lerp(goCamera.rect.size, targetRect.size, animationSpeed * Time.deltaTime));
        }
    }

    public void AnimateTo(Rect targetRect)
    {
        this.targetRect = new Rect(targetRect);
        this.fromRect = new Rect(goCamera.rect);
    }
}
