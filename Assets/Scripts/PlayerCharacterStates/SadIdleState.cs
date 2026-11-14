namespace AdventureSample.Player
{
    public class SadIdleState : ICharacterState
    {
        private readonly CharacterStateMachine _machine;
        private readonly CharacterStateContext _ctx;

        public SadIdleState(CharacterStateMachine machine)
        {
            _machine = machine;
            _ctx = machine.Context;
        }

        public void OnEnter()
        {
            _ctx.Animator.SetFloat("Speed", 0f);
            _ctx.Animator.SetBool("SadIdle", true);
            _ctx.LookAt.SetLooking(false);
        }

        public void OnUpdate()
        {
            _ctx.Animator.SetFloat("Speed", 0f);

            bool interrupted = _ctx.Input.move.sqrMagnitude > 0.01f || _ctx.Input.jump;

            if (interrupted)
            {
                _machine.ChangeState(_machine.Normal);
            }
        }

        public void OnExit()
        {
            _ctx.Animator.SetBool("SadIdle", false);
        }
    }
}
