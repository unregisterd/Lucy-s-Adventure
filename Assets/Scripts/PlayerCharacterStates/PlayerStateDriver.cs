using UnityEngine;
using UnityEngine.Animations.Rigging;
using StarterAssets;

/// <summary>
/// 角色状态驱动器类，负责驱动角色状态机的更新和状态切换（MonoBehaviour类，挂载在角色身上）
/// </summary>
namespace AdventureSample.Player
{
    public class PlayerStateDriver : MonoBehaviour
    {
        private CharacterStateContext _ctx;
        private CharacterStateMachine _machine;

        private void Awake()
        {
            _ctx = new CharacterStateContext(
                GetComponent<Animator>(),
                GetComponent<ThirdPersonController>(),
                GetComponent<StarterAssetsInputs>(),
                GetComponent<LookAtModule>()
            );
            _machine = new CharacterStateMachine(_ctx);
        }

        private void Start() => _machine.ChangeState(new NormalState(_machine));
        private void Update() => _machine.Tick();
    
    }

}
