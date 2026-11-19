using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;

    private void OnTriggerEnter(Collider other)
    {
        // TODO 1：判断进来的 other 是不是玩家。玩家 tag 是 "Player"。
        //         不是玩家就直接 return。
        if (!other.CompareTag("Player")) return;

        
        ItemInventory.Instance.Add(item.itemId, item.itemNumber);

        // TODO 2：发布完之后销毁自己（这个物品被捡走了）。
        //         用 Destroy(gameObject);
        Destroy(gameObject);
    }
}