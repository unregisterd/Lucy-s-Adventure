using UnityEngine;
using Cinemachine;

namespace AdventureSample.Player
{
    public class WeaponController : MonoBehaviour
    {
        /// <summary>
        /// 动画表现
        /// </summary>
        private AnimatorOverrideController _overrideController;
        public AnimatorOverrideController OverrideController => _overrideController;

        /// <summary>
        /// 武器设置
        /// </summary>
        [Tooltip("WeaponSocket 拖进来")]
        [SerializeField] private Transform weaponSocket;
        private string _currentWeaponId; // 当前所使用的武器 Id
        private GameObject _weaponPrefab;
        private GameObject _currentWeapon;
        private Transform _muzzlePoint;

        /// <summary>
        /// 子弹设置
        /// </summary>
        private string _currentBulletsId; // 当前武器所使用的子弹Id
        public string BulletsId => _currentBulletsId;
        private int _bulletsMag; // 当前武器的弹匣容量
        public int BulletsMag => _bulletsMag;
        private int _bulletsLeft = 0; // 剩余子弹数
        public int BulletsLeft => _bulletsLeft;
        private int _currentBullets = 0; // 当前子弹数量
        public int CurrentBullets => _currentBullets;
        
        /// <summary>
        /// 射击参数
        /// </summary>
        [SerializeField] private LayerMask shootLayer;
        private float _shootRange;
        private float _fireRate; 

        [Header("射击反馈")]
        [SerializeField] private CinemachineImpulseSource _impulse;
        private float _impulseTime;

        [Tooltip("物品信息总表")]
        [SerializeField] private ItemDatabase itemDatabase;

        private Camera _camera;
        private float _nextFireTime = 0f; // 下一次开火时间
        private bool _canShoot = true; // 是否可以开火

        
        
        
        

        private void OnEnable()
        {
            EventManager<string>.StartListening(GameEvents.WeaponlArmed, OnWeaponArmed);
        }

        private void OnDisable()
        {
            EventManager<string>.StopListening(GameEvents.WeaponlArmed, OnWeaponArmed);
        }

        private void OnWeaponArmed(string weaponId)
        {
            // 查找背包，看看有没有这个武器
            if(!ItemInventory.Instance.Has(weaponId))
            {
                Debug.LogError($"找不到该武器: {weaponId}", this);
                return;
            }

            // 查总表，获取关键信息参数
            var def = itemDatabase.FindWeapon(weaponId);
            if(def == null)
            {
                Debug.LogError($"找不到武器定义：{weaponId}", this);
                return;
            }

            _weaponPrefab = def.weaponPrefab;
            _currentWeapon = Instantiate(_weaponPrefab, weaponSocket, false);
            ShowWeapon();

            _muzzlePoint = def.muzzlePoint;
            _currentWeaponId = weaponId;
            _currentBulletsId = def.bulletsId;
            _bulletsMag = def.bulletsMag;
            _shootRange = def.shootRange;
            _fireRate = def.fireRate;
            _impulseTime = def.impulseTime;
            _overrideController = def.animatorOverride;

            if (ItemInventory.Instance.Has(_currentBulletsId))
            {
                _bulletsLeft = ItemInventory.Instance.GetCount(_currentBulletsId);
            }
                
        }

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            _nextFireTime += Time.deltaTime;
            if (_nextFireTime >= _fireRate) _canShoot = true;

            UpdataLeftBullets();
        }

        // 更新剩余子弹
        public void UpdataLeftBullets()
        {
            // 实时检查剩余的子弹数量
            if (ItemInventory.Instance.Has(_currentBulletsId))
            {
                Debug.Log("背包中有该武器的子弹");
                _bulletsLeft = ItemInventory.Instance.GetCount(_currentBulletsId);
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

            EventManager<string>.TriggerEvent(GameEvents.FireWeapon, _currentBulletsId); // 触发开火事件
            
            // 1. 相机中心射线：决定「我想打哪」
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint = ray.origin + ray.direction * _shootRange;
            Collider aimCollider = null;

            if (Physics.Raycast(ray, out RaycastHit aimHit, _shootRange, shootLayer,
                                QueryTriggerInteraction.Ignore))
            {
                aimPoint = aimHit.point;
                aimCollider = aimHit.collider;
            }

            // 2. 枪口 → 目标点：中间有没有东西挡着
            Vector3 muzzle = _muzzlePoint != null ? _muzzlePoint.position : transform.position;
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
                _impulse?.GenerateImpulse(_impulseTime);
            }
            else
            {
                Debug.Log("[开火] 打空了");
            }
        }
       

        public void ReloadBullets()
        {
            int bullets = 0; // 能够装填的子弹数量
            if (_bulletsLeft >= _bulletsMag - _currentBullets)
            {
                bullets = _bulletsMag - _currentBullets; // 装满弹匣
            }
            else
            {
                bullets = _bulletsLeft; // 未装满弹匣
            }
            _bulletsLeft -= bullets;
            _currentBullets += bullets;

            // 更新背包
            ItemInventory.Instance.Remove(_currentBulletsId, bullets);
            
            // 触发装弹事件
            EventManager<string>.TriggerEvent(GameEvents.ReloadBullets, _currentBulletsId);
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