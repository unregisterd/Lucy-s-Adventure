using UnityEngine;
using Cinemachine;

namespace AdventureSample.Player
{
    public class WeaponController : MonoBehaviour
    {
        [Header("武器设置")]
        [SerializeField] private  GameObject weaponPrefab; // 枪 Prefab 拖进来
        [SerializeField] private Transform weaponSocket;  // WeaponSocket 拖进来
        [SerializeField] private Transform muzzlePoint; // 枪口位置

        [Header("子弹设置")]
        // [SerializeField] private GameObject bulletPrefab; // 子弹 Prefab 拖进来
        // [SerializeField] private Transform bulletSpawnPoint; // 子弹生成点
        // [SerializeField] private float bulletSpeed = 50f; // 子弹速度
        // [SerializeField] private float bulletLifeTime = 1f; // 子弹存活时间
        [SerializeField] private string bulletsId = "bullets";
        [SerializeField] private int  bulletsMag = 10; // 弹匣容量
        [SerializeField] private int bulletsLeft = 0; // 剩余子弹数
        
        
        

        [Header("射击参数")]
        [SerializeField] private float shootRange = 100f;
        [SerializeField] private LayerMask shootLayer;
        [SerializeField] private float fireRate = 0.5f; // 射击间隔

        [Header("射击反馈")]
        [SerializeField] private CinemachineImpulseSource _impulse;

        private Camera _camera;
        private float _nextFireTime = 0f; // 下一次开火时间
        private bool _canShoot = true; // 是否可以开火

        private GameObject _currentWeapon;
        private int _currentBullets = 0; // 当前子弹数量

        public string BulletsId => bulletsId;
        public int BulletsMag => bulletsMag;
        public int BulletsLeft => bulletsLeft;
        public int CurrentBullets => _currentBullets;
        

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

        private void Update()
        {
            _nextFireTime += Time.deltaTime;
            if (_nextFireTime >= fireRate) _canShoot = true;

            // 检查背包，实时更新 bulletsLeft
            if (ItemInventory.Instance.Has(bulletsId))
            {
                bulletsLeft = ItemInventory.Instance.GetCount(bulletsId);
                Debug.Log("<color=yellow>[子弹剩余]</color>" + bulletsLeft);
            }

        }

        public void Shoot()
        {
            if (_camera == null) return;
            
            if(_currentBullets <= 0)
            {
                Debug.Log("<color=yellow>[开火]</color> 没子弹了");
                return;
            }

            _currentBullets --;
            _nextFireTime = 0f;
            _canShoot = false;

            EventManager<string>.TriggerEvent(GameEvents.FireWeapon, bulletsId); // 触发开火事件
            
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
                _impulse?.GenerateImpulse(0.35f);
            }
            else
            {
                Debug.Log("[开火] 打空了");
            }
        }

        public void ReloadBullets()
        {
            int bullets = 0; // 能够装填的子弹数量
            if (bulletsLeft >= bulletsMag - _currentBullets)
            {
                bullets = bulletsMag - _currentBullets; // 装满弹匣
            }
            else
            {
                bullets = bulletsLeft; // 未装满弹匣
            }
            bulletsLeft -= bullets;
            _currentBullets += bullets;

            // 更新背包
            ItemInventory.Instance.Remove(bulletsId, bullets);
            
            // 触发装弹事件
            EventManager<string>.TriggerEvent(GameEvents.ReloadBullets, bulletsId);
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