using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using StarterAssets;

/// <summary
/// 角色状态上下文类，负责存储角色状态机所需的上下文信息（纯数据类，只管存储数据，不管具体的状态逻辑）
/// </summary>

namespace AdventureSample.Player
{
    public class CharacterStateContext
    {
        public Animator Animator { get; private set; }
        public Transform Transform { get; private set; }
        public ThirdPersonController Controller { get; private set; }
        public StarterAssetsInputs Input { get; private set; }
        public LookAtModule LookAt { get;}
        public WeaponController WeaponController { get; set; }
        public GameObject CrosshairUI { get; private set; } // 准星UI
        public CameraModule CameraModule { get; private set; }
        public SpineAdjuster SpineAdjuster { get; private set; } // 脊椎模块，用于瞄准时的骨骼旋转

        public CharacterStateContext(Animator Animator, Transform Transform, ThirdPersonController Controller, StarterAssetsInputs Input, LookAtModule LookAt, WeaponController WeaponController, GameObject CrosshairUI, CameraModule CameraModule, SpineAdjuster SpineAdjuster)
        {
            this.Animator = Animator;
            this.Transform = Transform;
            this.Controller = Controller;
            this.Input = Input;
            this.LookAt = LookAt;
            this.WeaponController = WeaponController;
            this.CrosshairUI = CrosshairUI;
            this.CameraModule = CameraModule;
            this.SpineAdjuster = SpineAdjuster;
        }

    }
}
