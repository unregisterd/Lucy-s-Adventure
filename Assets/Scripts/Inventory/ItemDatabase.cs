using UnityEngine;
using System.Collections.Generic;

// ItemDatabase.cs —— 所有物品的总表

[CreateAssetMenu(menuName = "AdventureSample/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new();

    // TODO A：运行时用的查找表
    //   字段别用 public Dictionary<...>，想想为什么
    private Dictionary<string, ItemDefinition> lookup;
    
    private void OnEnable(){
        BuildLookup();
    }

    // TODO B：把 items 建成字典，重复 id 要处理（报警告还是覆盖？）
    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemDefinition>();
        foreach (var item in items)
        {
            // 1. item 为 null 就跳过
            if(item == null) continue;
            // 2. item.itemId 为空就跳过，打个警告（配置漏填）
            if(string.IsNullOrEmpty(item.itemId)){
                Debug.LogWarning($"[ItemDatabase] 物品 id 为空，已跳过：{item.name}");
                continue;
            }
            // 3. lookup 里已经有这个 id 就打警告（配置重复），然后继续覆盖
            if(lookup.ContainsKey(item.itemId)){
                Debug.LogWarning($"[ItemDatabase] 物品 id 重复，已覆盖：{item.itemId}");
            }
            lookup[item.itemId] = item;
        }
    }

    // TODO C：public ItemDefinition Find(string itemId)
    //   查不到返回 null，别抛异常
    public ItemDefinition Find(string itemId){
        if(lookup.TryGetValue(itemId, out var found)){
            return found;
        }
        else{
            return null;
        }
    }
}