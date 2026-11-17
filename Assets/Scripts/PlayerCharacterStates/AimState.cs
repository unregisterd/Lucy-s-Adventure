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
            _machine.Context.CrosshairUI.SetActive(true); // 开瞄准，显示准星
            _machine.Context.CameraModule.SetAimView(true); // 开瞄准，切换越肩视角
            _machine.Context.SpineAdjuster.SetDown(true); // 瞄准时调整脊椎

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

            // 单发：按下的这一帧开一枪，然后把标记吃掉，按住不会连发
            if (_machine.Context.Input.shoot)
            {
                _machine.Context.Input.ShootInput(false);
                _machine.Context.WeaponController.Shoot();
                _machine.Context.Animator.SetTrigger("Shoot");
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
            _machine.Context.CrosshairUI.SetActive(false); // 退出瞄准，隐藏准星
            _machine.Context.CameraModule.SetAimView(false); // 退出瞄准，切换回正常视角
        }
    }
}