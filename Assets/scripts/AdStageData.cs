using UnityEngine;

[System.Serializable]
public class AdStageData
{
    public string stageName;
    public Sprite backgroundImage;

    [Header("Player Sprites")]
    public Sprite playerWalkFrame1;
    public Sprite playerWalkFrame2;

    [Header("Food / Item Spawner Prefabs")]
    public GameObject[] stageItemPrefabs; // e.g. Burger/Fries for McD, Shoes for Nike
}