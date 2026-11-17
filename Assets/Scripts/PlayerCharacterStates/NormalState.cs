using UnityEngine;
using UnityEngine.InputSystem;

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
            // 确保关闭脊椎压低
            _ctx.SpineAdjuster.SetDown(false);
            
            _idleTimer = 0f;
            _ctx.LookAt.SetLooking(true);
        }

        public void OnUpdate()
        {
            // 按 M 键拔枪
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                _machine.ChangeState(_machine.Armed);
            }
            
            bool isMoving = _ctx.Input.move.sqrMagnitude > 0.01f;
            bool isGrounded = _ctx.Controller.Grounded;
            
            if (!isMoving && isGrounded)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= IdleThreshold)
                {
                    _machine.ChangeState(_machine.SadIdle);
                }

            }
            else
            {
                _idleTimer = 0f;
            }
        }

        public void OnExit()
        {
        }
    }
}
