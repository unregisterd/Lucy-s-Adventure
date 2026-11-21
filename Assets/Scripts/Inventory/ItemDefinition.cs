using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ItemDefinition.cs —— 一个物品的配置
[CreateAssetMenu(menuName = "AdventureSample/Item Definition", fileName = "Item_")]
public class ItemDefinition : ScriptableObject
{
    public string itemId;        // "wood_stick"
    public string displayName;   // "木棍"
    public int itemNumber; // 物品数量
    public Sprite icon;          // 物品图标（先给字段，UI 可以暂不用）
    // rifle setup: trigger domain reload v2
}
