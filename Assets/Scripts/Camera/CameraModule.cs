using UnityEngine;
using Cinemachine;

namespace AdventureSample.Player
{
    public class CameraModule : MonoBehaviour
    {
        [Header("越肩视角参数")]
        [Tooltip("瞄准时相机到角色的距离。太近会穿进角色模型，1.8~2.2 比较稳")]
        public float aimDistance = 2f;

        [Tooltip("瞄准时的肩部偏移。Cinemachine 会再乘 CameraSide，所以要填得比平时的 ShoulderOffset.x 大")]
        public Vector3 aimOffset = new Vector3(1.6f, 0f, 0f);

        [Header("过渡")]
        [Tooltip("每秒收敛速度：12 约 0.25 秒到位，8 更慢更电影感，16 更快更跟手")]
        public float smoothSpeed = 12f;

        [Tooltip("离目标小于这个值就直接落位，避免永远差一点点")]
        public float snapEpsilon = 0.005f;

        [SerializeField] private CinemachineVirtualCamera vcam;

        private Cinemachine3rdPersonFollow _follow;
        private float _defaultDistance;
        private Vector3 _defaultOffset;
        private float _currentDistance;
        private Vector3 _currentOffset;
        private bool _aiming;

        private void Awake()
        {
            // 默认值必须在 Awake 记下来：状态机是在 Awake 里建起来的，
            // 如果延到 Start 才记，先被 SetAimView(true) 打断就会记成 0
            if (vcam == null)
                vcam = GameObject.Find("PlayerFollowCamera")?.GetComponent<CinemachineVirtualCamera>();

            if (vcam == null)
            {
                Debug.LogWarning("[CameraModule] 找不到 PlayerFollowCamera，越肩视角不会生效", this);
                enabled = false;
                return;
            }

            _follow = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (_follow == null)
            {
                Debug.LogWarning("[CameraModule] 相机上没有 Cinemachine3rdPersonFollow，越肩视角不会生效", this);
                enabled = false;
                return;
            }

            _defaultDistance = _follow.CameraDistance;   // prefab 里是 4
            _defaultOffset = _follow.ShoulderOffset;     // prefab 里是 (1, 0, 0)
            _currentDistance = _defaultDistance;
            _currentOffset = _defaultOffset;
        }

        /// <summary>只记录“想去哪”，真正移动交给 Update 平滑。</summary>
        public void SetAimView(bool isAiming)
        {
            _aiming = isAiming;
        }

        private void Update()
        {
            if (_follow == null) return;

            float targetDistance = _aiming ? aimDistance : _defaultDistance;
            Vector3 targetOffset = _aiming ? aimOffset : _defaultOffset;

            float k = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, k);
            _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, k);

            // 够近了就落位，避免数学上永远到不了
            if (Mathf.Abs(_currentDistance - targetDistance) < snapEpsilon) _currentDistance = targetDistance;
            if ((_currentOffset - targetOffset).sqrMagnitude < snapEpsilon * snapEpsilon) _currentOffset = targetOffset;

            _follow.CameraDistance = _currentDistance;
            _follow.ShoulderOffset = _currentOffset;
        }

        private void OnDisable()
        {
            // 死亡、禁用、切场景时立刻还原，别把镜头留在肩膀上
            _aiming = false;
            if (_follow == null) return;
            _currentDistance = _defaultDistance;
            _currentOffset = _defaultOffset;
            _follow.CameraDistance = _defaultDistance;
            _follow.ShoulderOffset = _defaultOffset;
        }
    }
}