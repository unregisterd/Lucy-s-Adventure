using UnityEngine;

namespace AdventureSample.Player
{
    public class WeaponController : MonoBehaviour
    {
        [Header("武器设置")]
        public GameObject weaponPrefab; // 枪 Prefab 拖进来
        public Transform weaponSocket;  // WeaponSocket 拖进来
        public Transform muzzlePoint; // 枪口位置

        [Header("射击参数")]
        public float shootRange = 100f;
        public LayerMask shootLayer;

        private Camera _camera;
        private float _nextFireTime;

        private GameObject _currentWeapon;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            if (weaponPrefab != null && weaponSocket != null)
            {
                // 游戏开始时生成枪，并默认隐藏
                _currentWeapon = Instantiate(weaponPrefab, weaponSocket, false);
                HideWeapon(); 
            }
        }

        public void Shoot()
        {
            if (_camera == null) return;
            
            // 1. 相机中心射线：决定「我想打哪」
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint = ray.origin + ray.direction * shootRange;
            Collider aimCollider = null;

            if (Physics.Raycast(ray, out RaycastHit aimHit, shootRange, shootLayer,
                                QueryTriggerInteraction.Ignore))
            {
                aimPoint = aimHit.point;
                aimCollider = aimHit.collider;
            }

            // 2. 枪口 → 目标点：中间有没有东西挡着
            Vector3 muzzle = muzzlePoint != null ? muzzlePoint.position : transform.position;
            float distance = Vector3.Distance(muzzle, aimPoint);

            if (Physics.Linecast(muzzle, aimPoint, out RaycastHit block, shootLayer,
                                QueryTriggerInteraction.Ignore)
                && block.distance < distance - 0.01f)
            {
                // 墙挡在枪口和目标之间 → 实际打在墙上
                Debug.Log($"<color=orange>[被挡]</color> 打在：{block.collider.name}");
                return;
            }

            if (aimCollider != null)
            {
                Debug.Log($"<color=red>[开火]</color> 击中了：{aimCollider.gameObject.name}");
                // 拓展：扣血、弹孔、曳光线从 muzzle 连到 aimPoint
            }
            else
            {
                Debug.Log("[开火] 打空了");
            }
        }

        public void ShowWeapon()
        {
            if (_currentWeapon != null) _currentWeapon.SetActive(true);
        }

        public void HideWeapon()
        {
            if (_currentWeapon != null) _currentWeapon.SetActive(false);
        }
    }
}