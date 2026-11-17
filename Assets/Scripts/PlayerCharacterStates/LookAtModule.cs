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
        [SerializeField] private float stickiness = 0.15f; // 老目标每轮多得这么多分，避免来回换

        private Transform _bestTarget;
        private bool _isLooking = true;
        private float _timer = 0f;

        public void SetLooking(bool value)
        {
            _isLooking = value;
        }

        private void Awake()
        {
            if (headAimConstraint == null)
            {
                Debug.LogError($"{nameof(LookAtModule)}: headAimConstraint 没指定", this);
                return;
            }

            // 情况 1：Inspector 已经拖了 _dynamicTarget，什么都不用做
            if (_dynamicTarget != null) return;

            // 情况 2：没拖，但约束已经在看某个物体 —— 复用它，
            //         这样"改位置"和"约束看谁"是同一个对象
            var sources = headAimConstraint.data.sourceObjects;
            if (sources.Count > 0)
            {
                _dynamicTarget = sources[0].transform;
                return;
            }
            // 情况 3：两边都空 —— 运行时建一个，并挂进约束
            var go = new GameObject($"{name}_LookAtTarget");
            _dynamicTarget = go.transform;
            sources.Add(new WeightedTransform(_dynamicTarget, 1f));
            headAimConstraint.data.sourceObjects = sources;
        }
        
        private void Update()
        {
            if (headAimConstraint == null) return;

            if (!_isLooking) {                    
                headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, 0f, Time.deltaTime * followSpeed);
                return;
            }

            // 1. 定时检测，寻找最佳目标
            _timer += Time.deltaTime;
            if (_timer >= checkInterval)
            {
                _timer = 0f;
                FindBestTarget();
            }

            // 2. 平滑控制 IK 权重
            float targetWeight = (_isLooking && _bestTarget != null && _dynamicTarget != null) ? 1f : 0f;
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, targetWeight, Time.deltaTime * followSpeed);

            if (_bestTarget != null && _dynamicTarget != null)
            {
                _dynamicTarget.position = Vector3.Lerp(
                _dynamicTarget.position,
                _bestTarget.position,
                Time.deltaTime * followSpeed);
            }
        }

        private void FindBestTarget()
        {
            Transform previous = _bestTarget;   // 记住上一轮看的是谁
            _bestTarget = null;
            float bestScore = -1f;

            // 获取 3 米内的所有目标
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDistance, targetLayer);

            foreach (var col in hitColliders)
            { 
                Transform target = col.transform;
                if (target == transform || target.IsChildOf(transform)) continue;
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

                if (target == previous) score += stickiness;   // 老目标多得一点分

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
