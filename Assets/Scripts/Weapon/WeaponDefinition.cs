using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WeaponDefinition.cs —— 枪械的配置，继承自 ItemDefinition.cs
[CreateAssetMenu(menuName = "AdventureSample/Weapon Definition", fileName = "Weapon_")]
public class WeaponDefinition : ItemDefinition
{
    [Tooltip("生成的枪械 Prefab")]
    public GameObject weaponPrefab;

    [Tooltip("枪口位置")]
    public Transform muzzlePoint;
    
    [Header("子弹参数")]
    public string bulletsId = "bullets"; // 子弹的 Id
    [Tooltip("弹匣容量")]
    public int bulletsMag = 10;

    [Header("射击参数")]
    public float shootRange = 100;
    [Tooltip("时间间隔")]
    public float fireRate = 1f;

    [Header("动作配置")]
    public AnimatorOverrideController animatorOverride;

    [Header("射击反馈")]
    public float impulseTime = 0.35f;

}
