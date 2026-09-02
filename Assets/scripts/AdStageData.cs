using UnityEngine;

[System.Serializable]
public class AdStageData
{
    public string stageName;
    public GameObject backgroundObject; // Drag bkMcDonalds, Nike, or Elgiganten here

    [Header("Player Sprites")]
    public Sprite playerWalkFrame1;
    public Sprite playerWalkFrame2;

    [Header("Food / Item Spawner Prefabs")]
    public GameObject[] stageItemPrefabs;
}