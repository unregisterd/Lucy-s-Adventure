using UnityEngine;

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
            _idleTimer = 0f;
            _ctx.LookAt.SetLooking(true);
        }

        public void OnUpdate()
        {
            bool isMoving = _ctx.Input.move.sqrMagnitude > 0.01f;
            bool isGrounded = _ctx.Controller.Grounded;
            
            if (!isMoving && isGrounded)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= IdleThreshold)
                {
                    _machine.ChangeState(new SadIdleState(_machine));
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
