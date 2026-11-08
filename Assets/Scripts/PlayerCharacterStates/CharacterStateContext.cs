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
        public ThirdPersonController Controller { get; private set; }
        public StarterAssetsInputs Input { get; private set; }
        public LookAtModule LookAt { get;}
        
        public CharacterStateContext(Animator Animator, ThirdPersonController Controller, StarterAssetsInputs Input, LookAtModule LookAt)
        {
            this.Animator = Animator;
            this.Controller = Controller;
            this.Input = Input;
            this.LookAt = LookAt;
        }

    }
}
