using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包系统。管物品增删查
/// </summary>
public class ItemInventory : MonoBehaviour
{
    public static ItemInventory Instance { get; private set; }

    private Dictionary<string, int> items = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

   
    // ---- 增删查 ----

    public void Add(string itemId, int count = 1)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return;

        if (items.ContainsKey(itemId))
            items[itemId] += count;
        else
            items[itemId] = count;

        EventManager<string>.TriggerEvent(GameEvents.ItemAdded, itemId);
    }

    public bool Remove(string itemId, int count = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (!items.ContainsKey(itemId)) return false;
        if (items[itemId] < count) return false;

        items[itemId] -= count;
        if (items[itemId] <= 0)
            items.Remove(itemId);

        EventManager<string>.TriggerEvent(GameEvents.ItemRemoved, itemId);
        return true;
    }

    public bool Has(string itemId)
    {
        return items.ContainsKey(itemId) && items[itemId] > 0;
    }

    public int GetCount(string itemId)
    {
        return items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public Dictionary<string, int> GetAllItems()
    {
        return items;
    }

}