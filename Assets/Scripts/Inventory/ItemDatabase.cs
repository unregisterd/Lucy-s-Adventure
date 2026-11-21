using UnityEngine;
using System.Collections.Generic;

// ItemDatabase.cs —— 所有物品的总表

[CreateAssetMenu(menuName = "AdventureSample/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new();
    [SerializeField] private List<WeaponDefinition> weaponsDefinition = new();

    // TODO A：运行时用的查找表
    //   字段别用 public Dictionary<...>，想想为什么
    private Dictionary<string, ItemDefinition> lookup;
    private Dictionary<string, WeaponDefinition> weapons;
    
    private void OnEnable(){
        BuildLookup();
    }

    // TODO B：把 items 建成字典，重复 id 要处理（报警告还是覆盖？）
    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemDefinition>();
        weapons = new Dictionary<string, WeaponDefinition>();

        AddItem(lookup, items);
        AddItem(weapons, weaponsDefinition);

    }

    private void AddItem<T>(
        Dictionary<string, T> dictionary,
        List<T> definitions)
        where T : ItemDefinition
    {
        foreach (var item in definitions)
        {
            if (item == null) continue;

            if (string.IsNullOrEmpty(item.itemId))
            {
                Debug.LogWarning($"[ItemDatabase] 物品 id 为空，已跳过：{item.name}");
                continue;
            }

            if (dictionary.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"[ItemDatabase] 物品 id 重复，已覆盖：{item.itemId}");
            }

            dictionary[item.itemId] = item;
        }
}


    // TODO C：public ItemDefinition Find(string itemId)
    //   查不到返回 null，别抛异常
    // 查找普通物品的定义
    public ItemDefinition Find(string itemId){
        if (lookup.TryGetValue(itemId, out var found))
        {
            return found;
        }
        else{
            return null;
        }
    }

    // 查找武器的定义
    public WeaponDefinition FindWeapon(string weaponId)
    {
        if(weapons.TryGetValue(weaponId, out var found))
        {
            return found;
        }
        else
        {
            return null;
        }
    }
}