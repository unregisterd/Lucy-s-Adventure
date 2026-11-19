using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using AdventureSample.Player;

public class InventoryPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;   // 整个提示面板
    [SerializeField] private TextMeshProUGUI backpackText;   // 拖一个 Text 进来
    [SerializeField] private TextMeshProUGUI promptText;     // 拖一个 Text 进来
    [SerializeField] private ItemDatabase database;

    [SerializeField] private WeaponController weaponController;

    private void OnEnable()
    {
        // TODO 4：订阅 GameEvents.ItemAdded。
        //         接口：EventManager<string>.StartListening(事件名, 方法名);
        //         方法名就是下面那个 OnItemAdded。
        EventManager<string>.StartListening(GameEvents.ItemAdded, OnItemAdded);
        EventManager<string>.StartListening(GameEvents.InArmedState, ShowBullets);
        EventManager<string>.StartListening(GameEvents.FireWeapon, ShowBullets);
        EventManager<string>.StartListening(GameEvents.ReloadBullets, ShowBullets);
    }

    private void OnDisable()
    {
        // TODO 5：退订，接口同理，换成 StopListening。
        EventManager<string>.StopListening(GameEvents.ItemAdded, OnItemAdded);
        EventManager<string>.StopListening(GameEvents.InArmedState, ShowBullets);
        EventManager<string>.StopListening(GameEvents.FireWeapon, ShowBullets);
        EventManager<string>.StopListening(GameEvents.ReloadBullets, ShowBullets);
    }

    private void Awake()
    {
        promptRoot.SetActive(true);
    }

    private void OnItemAdded(string itemId)
    {
        // TODO D：database.Find(itemId) 拿到定义
        var def = database.Find(itemId);
        // TODO E：显示 "背包：" + displayName；定义为空时退回显示 itemId
        if (def == null) return;
        backpackText.text = "背包：" + def.displayName + "*" + def.itemNumber.ToString();

        // 启动协程，4秒后隐藏文字
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(4.0f);
        backpackText.text = "";
    }
    
    // 实时更新子弹
    private void ShowBullets(string itemId)
    {
        var def = database.Find(itemId);
        if (def == null) return;
        promptText.text = def.displayName + " : " + weaponController.CurrentBullets.ToString() + "/10";
    }
}