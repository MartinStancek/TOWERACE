using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TowerData
{
    public GameObject prefab;
    public int price;
    public Sprite preview;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TowerOptions", order = 1)]
public class TowerOptions : ScriptableObject
{
    public List<TowerData> data;
}
