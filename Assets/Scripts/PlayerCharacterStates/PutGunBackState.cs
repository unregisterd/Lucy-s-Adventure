using UnityEngine;

namespace AdventureSample.Player
{
    public class PutGunBackState : ICharacterState
    {
        private readonly CharacterStateMachine _machine;

        // 这两个是「契约」
        private const int UpperBodyLayer = 1; // 动画层索引
        private const string PutGunBackStateName = "PutBackGun";

        public PutGunBackState(CharacterStateMachine machine) { _machine = machine; }

        public void OnEnter()
        {
            _machine.Context.Animator.SetTrigger("PutGunBack");
            _machine.Context.SpineAdjuster.SetDown(true);
        }

        public void OnUpdate()
        {
            var animator = _machine.Context.Animator;

            if (animator.IsInTransition(UpperBodyLayer)) return;   // 混合中，读到的不是最终状态

            var info = animator.GetCurrentAnimatorStateInfo(UpperBodyLayer);
            if (!info.IsName(PutGunBackStateName)) return;    // 还没真正进到收枪动画
            if (info.normalizedTime >= 1f)                    // 播到 100%
            {
                _machine.Context.Animator.SetBool("IsArmed", false);
                _machine.ChangeState(_machine.Normal);
            }
        }

        public void OnExit()
        {
            _machine.Context.WeaponController.HideWeapon();
            _machine.Context.SpineAdjuster.SetDown(false);
        }
    }
}