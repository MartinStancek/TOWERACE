using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public Color playerColor;

    public CinemachineVirtualCamera vcam;
    public CarController car;

    public int startMoney = 100;
    public int moneyByRound = 100;
    public float scoreMultilier = 30;
    private int _money = 0;
    public int money
    {
        set { 
            _money = value;
            moneyVisual.text = "" + value + "$";
        }
        get { return _money; }
    }

    public TMP_Text moneyVisual;
}
