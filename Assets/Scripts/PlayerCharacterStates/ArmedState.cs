using UnityEngine;
using UnityEngine.InputSystem;

namespace AdventureSample.Player
{
    public class ArmedState : ICharacterState
    {
        private CharacterStateMachine _machine;

        public ArmedState(CharacterStateMachine machine) { _machine = machine; }

        public void OnEnter()
        {
            _machine.Context.Animator.SetBool("IsArmed", true);
            _machine.Context.Animator.SetBool("IsAiming", false);
            if (_machine.Context.LookAt != null) _machine.Context.LookAt.SetLooking(true); // 持枪时启用3米视线
        }

        public void OnUpdate()
        {
            // 按右键 -> 进入瞄准
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                _machine.ChangeState(_machine.Aim);
                return;
            }

            // 按 M 键 -> 播放收枪动画
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                _machine.ChangeState(_machine.PutGunBack);
            }
        }

        public void OnExit() { }
    }
}