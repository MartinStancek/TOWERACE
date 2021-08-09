using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputImage : MonoBehaviour
{
    public int playerIndex = -1;
    public InputType inputType;
    private Image image;
    private SpriteRenderer renderrer;

    private void Awake()
    {
        image = GetComponent<Image>();
        renderrer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (image != null)
        {
            image.sprite = ControlsManager.Instance.GetSprite(playerIndex, inputType);
        }

        if(renderrer != null)
        {
            renderrer.sprite = ControlsManager.Instance.GetSprite(playerIndex, inputType);

        }
    }
}
