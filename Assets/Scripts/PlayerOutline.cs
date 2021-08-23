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

    public Transform countDownPanel;
    public TMP_Text countDownText;

    public Transform wingItPanel;

    public List<RectTransform> speedBars;
    public float maxSpeedBarHeight = 256f;

    public List<Transform> wrongWayColoredParts;
    public Transform wrongWayPanel;
    public Color wrongWayColor;

    

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

    public void SetWrongWay(bool value)
    {
        wrongWayPanel.gameObject.SetActive(value);
        Color targetColor;
        if (value)
        {
            targetColor = wrongWayColor;
        }
        else
        {
            targetColor = Color.white;
        }
        foreach(var t in wrongWayColoredParts)
        {
            SetColorRecur(t, targetColor);
        }
        transform.Find("GameObject").GetComponent<Image>().color = GetColorWithA(transform.Find("GameObject").GetComponent<Image>().color, targetColor);
    }

    private void SetColorRecur(Transform tr, Color c)
    {
        var text = tr.GetComponent<TMP_Text>();
        var image = tr.GetComponent<Image>();
        if (text)
        {
            text.color = GetColorWithA(text.color, c);
        }
        if (image)
        {
            image.color = GetColorWithA(image.color, c);
        }

        foreach(Transform t in tr)
        {
            SetColorRecur(t, c);
        }
    }
    private Color GetColorWithA(Color orig, Color target)
    {
        target.a = orig.a;
        return target;
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
