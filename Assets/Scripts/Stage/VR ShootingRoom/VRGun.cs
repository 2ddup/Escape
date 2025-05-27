///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using TempNamespace.Character.Equipment;
using UnityEngine;

namespace TempNamespace.Items
{
	using Character = Character.Character;
	public class VRGun : Equipment, IUsable
	{
		#region Inspector Fields
		[SerializeField, Tooltip("투사체 아이템의 생성 지점 및 방향")]
		private Transform _muzzle;		
		[SerializeField, Tooltip("생성될 투사체 Prefab")]
		private Transform _bulletPrefab;
		[SerializeField, Tooltip("생성될 총구 화염 Effect Prefab")]
		private Transform _muzzleFlashPrefab;
		[SerializeField, Tooltip("조준점 UI 활성화")]
		private Transform _gunFocusImg;
		[SerializeField, Tooltip("총알 갯수 UI")]
		private int _currentBullet;
		[SerializeField, Tooltip("총구에 그려지는 조준선")]
		private LineRenderer _lineRenderer;
		#endregion

		#region Fields
		Transform _transform;
		#endregion
		
		#region Properties
		public new Transform transform
		{
			get
			{
				#if UNITY_EDITOR
				if(_transform == null) _transform = GetComponent<Transform>();
				#endif
				return _transform;
			}
		}

		/// <summary>
		/// 조준점 UI 활성화
		/// </summary>
		public Transform GunFocus
		{
		   get => _gunFocusImg;
		   set => _gunFocusImg = value;
		}

		/// <summary>
		/// 투사체 아이템의 생성 지점 및 방향
		/// </summary>
		public Transform Muzzle
		{
		   get => _muzzle;
		   set => _muzzle = value;
		}
		/// <summary>
		/// 생성될 투사체 Prefab
		/// </summary>
		public Transform BulletPrefab
		{
		   get => _bulletPrefab;
		   set => _bulletPrefab = value;
		}

		/// <summary>
		/// 생성될 총구 화염 Effect Prefab
		/// </summary>
		public Transform MuzzleFlashPrefab
		{
			get => _muzzleFlashPrefab;
			set => _muzzleFlashPrefab = value;
		}

		/// <summary>
		/// 총구에 부착된 LineRenderer
		/// </summary>
		public LineRenderer LineRenderer
		{
			get => _lineRenderer;
			set => _lineRenderer = value;
		}
        #endregion

        #region	Events
        #endregion

        #region Methods

        public override void OnEquip(Character character)
        {
			_equippedCharacter = character;

			if (_gunFocusImg != null)
            _gunFocusImg.gameObject.SetActive(true);
        }

        public override void OnUnequip(Character character)
        {
			_equippedCharacter = null;

			if (_gunFocusImg != null)
            _gunFocusImg.gameObject.SetActive(false);
        }
		public void Use()
		{
			Fire();
		}

		#endregion

        #region MonoBehaviour Methods
        protected virtual void Awake()
		{
			CacheComponents();
		}
		#endregion

		#region Methods
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
			_lineRenderer = GetComponentInChildren<LineRenderer>();
		}

		/// <summary>
		/// 총 발사
		/// </summary>
		protected virtual void Fire()
		{
			var bullet = Instantiate(BulletPrefab, Muzzle.position, Muzzle.rotation);
			bullet.LookAt(Muzzle.position+Muzzle.forward);

			if(MuzzleFlashPrefab != null)
			{
				Transform flash = Instantiate(MuzzleFlashPrefab, Muzzle.position, Muzzle.rotation);
				Destroy(flash.gameObject, 1f);
			}	
			SoundManager.Instance.PlaySFX("GunShot", false);
		}

		public void DrawLine()
		{
			_lineRenderer.enabled = true;
		}

		public void EraseLine()
		{
			_lineRenderer.enabled = false;
		}
		#endregion

		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void Reset()
		{
			_equipmentType = EquipmentType.Hand;
		}
		protected virtual void OnValidate()
		{
		}
		#endif
        #endregion
    }
}