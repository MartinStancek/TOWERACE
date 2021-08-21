using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerOutline : MonoBehaviour
{
    public Transform readyPanel;
    public List<Transform> readies;
    public List<Transform> notReadies;
    public Transform positionPanel;
    public TMP_Text positionPlayer;
    public TMP_Text positionCount;

    public List<RectTransform> speedBars;
    public float maxSpeedBarHeight = 256f;

    private void Start()
    {
        SetSpeedBar(1f);
    }

    public void SetSpeedBar(float speed)
    {
        speed = Mathf.Clamp(speed, 0f, 1f);
        foreach(RectTransform bar in speedBars)
        {
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, speed * maxSpeedBarHeight);
            var img = bar.GetComponent<Image>();
            img.enabled = false;
            img.enabled = true;
        }
    }

    public void SetReady(bool value)
    {
        foreach (var ready in readies)
        {
            ready.gameObject.SetActive(value);
        }
        foreach (var notReady in notReadies)
        {
            notReady.gameObject.SetActive(!value);
        }
    }
}
