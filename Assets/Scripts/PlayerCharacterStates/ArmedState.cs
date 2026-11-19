using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AdventureSample.Player
{
    public class ArmedState : ICharacterState
    {
        private CharacterStateMachine _machine;
        private CharacterStateContext _ctx;

        public ArmedState(CharacterStateMachine machine) 
        { 
            _machine = machine; 
            _ctx = _machine.Context;
        }

        public void OnEnter()
        {
            _ctx.Animator.SetBool("IsArmed", true);
            _ctx.Animator.SetBool("IsAiming", false);
            // _machine.Context.SpineAdjuster.SetDown(true); // 持枪时调整脊椎
            _ctx.WeaponController.ShowWeapon();
            // _machine.Context.CameraModule.SetAimView(false); // 持枪时切换到第三人称视角

            if (_ctx != null) _machine.Context.LookAt.SetLooking(false); // 持枪时禁用3米视线
            EventManager<string>.TriggerEvent(GameEvents.InArmedState, _ctx.WeaponController.BulletsId);

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

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame && _ctx.WeaponController.BulletsLeft > 0 && _ctx.WeaponController.CurrentBullets < _ctx.WeaponController.BulletsMag)
            {
                _ctx.Animator.SetTrigger("Reload");
                _ctx.WeaponController.ReloadBullets();

            }
            
        }

        public void OnExit() 
        {
            _ctx.SpineAdjuster.SetDown(false); // 退出持枪，恢复脊椎
        }
    }
}