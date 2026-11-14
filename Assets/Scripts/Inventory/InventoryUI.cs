using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;   // 整个提示面板
    [SerializeField] private TextMeshProUGUI backpackText;   // 拖一个 Text 进来
    [SerializeField] private ItemDatabase database;

    private void OnEnable()
    {
        // TODO 4：订阅 GameEvents.ItemAdded。
        //         接口：EventManager<string>.StartListening(事件名, 方法名);
        //         方法名就是下面那个 OnItemAdded。
        EventManager<string>.StartListening(GameEvents.ItemAdded, OnItemAdded);
    }

    private void OnDisable()
    {
        // TODO 5：退订，接口同理，换成 StopListening。
        EventManager<string>.StopListening(GameEvents.ItemAdded, OnItemAdded);
    }

    private void OnItemAdded(string itemId)
    {
        // TODO D：database.Find(itemId) 拿到定义
        var def = database.Find(itemId);
        // TODO E：显示 "背包：" + displayName；定义为空时退回显示 itemId
        backpackText.text = "背包：" + (def != null ? def.displayName : itemId);
        promptRoot.SetActive(true);
    }
}