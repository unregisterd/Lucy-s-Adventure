using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace AdventureSample.Player
{
    public class LookAtModule : MonoBehaviour
    {
        [Header("约束设置")]
        [SerializeField] private MultiAimConstraint headAimConstraint;
        [SerializeField] private  float maxDistance = 3f;
        [SerializeField] private float followSpeed = 3f;
        [SerializeField] private LayerMask targetLayer; // 建议新建一个 Target 层，只勾选需要看的物体
        [SerializeField] private Transform _dynamicTarget; // 用于调试，运行时可以在 Inspector 里拖动这个物体，看看头部是否跟随它

        [Header("筛选设置")]
        [SerializeField] private float checkInterval = 0.4f; // 降低检测频率，不用每帧查，节约性能
        [SerializeField] private float angleWeight = 0.5f;   // 角度在评分中的权重

        private Transform _bestTarget;
        private bool _isLooking = true;
        private float _timer = 0f;

        public void SetLooking(bool value)
        {
            _isLooking = value;
        }

        private void Update()
        {
            if (headAimConstraint == null) return;

            // 1. 定时检测，寻找最佳目标
            _timer += Time.deltaTime;
            if (_timer >= checkInterval)
            {
                _timer = 0f;
                FindBestTarget();
            }

            // 2. 平滑控制 IK 权重
            float targetWeight = (_isLooking && _bestTarget != null) ? 1f : 0f;
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, targetWeight, Time.deltaTime * followSpeed);

            // 3. 如果找到了目标，将其赋给约束
            if (_bestTarget != null)
            {
                _dynamicTarget.position = _bestTarget.position;
            }
        }

        private void FindBestTarget()
        {
            _bestTarget = null;
            float bestScore = -1f;

            // 获取 3 米内的所有目标
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, targetLayer);

            foreach (var col in hitColliders)
            {
                Transform target = col.transform;
            
                // 计算距离比例（0到1，越近越大）
                float distance = Vector3.Distance(transform.position, target.position);
                float distanceScore = 1f - (distance / maxDistance);

                // 计算角度比例（0到1，越正对越大）
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dirToTarget); 

                if (dot < 0f) continue; // 如果目标在角色背后，直接跳过

                //float angleScore = (dot + 1f) / 2f; // 把 0~1 映射到 0~1
                float angleScore = dot; // dot 本身就是 0~1 的范围，越正对越大

                // 综合评分（距离为主，角度为辅）
                float score = distanceScore + (angleScore * angleWeight);

                if (score > bestScore)
                {
                    bestScore = score;
                    _bestTarget = target;
                }
            }
        }

        // 在 Scene 视图画个圈，方便调试
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}
