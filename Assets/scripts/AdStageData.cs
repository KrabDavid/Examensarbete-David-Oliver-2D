using UnityEngine;

[System.Serializable]
public class AdStageData
{
    public string stageName;
    public GameObject backgroundObject;
    public Color counterColor = Color.white; // Choose the color per stage in the Inspector

    [Header("Player Sprites")]
    public Sprite playerWalkFrame1;
    public Sprite playerWalkFrame2;

    [Header("Food / Item Spawner Prefabs")]
    public GameObject[] stageItemPrefabs;
}