using UnityEngine;

/// <summary>
/// 在动画 IK 阶段压低脊椎。注意：在 Update 里改骨骼无效，
/// 动画系统会在 Update 之后把整副骨架的姿势重写一遍。
/// 必须和 Animator 挂在同一个物体上，并且 Animator 的 Base Layer 要勾 IK Pass。
/// </summary>
namespace AdventureSample.Player
{
    public class SpineAdjuster : MonoBehaviour
    {
        [Header("压低脊椎的角度")]
        [SerializeField] private float _downAngle = 15f;
        
        [Header("压低脊椎的骨骼")]
        [SerializeField] private Transform _spineBone;

        [Header("跟随相机俯仰角度")]
        [SerializeField] private float pitchClamp = 25f;   // 最多跟随 ±25°

        [SerializeField] private float transitionSpeed = 8f; // 平滑过渡速度
        private float _currentPitch;

        private float _currentDownAngle = 0f; // 当前实际角度
        private float _targetAngle = 0f;  // 目标角度
        private Animator _animator;
        private bool _down;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogWarning("[SpineAdjuster] 需要和 Animator 挂在同一个物体上", this);
                enabled = false;
                return;
            }

            if (_spineBone == null) _spineBone = _animator.GetBoneTransform(HumanBodyBones.Spine);
            if (_spineBone == null) Debug.LogWarning("[SpineAdjuster] 找不到 Spine 骨骼", this);
        }

        private void Update()
        {
            // 2. Mathf.Lerp(a, b, t) 在 a 和 b 之间插值，t 越大越接近 b
            _currentDownAngle = Mathf.Lerp(_currentDownAngle, _targetAngle, Time.deltaTime * transitionSpeed);

            // 瞄准时，把相机的俯仰角映射到脊椎上
            float targetPitch = 0f;
            if (_down && Camera.main != null)
            {
                float camPitch = Camera.main.transform.eulerAngles.x;
                if (camPitch > 180f) camPitch -= 360f;                     // 350° → -10°
                targetPitch = Mathf.Clamp(camPitch, -pitchClamp, pitchClamp);
            }
            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, Time.deltaTime * transitionSpeed);

            // 用 Unity 自带的近似判断，如果极其接近，直接 Snap（吸附）到目标点
            if (Mathf.Approximately(_currentDownAngle, _targetAngle))
            {
                _currentDownAngle = _targetAngle; 
            }
        }

        public void SetDown(bool down) 
        { 
            _down = down; 
            // 1. 如果 down 为 true，目标角度就是 _downAngle，否则就是 0
            _targetAngle = down ? _downAngle : 0f; 
        }

        private void OnAnimatorIK(int layerIndex)   // layerIndex 0 = Base Layer
        {
            if (_spineBone == null) return;

            // 如果当前角度极小，说明已经完全回正了，可以不用管了
            if (!_down && _currentDownAngle < 0.01f && Mathf.Abs(_currentPitch) < 0.01f) return;

            Quaternion animated = _spineBone.localRotation;   // 这一帧动画算出的姿势

            // 加上我们平滑过渡后的当前角度
            Quaternion offset = Quaternion.Euler(_currentDownAngle + _currentPitch, 0f, 0f);

            // 先动画算出的姿势，再加上我们想要的偏移
            _animator.SetBoneLocalRotation(HumanBodyBones.Spine,animated * offset); 
        }
    }
}