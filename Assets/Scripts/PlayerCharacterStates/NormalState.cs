using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AdventureSample.Player
{
    public class NormalState : ICharacterState
    {
        private readonly CharacterStateMachine _machine;
        private readonly CharacterStateContext _ctx;
        private float _idleTimer;
        private const float IdleThreshold = 4f;

        public NormalState(CharacterStateMachine machine)
        {
            _machine = machine;
            _ctx = machine.Context;
        }

        public void OnEnter()
        {
            // 确保关闭所有动画层
            _ctx.Animator.SetBool("IsArmed", false);
            _ctx.Animator.SetBool("IsAiming", false);
            // 确保关闭武器显示
            _ctx.WeaponController.HideWeapon();
            // 确保关闭脊椎不压低
            _ctx.SpineAdjuster.SetDown(false, 0);
            
            _idleTimer = 0f;
            _ctx.LookAt.SetLooking(false);
        }

        public void OnUpdate()
        {
            // 按 M 键拔手枪
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame && ItemInventory.Instance.Has(WeaponNames.Pistol))
            {
                EventManager<string>.TriggerEvent(GameEvents.WeaponlArmed, WeaponNames.Pistol);
                ChangeAnimation();
            }

            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame && ItemInventory.Instance.Has(WeaponNames.Refle))
            {
                EventManager<string>.TriggerEvent(GameEvents.WeaponlArmed, WeaponNames.Refle);
                ChangeAnimation();
            }
            
            
            bool isMoving = _ctx.Input.move.sqrMagnitude > 0.01f;
            bool isGrounded = _ctx.Controller.Grounded;
            
            if (!isMoving && isGrounded)
            {
                _ctx.LookAt.SetLooking(true);
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= IdleThreshold)
                {
                    _machine.ChangeState(_machine.SadIdle);
                }

            }
            else
            {
                _ctx.LookAt.SetLooking(false);
                _idleTimer = 0f;
            }
        }

        private void ChangeAnimation()
        {
            _ctx.Animator.runtimeAnimatorController = _ctx.WeaponController.OverrideController;
            _machine.ChangeState(_machine.Armed); 
        }

        public void OnExit()
        {
        }
    }
}
