using UnityEngine;
using UnityEngine.InputSystem; // 必须加上这个才能检测鼠标

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
            _machine.Context.Animator.SetBool("IsArmed", false);
            _machine.Context.Animator.SetBool("IsAiming", false);
            
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
