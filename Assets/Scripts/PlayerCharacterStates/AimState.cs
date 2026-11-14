using UnityEngine;
using UnityEngine.InputSystem;

namespace AdventureSample.Player
{
    public class AimState : ICharacterState
    {
        private CharacterStateMachine _machine;

        public AimState(CharacterStateMachine machine) { _machine = machine; }

        public void OnEnter()
        {
            _machine.Context.Animator.SetBool("IsAiming", true);
            if (_machine.Context.LookAt != null) _machine.Context.LookAt.SetLooking(false); // 瞄准时禁用3米视线，交给摄像机
        }

        public void OnUpdate()
        {
            // 松开右键 -> 退回持枪
            if (Mouse.current != null && !Mouse.current.rightButton.isPressed)
            {
                _machine.ChangeState(_machine.Armed);
                return;
            }

            // 瞄准时的身体旋转
            if (Camera.main != null)
            {
                float cameraYaw = Camera.main.transform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler(0, cameraYaw, 0);
                _machine.Context.Transform.rotation = Quaternion.Slerp(_machine.Context.Transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        public void OnExit()
        {
            _machine.Context.Animator.SetBool("IsAiming", false);
        }
    }
}