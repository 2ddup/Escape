///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;

namespace TempNamespace
{
	[RequireComponent(typeof(BoxCollider))]
	public class PhysicsVolume : MonoBehaviour
	{
        #region EDITOR EXPOSED FIELDS

        [SerializeField, Tooltip("Determines which PhysicsVolume takes precedence if they overlap (higher value == higher priority).")]
        private int priority;

        [SerializeField, Tooltip("Determines the amount of friction applied by the volume as Character using CharacterMovement moves through it.\n" +
                 "The higher this value, the harder it will feel to move through the volume.")]
        private float friction;

        [SerializeField, Tooltip("Determines the terminal velocity of Characters using CharacterMovement when falling.")]
        private float maxFallSpeed;

        [SerializeField, Tooltip("Determines if the volume contains a fluid, like water.")]
        private bool isFluid;

        [SerializeField, Tooltip("밀도가 높을수록 부력이 더 커짐, 기준치는 담수(1,000㎏/㎥)")]
        private float density;

        #endregion

        #region FIELDS
        private BoxCollider _collider;
        #endregion

        #region PROPERTIES

        /// <summary>
        /// This volume collider (trigger).
        /// </summary>
		
        public BoxCollider boxCollider
        {
            get
            {
				#if UNITY_EDITOR
                if (_collider == null)
                    _collider = GetComponent<BoxCollider>();
				#endif
                return _collider;
            }
        }

        /// <summary>
        /// Determines which PhysicsVolume takes precedence if they overlap (higher value == higher priority).
        /// </summary>

        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>
        /// 이 Volume을 지나가는 캐릭터의 마찰 계수, 값이 높을수록 지나가기 어려움
        /// </summary>

        public float Friction
        {
            get => friction;
            set => friction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// MovementSystem을 사용하는 캐릭터 객체의 최대 낙하 속도
        /// </summary>
        public float MaxFallSpeed
        {
            get => maxFallSpeed;
            set => maxFallSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// 이 공간은 액체인가?
        /// </summary>
        public bool IsFluid
        {
            get => isFluid;
            set => isFluid = value;
        }

        /// <summary>
        /// 부력을 계산하는 밀도
        /// </summary>
        public float Density
        {
            get => density;
            set => density = Mathf.Max(0, value);
        }
		#endregion

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_collider = GetComponent<BoxCollider>();
		}

		void Awake()
		{
			CacheComponents();
			boxCollider.isTrigger = true;
		}
		
        void OnValidate()
        {
			Friction = friction;
			MaxFallSpeed = maxFallSpeed;
            Density = density;
        }
        void Reset()
        {
			Priority = 0;
			Friction = .5f;
			MaxFallSpeed = 40;
			IsFluid = true;
            density = 1;
        }
    }
}