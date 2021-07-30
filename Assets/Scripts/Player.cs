using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public Color playerColor;

    public CinemachineVirtualCamera vcam;
    public CarController car;

    public int startMoney = 100;
    public int moneyByRound = 100;
    public float scoreMultilier = 30;
    public int money = 0;
}
