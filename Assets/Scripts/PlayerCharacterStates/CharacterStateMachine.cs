using System;
using UnityEngine;
using StarterAssets;

/// <summary>
/// 角色状态机类，负责管理角色的状态切换和状态更新（纯类，只管切换状态，不管具体的状态逻辑）
/// </summry>
namespace AdventureSample.Player
{
    public class CharacterStateMachine
    {
        public ICharacterState CurrentState { get; private set; }
        public ICharacterState PreviousState { get; private set; }
        public CharacterStateContext Context { get; }

        // 五个状态各建一份，之后反复复用
        public ICharacterState Normal { get; }
        public ICharacterState Armed { get; }
        public ICharacterState Aim { get; }
        public ICharacterState SadIdle { get; }
        public ICharacterState PutGunBack { get; }

        public CharacterStateMachine(CharacterStateContext context)
        {
            Context = context;
            Normal = new NormalState(this);
            Armed = new ArmedState(this);
            Aim = new AimState(this);
            SadIdle = new SadIdleState(this);
            PutGunBack = new PutGunBackState(this);
        }

        // 当事件发生时，StateChanged事件将被触发，并传递前一个状态和当前状态作为参数
        public event Action<ICharacterState, ICharacterState> StateChanged;

        public void Tick() => CurrentState?.OnUpdate();

        public void ChangeState(ICharacterState newState)
        {
            if (newState == null || newState == CurrentState)
                return;

            PreviousState = CurrentState;
            CurrentState?.OnExit();

            CurrentState = newState;
            CurrentState.OnEnter();

            StateChanged?.Invoke(PreviousState, CurrentState);
        }
    }
}
