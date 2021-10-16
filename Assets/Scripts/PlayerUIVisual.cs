using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIVisual : MonoBehaviour
{
    public TMP_Text playerName;
    public GameObject readyPanel;
    public GameObject notReadyPanel;

    public void SetReady(bool value)
    {

        readyPanel.SetActive(value);
        notReadyPanel.SetActive(!value);
    }
}
