///스크립트 생성 일자 - 2025 - 02 - 26
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CHG.Utilities.Attribute;
using System;
using TempNamespace.Utilities;
using TempNamespace.Character.Controller.Physics;

namespace TempNamespace.Character.Controller
{
	/// <summary>
	/// 물리 기반으로 동작을 처리하는 컴퍼넌트
	/// </summary>
	[RequireComponent(typeof(CharacterPhysics)), DisallowMultipleComponent]
	public class PhysicsBasedController : MonoBehaviour
	{
		#region Member Enums
        /// <summary>
        /// 캐릭터의 현재 회전 방식
        /// </summary>
        public enum RotationMode
        {
            /// <summary>
            /// 캐릭터의 자체 회전 비활성화
            /// </summary>
            None,
            
            /// <summary>
            /// 캐릭터의 현재 이동 방향에 맞춰 회전
            /// </summary
            OrientRotationToMovement,
            
            /// <summary>
            /// 현재 카메라가 가리키는 방향으로 회전
            /// </summary>
            OrientRotationToViewDirection,
            
            /// <summary>
            /// RootMotion에 맞춰서 회전
            /// </summary>
            OrientWithRootMotion,
            
            /// <summary>
            /// 커스텀 회전 모드, CustomRotationModeUpdated 이벤트로 제어
            /// </summary>
            Custom
        }
        #endregion

		#region Inspector Fields


        [Header("Speed")]
        [SerializeField, Tooltip("캐릭터의 이동속도 계수")]
        private float _speedMultiplier;        

        [Header("Rotation")]
        [SerializeField, Tooltip("캐릭터의 현재 회전 모드")]
        private RotationMode _rotationMode;
        [SerializeField, Tooltip("초당 회전속도(Deg)")]
        private float _rotationRate;
        
        [Header("Gravity")]
        [SerializeField, Tooltip("캐릭터의 중력 벡터")]
        private Vector3 _gravity;
        [SerializeField, Tooltip("캐릭터에 적용되는 중력 계수, 0 미만의 값이면 역으로 적용")]
        private float _gravityScale;

        [Header("Input Values")]
        [SerializeField, Tooltip("실수(Float) 입력 값 목록")]
        private List<KeyValues<float>> _floatValues;
        [SerializeField, Tooltip("논리(Bool) 입력 값 목록")]
        private List<KeyValues<bool>> _boolValues;

        [Header("Etc Settings")]
        
        [SerializeField, Tooltip("루트 모션을 사용하는 캐릭터")]
        private bool _useRootMotion;

        [SerializeField, Tooltip("부력 계수, 0이면 부력 없음 / 1이면 기본")]
        private float _buoyancy;

        [SerializeField, Tooltip("발 아래에 있는 Rigidbody 객체에 무게 적용(흔들다리 등에서 사용)")]
        private bool _applyStandingDownwardForce;
        [SerializeField, Tooltip("아래쪽으로 가해지는 힘 계수"), ConditionalHide("_applyStandingDownwardForce")]
        private float _standingDownwardForceScale;

        [Space(10)]
        [SerializeField, Tooltip("래그돌 시스템")]
        private RagdollController _ragdollController;
        
        [Space(10)]
        [SerializeField, Tooltip("카메라 레퍼런스, null이 아니라면 카메라를 기준으로 이동 / null이라면 World 기준으로 이동")]
        private Camera _camera;

        [SerializeField, Tooltip("카메라가 위치할 Transform")]
        private Transform _cameraHolder;        
        #endregion

        #region Fields

        // STATE
        [SerializeField, HideInInspector]
        protected List<MoveState> _moveStates = new List<MoveState>();
        private MoveState _currentMoveState;
        [SerializeField, HideInInspector]
        private MoveState _initialMoveState;
        //

        // STANCES
        [SerializeField, HideInInspector]
        private List<CharacterStance> _characterStances = new List<CharacterStance>();

        // ACTIONS
        [SerializeField, HideInInspector]
        private List<CharacterAction> _characterActions = new List<CharacterAction>();
        //

        // CHARACTER INPUT EVENT VALUES
        private HashSet<string> _inputEvents = new HashSet<string>();        
        // 

        protected readonly List<PhysicsVolume> _physicsVolumes = new List<PhysicsVolume>();        

        private Coroutine _lateFixedUpdateCoroutine;
        private bool _enableAutoSimulation = true;
        
        private Transform _transform;
        private CharacterPhysics _physicsSystem;
        private Animator _animator;
        private RootMotionController _rootMotionController;
        private Transform _cameraTransform;
                
        private bool _useSeparateBrakingFriction;
        private float _brakingFriction;
        
        private bool _useSeparateBrakingDeceleration;
        private float _brakingDeceleration;
        
        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _rotationInput = Vector3.zero;

        private Vector3 _desiredVelocity = Vector3.zero;
                
        protected bool _isPaused;
        #endregion

        #region Properties
        
        /// <summary>
        /// 논리(Bool) 입력 값 목록
        /// </summary>
        public List<KeyValues<bool>> BoolValues
        {
           get => _boolValues;
           set => _boolValues = value;
        }

        /// <summary>
        /// 실수형 입력 값 목록
        /// </summary>
        public List<KeyValues<float>> FloatValues
        {
           get => _floatValues;
           set => _floatValues = value;
        }

        /// STATE
        /// <summary>
        /// State List
        /// </summary>
        public List<MoveState> MoveStates => _moveStates;
        
        /// <summary>
        /// 초기 이동 상태
        /// </summary>
        public MoveState InitialState
        {
           get => _initialMoveState;
           set => _initialMoveState = value;
        }
        /// <summary>
        /// 현재 이동 상태
        /// </summary>
        public MoveState CurrentState
        {
            get => _currentMoveState;
            set => _currentMoveState = value;
        }
        ///
        
        /// STANCE
        /// <summary>
        /// 캐릭터가 취할 수 있는 자세들
        /// </summary>
        public List<CharacterStance> CharacterStances
        {
            get => _characterStances;
            set => _characterStances = value;
        }
        //
        ///

        /// ACTIONS
        /// <summary>
        /// 캐릭터가 발생시킬 수 있는 행동, State보다 우선적으로 처리
        /// </summary>
        public List<CharacterAction> CharacterActions
        {
            get => _characterActions;
            set => _characterActions = value;
        }
        ///       
        
        /// <summary>
        /// 캐릭터의 이동속도 계수
        /// </summary>
        public float SpeedMultiplier
        {
           get => _speedMultiplier;
           set => _speedMultiplier = Mathf.Max(0, value);
        }

        /// <summary>
        /// This Character's camera transform.
        /// If assigned, the Character's movement will be relative to this, otherwise movement will be relative to world.
        /// </summary>
        public new Camera camera
        {
            get => _camera;
            set => _camera = value;
        }

        /// <summary>
        /// Cached camera transform (if any).
        /// </summary>
        public Transform cameraTransform
        {
            get
            {
                if (_camera != null)
                    _cameraTransform = _camera.transform;

                return _cameraTransform;
            }
        }
        /// <summary>
        /// 카메라가 위치할 Transform
        /// </summary>
        public Transform CameraHolder
        {
           get => _cameraHolder;
           protected set => _cameraHolder = value;
        }
        
        /// <summary>
        /// Cached Character transform.
        /// </summary>

        public new Transform transform => _transform;
        
        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>
        
        public CharacterPhysics physicsSystem => _physicsSystem;
        
        /// <summary>
        /// Cached Animator component. Can be null.
        /// </summary>

        public Animator animator => _animator;

        /// <summary>
        /// 래그돌 시스템
        /// </summary>
        public RagdollController RagdollController
        {
           get => _ragdollController;
           set => _ragdollController = value;
        }

        /// <summary>
        /// Cached Character's RootMotionController component. Can be null.
        /// </summary>

        public RootMotionController rootMotionController => _rootMotionController;
        
        /// <summary>
        /// Change in rotation per second, used when orientRotationToMovement or orientRotationToViewDirection are true.
        /// </summary>
        
        public float rotationRate
        {
            get => _rotationRate;
            set => _rotationRate = value;
        }
        
        /// <summary>
        /// The Character's current rotation mode.
        /// </summary>

        public RotationMode rotationMode
        {
            get => _rotationMode;
            set => _rotationMode = value;
        }        
        
        /// <summary>
        /// Is the crouch input pressed?
        /// </summary>

        public bool crouchInputPressed { get; protected set; }
        

        public bool UseGrounding => CurrentState ==  null ? false : CurrentState.StateFlag.HasFlag(MoveStateFlag.OnGround);
        /// <summary>
        /// 중력에 영향을 받는 상태인가?
        /// </summary>
        public bool UseGravity => CurrentState == null ? true : CurrentState.UseGravity;
        
        
        /// <summary>
        /// Water buoyancy ratio. 1 = Neutral Buoyancy, 0 = No Buoyancy.
        /// </summary>
        public float buoyancy
        {
            get => _buoyancy;
            set => _buoyancy = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Should use a separate braking friction ?
        /// </summary>

        public bool useSeparateBrakingFriction
        {
            get => _useSeparateBrakingFriction;
            set => _useSeparateBrakingFriction = value;
        }

        /// <summary>
        /// Friction (drag) coefficient applied when braking (whenever Acceleration = 0, or if Character is exceeding max speed).
        /// This is the value, used in all movement modes IF useSeparateBrakingFriction is true.
        /// </summary>

        public float brakingFriction
        {
            get => _brakingFriction;
            set => _brakingFriction = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Should use a separate braking deceleration ?
        /// </summary>

        public bool useSeparateBrakingDeceleration
        {
            get => _useSeparateBrakingDeceleration;
            set => _useSeparateBrakingDeceleration = value;
        }
        
        /// <summary>
        /// Deceleration when not applying acceleration.
        /// This is a constant opposing force that directly lowers velocity by a constant value.
        /// This is the value, used in all movement modes IF useSeparateBrakingDeceleration is true.
        /// </summary>

        public float brakingDeceleration
        {
            get => _brakingDeceleration;
            set => _brakingDeceleration = value;
        }
        
        /// <summary>
        /// The Character's gravity (modified by gravityScale). Defaults to Physics.gravity.
        /// </summary>

        public Vector3 gravity
        {
            get => _gravity * _gravityScale;
            set => _gravity = value;
        }

        /// <summary>
        /// The degree to which this object is affected by gravity.
        /// Can be negative allowing to change gravity direction.
        /// </summary>

        public float gravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }
        
        /// <summary>
        /// Should animation determines the Character' movement ?
        /// </summary>

        public bool useRootMotion
        {
            get => _useRootMotion;
            set => _useRootMotion = value;
        }
        
        /// <summary>
        /// If enabled, the player will interact with dynamic rigidbodies when walking into them.
        /// </summary>

        public bool enablePhysicsInteraction
        {
            get => _physicsSystem.EnablePhysicsInteraction;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif

                _physicsSystem.EnablePhysicsInteraction = value;
            }
        }

        /// <summary>
        /// Should apply push force to other characters when walking into them ?
        /// </summary>

        public bool AllowPushed
        {
            get => _physicsSystem.AllowPushed;
            set
            {                
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif
                _physicsSystem.AllowPushed = value;
            }
        }

        /// <summary>
        /// Should apply a downward force to rigidbodies we stand on ?
        /// </summary>

        public bool applyStandingDownwardForce
        {
            get => _applyStandingDownwardForce;
            set => _applyStandingDownwardForce = value;
        }

        /// <summary>
        /// This Character's mass (in Kg).
        /// </summary>

        public float mass
        {
            get => _physicsSystem.rigidbody.mass;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif
                _physicsSystem.rigidbody.mass = Mathf.Max(1e-07f, value);
            }
        }

        /// <summary>
        /// Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.
        /// </summary>

        public float ForceScale
        {
            get => _physicsSystem.ForceScale;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif
                _physicsSystem.ForceScale = Mathf.Max(value, 0);
            }
        }

        /// <summary>
        /// Force applied to rigidbodies we stand on (due to mass and gravity) is scaled by this amount.
        /// </summary>

        public float standingDownwardForceScale
        {
            get => _standingDownwardForceScale;
            set => _standingDownwardForceScale = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// If true, impart the platform's velocity when jumping or falling off it.
        /// </summary>

        public bool impartPlatformVelocity
        {
            get => _physicsSystem.ImpartPlatformVelocity;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif
                _physicsSystem.ImpartPlatformVelocity = value;
            }
        }

        /// <summary>
        /// Whether the Character moves with the moving platform it is standing on.
        /// If true, the Character moves with the moving platform.
        /// </summary>

        public bool impartPlatformMovement
        {
            get => _physicsSystem.ImpartPlatformMovement;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif

                _physicsSystem.ImpartPlatformMovement = value;
            }
        }

        /// <summary>
        /// Whether the Character receives the changes in rotation of the platform it is standing on.
        /// If true, the Character rotates with the moving platform.
        /// </summary>

        public bool impartPlatformRotation
        {
            get => _physicsSystem.ImpartPlatformRotation;
            set
            {
                #if UNITY_EDITOR
                if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
                #endif

                _physicsSystem.ImpartPlatformRotation = value;
            }
        }
        
        /// <summary>
        /// The character's current position (read only)
        /// Use SetPosition method to modify it. 
        /// </summary>
        
        public Vector3 position => physicsSystem.Position;
        
        /// <summary>
        /// The character's current position (read only).
        /// Use SetRotation method to modify it. 
        /// </summary>

        public Quaternion rotation => physicsSystem.Rotation;
        
        /// <summary>
        /// The character's current velocity (read only).
        /// Use SetVelocity method to modify it. 
        /// </summary>

        public Vector3 velocity => physicsSystem.Velocity;

        /// <summary>
        /// The Character's current speed.
        /// </summary>

        public float speed => physicsSystem.Speed;
        
        /// <summary>
        /// The character's current radius (read only).
        /// Use CharacterMovement SetDimensions method to modify it. 
        /// </summary>

        public float radius => physicsSystem.Radius;
        
        /// <summary>
        /// The character's current height (read only).
        /// Use CharacterMovement SetDimensions method to modify it. 
        /// </summary>

        public float height => physicsSystem.Height;
        
        /// <summary>
        /// PhysicsVolume overlapping this component. NULL if none.
        /// </summary>

        public PhysicsVolume physicsVolume { get; protected set; }
        
        /// <summary>
        /// If true, enables a LateFixedUpdate Coroutine to simulate this character.
        /// If false, Simulate method must be called in order to simulate this character.
        /// Enabled by default.
        /// </summary>
        public bool enableAutoSimulation
        {
            get => _enableAutoSimulation;
            set
            {
                _enableAutoSimulation = value;
                EnableAutoSimulationCoroutine(_enableAutoSimulation);
            }
        }
        
        /// <summary>
        /// 캐릭터가 일시정지 상태인가?
        /// </summary>
        public bool IsPaused => _isPaused;

        #endregion
        
        #region Events                       
        public delegate void CollidedEventHandler(ref CollisionResult collisionResult);
        public delegate void FoundGroundEventHandler(ref FindGroundResult foundGround);
        
        
        public delegate void JumpedEventHandler();
        
        /// <summary>
        /// 캐릭터가 특정 PhysicsVolume에 진입할 때 발동
        /// </summary>
        public Action<PhysicsVolume> PhysicsVolumeChanged;
        
        /// <summary>
        /// 캐릭터의 MovementMode가 Custom일 때 위치 이동 처리, 일반적으로 DeltaTime 전달
        /// </summary>
        public Action<float> CustomMovementModeUpdated;
        
        /// <summary>
        /// 캐릭터의 RotationMode가 Custom일 때 회전 처리, 일반적으로 DeltaTime 전달
        /// </summary>
        public Action<float> CustomRotationModeUpdated;
        
        /// <summary>
        /// 캐릭터의 물리 시뮬레이션 계산 전 호출, 일반적으로 DeltaTime 전달
        /// </summary>
        public Action<float>  BeforeSimulationUpdated;
        
        /// <summary>
        /// 캐릭터의 물리 시뮬레이션 계산 후 호출, 일반적으로 DeltaTime 전달
        /// </summary>
        public Action<float> AfterSimulationUpdated;
        
        /// <summary>
        /// Physics 업데이트 후 호출, 일반적으로 DeltaTime 전달
        /// </summary>
        public Action<float> MovementApplied;
        
        /// <summary>
        /// 이동 중 충돌 발생시 호출
        /// </summary>
        public event CollidedEventHandler Collided;

        /// <summary>
        /// 캐릭터가 발 아래쪽 방향에서 지면을 찾아냈을 때 호출
        /// </summary>
        public event FoundGroundEventHandler FoundGround;
        
        /// <summary>
        /// 캐릭터가 낙하 상태(MovementMode == Falling)일 때 발 아래쪽 방향에서 걸을 수 있는 지면을 찾으면 호출(=일반적으로 착지 시점), 찾아낸 시점에서의 속력 전달
        /// </summary>
        public Action<Vector3> Landed;
        
        /// <summary>
        /// 앉기 상태 돌입시 호출
        /// </summary>
        public Action Crouched;

        /// <summary>
        /// 앉기 상태 해제시 호출
        /// </summary>
        public Action UnCrouched;
        
        /// <summary>
        /// 캐릭터가 점프할 때 호출
        /// </summary>
        public Action Jumped;
        
        /// <summary>
        /// Event for implementing custom character movement mode.
        /// Called if MovementMode is set to Custom.
        /// Derived Character classes should override CustomMovementMode method instead. 
        /// </summary>
        
        protected virtual void OnCustomMovementMode(float deltaTime)
        {
            // Trigger event            
            CustomMovementModeUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Event for implementing custom character rotation mode.
        /// Called if RotationMode is set to Custom.
        /// Derived Character classes should override CustomRotationMode method instead. 
        /// </summary>
        
        protected virtual void OnCustomRotationMode(float deltaTime)
        {
            CustomRotationModeUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Called at the beginning of the Character Simulation, before current movement mode update.
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>
        
        protected virtual void OnBeforeSimulationUpdate(float deltaTime)
        {
            CurrentState?.OnBeforeSimulate(this);
            BeforeSimulationUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Called after current movement mode update.
        /// This 'hook' lets you externally update the character 'state'. 
        /// </summary>

        protected virtual void OnAfterSimulationUpdate(float deltaTime)
        {
            CurrentState?.OnAfterSimulate(this);
            AfterSimulationUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Event called when CharacterMovement component is updated (ie. Move call).
        /// At this point the character movement has been applied and its state is current. 
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>

        protected virtual void ApplyMovement(float deltaTime)
        {
            CurrentState?.OnApplyMovement(this);
            MovementApplied?.Invoke(deltaTime);
        }

        /// <summary>
        /// Event triggered when characters collides with other during a CharacterMovement Move call.
        /// Can be called multiple times.
        /// </summary>

        protected virtual void OnCollided(ref CollisionResult collisionResult)
        {
            Collided?.Invoke(ref collisionResult);
        }

        /// <summary>
        /// Event triggered when a character find ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        protected virtual void OnFoundGround(ref FindGroundResult foundGround)
        {
            FoundGround?.Invoke(ref foundGround);
        }

        /// <summary>
        /// Event triggered when character enter Walking movement mode (ie: isOnWalkableGround AND isConstrainedToGround).
        /// </summary>

        protected virtual void OnLanded(Vector3 landingVelocity)
        {
            Landed?.Invoke(landingVelocity);
        }
        
        /// <summary>
        /// Called when a jump has been successfully triggered.
        /// </summary>        
        protected virtual void OnJumped()
        {
            Jumped?.Invoke();
        }
        #endregion

        #region Methods
        public bool CheckBoolKey(string name, bool oneShot = true)
        {
            KeyValues<bool> keyValues = BoolValues.Find(x => x.name == name);
            if(keyValues != null && keyValues.value)
            {
                if(oneShot)
                    keyValues.value = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        public float CheckFloatKey(string name)
        {
            KeyValues<float> keyValues = FloatValues.Find(x => x.name == name);
            
            return keyValues == null ? 0 : keyValues.value;
        }

        public bool SetRagdollState(bool isRagdollActivate)
        {
            RagdollController.SetRagdoll(isRagdollActivate);

            return isRagdollActivate;
        }
        
        /// <summary>
        /// Returns the Character's gravity vector modified by gravityScale.
        /// </summary>

        public Vector3 GetGravityVector()
        {
            return gravity;
        }

        /// <summary>
        /// Returns the gravity direction (normalized).
        /// </summary>

        public Vector3 GetGravityDirection()
        {
            return gravity.normalized;
        }
        
        /// <summary>
        /// Returns the current gravity magnitude factoring current gravity scale.
        /// </summary>

        public float GetGravityMagnitude()
        {
            return gravity.magnitude;
        }

        /// <summary>
        /// Sets the Character's gravity vector
        /// </summary>

        public void SetGravityVector(Vector3 newGravityVector)
        {
            _gravity = newGravityVector;
        }
        
        /// <summary>
        /// Start / Stops Auto-simulation coroutine (ie: LateFixedUpdate).
        /// </summary>

        private void EnableAutoSimulationCoroutine(bool enable)
        {
            if (enable)
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);

                _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
            }
            else
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);
            }
        }
        
        /// <summary>
        /// Cache used components.
        /// </summary>

        protected virtual void CacheComponents()
        {
            _physicsSystem = GetComponent<CharacterPhysics>();

            _transform = GetComponent<Transform>();            
            _animator = GetComponentInChildren<Animator>();
            _rootMotionController = GetComponentInChildren<RootMotionController>();
            
            if(_camera == null)
                _camera = Camera.main;
        }
        
        /// <summary>
        /// Sets the given new volume as our current Physics Volume.
        /// Trigger PhysicsVolumeChanged event.
        /// </summary>

        protected virtual void SetPhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            // Do nothing if nothing is changing            
            if (newPhysicsVolume == physicsVolume)
                return;

            // Trigger PhysicsVolumeChanged event
            PhysicsVolumeChanged?.Invoke(newPhysicsVolume);

            // Updates current physics volume
            physicsVolume = newPhysicsVolume;
        }

        /// <summary>
        /// Update character's current physics volume.
        /// </summary>
        protected virtual void UpdatePhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            // Check if Character is inside or outside a PhysicsVolume,
            // It uses the Character's center as reference point

            Vector3 characterCenter = physicsSystem.CharacterCenter;

            if (newPhysicsVolume && newPhysicsVolume.boxCollider.ClosestPoint(characterCenter) == characterCenter)
            {
                // Entering physics volume

                SetPhysicsVolume(newPhysicsVolume);
            }
            else
            {
                // Leaving physics volume

                SetPhysicsVolume(null);
            }
        }

        /// <summary>
        /// Attempts to add a new physics volume to our volumes list.
        /// </summary>
        protected virtual void AddPhysicsVolume(PhysicsVolume volume)//Collider other)
        {
            _physicsVolumes.Insert(0, volume);
        }

        /// <summary>
        /// Attempts to remove a physics volume from our volumes list.
        /// </summary>
        protected virtual void RemovePhysicsVolume(PhysicsVolume volume)
        {
            _physicsVolumes.Remove(volume);
        }

        /// <summary>
        /// Sets as current physics volume the one with higher priority.
        /// </summary>

        protected virtual void UpdatePhysicsVolumes()
        {
            // Find volume with higher priority
            PhysicsVolume volume = null;
            int maxPriority = int.MinValue;

            for (int i = 0, c = _physicsVolumes.Count; i < c; i++)
            {
                PhysicsVolume vol = _physicsVolumes[i];
                if (vol.Priority <= maxPriority)
                    continue;

                maxPriority = vol.Priority;
                volume = vol;
            }

            // Update character's current volume
            UpdatePhysicsVolume(volume);
        }
        
        /// <summary>
        /// Is the character in a water physics volume ?
        /// </summary>

        public virtual bool IsInWaterPhysicsVolume()
        {
            return physicsVolume && physicsVolume.IsFluid;
        }
        
        /// <summary>
        /// Adds a force to the Character.
        /// This forces will be accumulated and applied during Move method call.
        /// </summary>

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            physicsSystem.AddForce(force, forceMode);
        }

        /// <summary>
        /// Applies a force to a rigidbody that simulates explosion effects.
        /// </summary>

        public void AddExplosionForce(float forceMagnitude, Vector3 origin, float explosionRadius, float upwardModifier, ForceMode forceMode = ForceMode.Force)
        {
            physicsSystem.AddExplosionForce(forceMagnitude, origin, explosionRadius, upwardModifier, forceMode);
        }

        /// <summary>
        /// Set a pending launch velocity on the Character. This velocity will be processed next Move call.
        /// If overrideVerticalVelocity is true replace the vertical component of the Character's velocity instead of adding to it.
        /// If overrideLateralVelocity is true replace the XY part of the Character's velocity instead of adding to it.
        /// </summary>

        public void LaunchCharacter(Vector3 launchVelocity, bool overrideVerticalVelocity = false,
            bool overrideLateralVelocity = false)
        {
            physicsSystem.LaunchCharacter(launchVelocity, overrideVerticalVelocity, overrideLateralVelocity);
        }

        /// <summary>
        /// Should collision detection be enabled ?
        /// </summary>

        public void DetectCollisions(bool detectCollisions)
        {
            physicsSystem.DetectCollisions = detectCollisions;
        }

        /// <summary>
        /// Makes the character to ignore all collisions vs otherCollider.
        /// </summary>

        public void IgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            physicsSystem.IgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Makes the character to ignore collisions vs all colliders attached to the otherRigidbody.
        /// </summary>

        public void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
        {
            physicsSystem.IgnoreCollision(otherRigidbody, ignore);
        }

        /// <summary>
        /// Makes the character's collider (eg: CapsuleCollider) to ignore all collisions vs otherCollider.
        /// NOTE: The character can still collide with other during a Move call if otherCollider is in CollisionLayers mask.
        /// </summary>

        public void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            physicsSystem.CapsuleIgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Jump, etc.
        /// </summary>

        public void PauseGroundConstraint(float seconds = 0.1f)
        {
            physicsSystem.PauseGroundConstraint(seconds);
        }
        
        /// <summary>
        /// Should movement be constrained to ground when on walkable ground ?
        /// When enabled, character will be constrained to ground ignoring vertical velocity.  
        /// </summary>

        public void EnableGroundConstraint(bool enable)
        {
            physicsSystem.ConstrainToGround = enable;
        }
        
        /// <summary>
        /// Was the character on ground last Move call ?
        /// </summary>

        public bool WasOnGround()
        {
            return physicsSystem.WasOnGround;
        }

        /// <summary>
        /// Is the character on ground ?
        /// </summary>

        public bool IsOnGround()
        {
            return physicsSystem.IsOnGround;
        }

        /// <summary>
        /// Was the character on walkable ground last Move call ?
        /// </summary>

        public bool WasOnWalkableGround()
        {
            return physicsSystem.wasOnWalkableGround;
        }

        /// <summary>
        /// Is the character on walkable ground ?
        /// </summary>

        public bool IsOnWalkableGround()
        {
            return physicsSystem.isOnWalkableGround;
        }

        /// <summary>
        /// Was the character on walkable ground AND constrained to ground last Move call ?
        /// </summary>

        public bool WasGrounded()
        {
            return physicsSystem.wasGrounded;
        }

        /// <summary>
        /// Is the character on walkable ground AND constrained to ground.
        /// </summary>

        public bool IsGrounded()
        {
            return physicsSystem.IsGrounded;
        }

        /// <summary>
        /// Return the RootMotionController or null is not found.
        /// </summary>

        public RootMotionController GetRootMotionController()
        {
            return rootMotionController;
        }

        /// <summary>
        /// Return the Character's current PhysicsVolume, null if none.
        /// </summary>

        public PhysicsVolume GetPhysicsVolume()
        {
            return physicsVolume;
        }

        /// <summary>
        /// Sets the Character's position.
        /// This complies with the interpolation resulting in a smooth transition between the two positions in any intermediate frames rendered.
        /// </summary>

        public void SetPosition(Vector3 position, bool updateGround = false)
        {
            physicsSystem.SetPosition(position, updateGround);
        }

        /// <summary>
        /// Instantly modify the character's position.
        /// Unlike SetPosition this disables rigidbody interpolation (interpolating == true) before updating the character's position resulting in an instant movement.
        /// If interpolating == true it will re-enable rigidbody interpolation after teleportation.
        /// </summary>

        public void TeleportPosition(Vector3 newPosition, bool interpolating = true, bool updateGround = false)
        {
            if (interpolating)
            {
                physicsSystem.rigidbody.interpolation = RigidbodyInterpolation.None;
            }

            physicsSystem.SetPosition(newPosition, updateGround);

            if (interpolating)
            {
                physicsSystem.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        /// <summary>
        /// Instantly modify the character's rotation.
        /// Unlike SetRotation this disables rigidbody interpolation (interpolating == true) before updating the character's rotation resulting in an instant rotation.
        /// If interpolating == true it will re-enable rigidbody interpolation after teleportation.
        /// </summary>

        public void TeleportRotation(Quaternion newRotation, bool interpolating = true)
        {
            if (interpolating)
            {
                physicsSystem.rigidbody.interpolation = RigidbodyInterpolation.None;
            }

            physicsSystem.SetRotation(newRotation);

            if (interpolating)
            {
                physicsSystem.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        
        /// <summary>
        /// The Character's current up vector.
        /// </summary>
        public virtual Vector3 GetUpVector()
        {
            return transform.up;
        }

        /// <summary>
        /// The Character's current right vector.
        /// </summary>
        public virtual Vector3 GetRightVector()
        {
            return transform.right;
        }

        /// <summary>
        /// The Character's current forward vector.
        /// </summary>
        public virtual Vector3 GetForwardVector()
        {
            return transform.forward;
        }
        
        /// <summary>
        /// Orient the character's towards the given direction (in world space) using rotationRate as the rate of rotation change.
        /// If updateYawOnly is true, rotation will affect character's yaw axis only (defined by its up-axis).
        /// </summary>
        
        public virtual void RotateTowards(Vector3 worldDirection, float deltaTime, bool updateYawOnly = true)
        {
            Vector3 characterUp = GetUpVector();

            if (updateYawOnly)
                worldDirection = Vector3.ProjectOnPlane(worldDirection, characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);
            physicsSystem.Rotation = Quaternion.RotateTowards(rotation, targetRotation, rotationRate * deltaTime);
        }
        
        /// <summary>
        /// Append root motion rotation to Character's rotation.
        /// </summary>

        protected virtual void RotateWithRootMotion()
        {
            if (useRootMotion && rootMotionController)
                physicsSystem.Rotation = rootMotionController.ConsumeRootMotionRotation() * physicsSystem.Rotation;
        }
		        
        /// <summary>
        /// The current movement direction (in world space), eg: the movement direction used to move this Character.
        /// </summary>

        public Vector3 GetMovementDirection()
        {
            return _movementDirection;
        }

        /// <summary>
        /// Assigns the Character's movement direction (in world space), eg: our desired movement direction vector.
        /// </summary>

        public void SetMovementDirection(Vector3 movementDirection)
        {
            _movementDirection = movementDirection;
        }
        
        /// <summary>
        /// Sets the yaw value.
        /// This will reset current pitch and roll values.
        /// </summary>

        public virtual void SetYaw(float value)
        {
            physicsSystem.Rotation = Quaternion.Euler(0.0f, value, 0.0f);
        }
        
        /// <summary>
        /// Amount to add to Yaw (up axis).
        /// </summary>

        public virtual void AddYawInput(float value)
        {
            _rotationInput.y += value;
        }

        /// <summary>
        /// Amount to add to Pitch (right axis).
        /// </summary>

        public virtual void AddPitchInput(float value)
        {
            _rotationInput.x += value;
        }

        /// <summary>
        /// Amount to add to Roll (forward axis).
        /// </summary>

        public virtual void AddRollInput(float value)
        {
            _rotationInput.z += value;
        }

        /// <summary>
        /// Append input rotation (eg: AddPitchInput, AddYawInput, AddRollInput) to character rotation.
        /// </summary>

        protected virtual void ConsumeRotationInput()
        {
            // Apply rotation input (if any)

            if (_rotationInput != Vector3.zero)
            {
                // Consumes rotation input (e.g. apply and clear it)

                physicsSystem.Rotation *= Quaternion.Euler(_rotationInput);

                _rotationInput = Vector3.zero;
            }
        }

        /// <summary>
        /// The maximum speed for current movement mode (accounting crouching state).
        /// </summary>
        public virtual float GetMaxSpeed()
        {
            if(CurrentState == null)
                return 0;
            else
                return CurrentState.MaxSpeed * SpeedMultiplier;
        }

        /// <summary>
        /// The acceleration for current movement mode.
        /// </summary>
        public virtual float GetMaxAcceleration()
        {
            if(CurrentState == null)
                return 0;
            else
                return CurrentState.MaxAcceleration;
        }

        /// <summary>
        /// The braking deceleration for current movement mode.
        /// </summary>

        public virtual float GetMaxBrakingDeceleration()
        {
            if(CurrentState == null)
                return 0;
            else
                return CurrentState.MaxDeceleration;
        }
        
        /// <summary>
        /// Computes the analog input modifier (0.0f to 1.0f) based on current input vector and desired velocity.
        /// </summary>
        
        protected virtual float ComputeAnalogInputModifier(Vector3 desiredVelocity)
        {
            float maxSpeed = GetMaxSpeed();
            
            if (desiredVelocity.sqrMagnitude > 0.0f && maxSpeed > 0.00000001f)
            {
                return Mathf.Clamp01(desiredVelocity.magnitude / maxSpeed);
            }

            return 0.0f;
        }
        
        /// <summary>
        /// Apply friction and braking deceleration to given velocity.
        /// Returns modified input velocity.
        /// </summary>
        
        public virtual Vector3 ApplyVelocityBraking(Vector3 velocity, float friction, float maxBrakingDeceleration, float deltaTime)
        {
            const float kMinTickTime = 0.000001f;
            if (velocity.IsZero() || deltaTime < kMinTickTime)
                return velocity;
            
            bool isZeroFriction = friction == 0.0f;
            bool isZeroBraking = maxBrakingDeceleration == 0.0f;
            if (isZeroFriction && isZeroBraking)
                return velocity;
            
            // Decelerate to brake to a stop
            
            Vector3 oldVel = velocity;
            Vector3 revAccel = isZeroBraking ? Vector3.zero : -maxBrakingDeceleration * velocity.normalized;
            
            // Subdivide braking to get reasonably consistent results at lower frame rates
            
            const float kMaxTimeStep = 1.0f / 33.0f;
            
            float remainingTime = deltaTime;
            while (remainingTime >= kMinTickTime)
            {
                // Zero friction uses constant deceleration, so no need for iteration

                float dt = remainingTime > kMaxTimeStep && !isZeroFriction
                    ? Mathf.Min(kMaxTimeStep, remainingTime * 0.5f)
                    : remainingTime;
                
                remainingTime -= dt;
                
                // Apply friction and braking
                
                velocity += (-friction * velocity + revAccel) * dt;
                
                // Don't reverse direction
                
                if (Vector3.Dot(velocity, oldVel) <= 0.0f)
                    return Vector3.zero;
            }
            
            // Clamp to zero if nearly zero, or if below min threshold and braking
            
            float sqrSpeed = velocity.sqrMagnitude;
            if (sqrSpeed <= 0.00001f || (!isZeroBraking && sqrSpeed <= 0.1f))
                return Vector3.zero;

            return velocity;
        }

        /// <summary>
        /// Calculates a new velocity for the given state, applying the effects of friction or
        /// braking friction and acceleration or deceleration.
        /// </summary>

        public virtual Vector3 CalcVelocity(Vector3 velocity, Vector3 desiredVelocity, float friction, bool isFluid, float deltaTime)
        {
            const float kMinTickTime = 0.000001f;
            if (deltaTime < kMinTickTime)
                return velocity;
            
            // Compute requested move direction

            float desiredSpeed = desiredVelocity.magnitude;
            Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

            // Requested acceleration (factoring analog input)

            float analogInputModifier = ComputeAnalogInputModifier(desiredVelocity);
            Vector3 inputAcceleration = GetMaxAcceleration() * analogInputModifier * desiredMoveDirection;

            // Actual max speed (factoring analog input)

            float actualMaxSpeed = Mathf.Max(0, GetMaxSpeed() * analogInputModifier);
            
            // Apply braking or deceleration
            
            bool isZeroAcceleration = inputAcceleration.IsZero();
            bool isVelocityOverMax = velocity.IsExceeding(actualMaxSpeed);
            
            // Only apply braking if there is no acceleration, or we are over our max speed and need to slow down to it.

            if (isZeroAcceleration || isVelocityOverMax)
            {
                Vector3 oldVelocity = velocity;
                
                // Apply friction and braking

                float actualBrakingFriction = useSeparateBrakingFriction ? brakingFriction : friction;
                float actualBrakingAcceleration =
                    useSeparateBrakingDeceleration ? brakingDeceleration : GetMaxBrakingDeceleration();

                velocity = ApplyVelocityBraking(velocity, actualBrakingFriction, actualBrakingAcceleration, deltaTime);
                
                // Don't allow braking to lower us below max speed if we started above it.
                
                if (isVelocityOverMax && velocity.sqrMagnitude < actualMaxSpeed.Square() && Vector3.Dot(inputAcceleration, oldVelocity) > 0.0f)
                    velocity = oldVelocity.normalized * actualMaxSpeed;
            }
            else
            {
                // Friction, this affects our ability to change direction
                
                Vector3 accelDir = inputAcceleration.normalized;
                float velMag = velocity.magnitude;

                velocity -= (velocity - accelDir * velMag) * Mathf.Min(friction * deltaTime, 1.0f);
            }
            
            // Apply fluid friction
            
            if (isFluid)
                velocity *= 1.0f - Mathf.Min(friction * deltaTime, 1.0f);
            
            // Apply input acceleration

            if (!isZeroAcceleration)
            {
                float newMaxSpeed = velocity.IsExceeding(actualMaxSpeed) ? velocity.magnitude : actualMaxSpeed;

                velocity += inputAcceleration * deltaTime;
                velocity = Vector3.ClampMagnitude(velocity, newMaxSpeed);
            }

            return velocity;
        }
        
        /// <summary>
        /// Enforce constraints on input vector given current movement mode.
        /// Return constrained input vector.
        /// </summary>
        
        public virtual Vector3 ConstrainInputVector(Vector3 inputVector)
        {
            Vector3 worldUp = -GetGravityDirection();
            
            float inputVectorDotWorldUp = Vector3.Dot(inputVector, worldUp);
            if (!Mathf.Approximately(inputVectorDotWorldUp, 0.0f) && UseGravity)
                inputVector = Vector3.ProjectOnPlane(inputVector, worldUp);

            return physicsSystem.ConstrainVectorToPlane(inputVector);
        }
        
        /// <summary>
        /// Calculate the desired velocity for current movement mode.
        /// </summary>

        protected virtual void CalcDesiredVelocity(float deltaTime)
        {
            // Current movement direction

            Vector3 movementDirection = Vector3.ClampMagnitude(GetMovementDirection(), 1.0f);

            // The desired velocity from animation (if using root motion) or from input movement vector

            Vector3 desiredVelocity = useRootMotion && rootMotionController
                ? rootMotionController.ConsumeRootMotionVelocity(deltaTime)
                : movementDirection * GetMaxSpeed();
            
            // Return constrained desired velocity

            _desiredVelocity = ConstrainInputVector(desiredVelocity);
        }
        
        /// <summary>
        /// Calculated desired velocity for current movement mode.
        /// </summary>

        public virtual Vector3 GetDesiredVelocity()
        {
            return _desiredVelocity;
        }
        
        /// <summary>
        /// Calculates the signed slope angle in degrees for current movement direction.
        /// Positive if moving up-slope, negative if moving down-slope or 0 if Character
        /// is not on ground or not moving (ie: movementDirection == Vector3.zero).
        /// </summary>
        
        public float GetSignedSlopeAngle()
        {
            Vector3 movementDirection = GetMovementDirection();
            if (movementDirection.IsZero() || !IsOnGround())
                return 0.0f;

            Vector3 projMovementDirection =
                Vector3.ProjectOnPlane(movementDirection, physicsSystem.GroundNormal).normalized;

            return Mathf.Asin(Vector3.Dot(projMovementDirection, -GetGravityDirection())) * Mathf.Rad2Deg;
        }
        
        /// <summary>
        /// Apply a downward force when standing on top of non-kinematic physics objects (if applyStandingDownwardForce == true).
        /// The force applied is: mass * gravity * standingDownwardForceScale
        /// </summary>        
        public virtual void ApplyDownwardsForce()
        {
            Rigidbody groundRigidbody = physicsSystem.GroundRigidbody;
            if (!groundRigidbody || groundRigidbody.isKinematic)
                return;

            Vector3 downwardForce = mass * GetGravityVector();
            groundRigidbody.AddForceAtPosition(downwardForce * standingDownwardForceScale, position);
        }
        
        
        /// <summary>
        /// How deep in water the character is immersed.
        /// Returns a float in range 0.0 = not in water, 1.0 = fully immersed.
        /// </summary>

        public virtual float CalcImmersionDepth()
        {
            float depth = 0.0f;

            if (IsInWaterPhysicsVolume())
            {
                float height = physicsSystem.Height;
                if (height == 0.0f || buoyancy == 0.0f)
                    depth = 1.0f;
                else
                {
                    Vector3 worldUp = -GetGravityDirection();
                    
                    Vector3 rayOrigin = position + worldUp * height;
                    Vector3 rayDirection = -worldUp;
                    
                    BoxCollider waterVolumeCollider = physicsVolume.boxCollider;

                    depth = !waterVolumeCollider.Raycast(new Ray(rayOrigin, rayDirection), out RaycastHit hitInfo, height)
                        ? 1.0f
                        : 1.0f - Mathf.InverseLerp(0.0f, height, hitInfo.distance);
                }
            }
            
            return depth;
        }
        
        /// <summary>
        /// Returns the Character's current rotation mode.
        /// </summary>

        public RotationMode GetRotationMode()
        {
            return _rotationMode;
        }

        /// <summary>
        /// Sets the Character's current rotation mode:
        ///     -None:                          Disable rotation.
        ///     -OrientRotationToMovement:      Rotate the Character toward the direction of acceleration, using rotationRate as the rate of rotation change.
        ///     -OrientRotationToViewDirection: Smoothly rotate the Character toward camera's view direction, using rotationRate as the rate of rotation change.
        ///     -OrientWithRootMotion:          Let root motion handle Character rotation.
        ///     -Custom:                        User-defined custom rotation mode.
        /// </summary>

        public void SetRotationMode(RotationMode rotationMode)
        {
            _rotationMode = rotationMode;
        }
        
        /// <summary>
        /// Updates the Character's rotation based on its current RotationMode.
        /// </summary>

        protected virtual void UpdateRotation(float deltaTime)
        {
            if (_rotationMode == RotationMode.None)
            {
                // Do nothing
            }
            else if (_rotationMode == RotationMode.OrientRotationToMovement)
            {
                // Determines if rotation should modify character's yaw only
            
                bool shouldRemainVertical = UseGravity;
                
                // Smoothly rotate the Character toward the movement direction, using rotationRate as the rate of rotation change
                
                RotateTowards(_movementDirection, deltaTime, shouldRemainVertical);
            }
            else if (_rotationMode == RotationMode.OrientRotationToViewDirection && camera != null)
            {
                // Determines if rotation should modify character's yaw only
            
                bool shouldRemainVertical = UseGravity;
                
                // Smoothly rotate the Character toward camera's view direction, using rotationRate as the rate of rotation change
                
                RotateTowards(cameraTransform.forward, deltaTime, shouldRemainVertical);
            }
            else if (_rotationMode == RotationMode.OrientWithRootMotion)
            {
                // Let root motion handle Character rotation
                
                RotateWithRootMotion();
            }
            else if (_rotationMode == RotationMode.Custom)
            {
                CustomRotationMode(deltaTime);
            }
        }
        
        /// <summary>
        /// User-defined custom rotation mode.
        /// Called if RotationMode is set to Custom.
        /// </summary>
        
        protected virtual void CustomRotationMode(float deltaTime)
        {
            // Trigger CustomRotationModeUpdated event
            
            OnCustomRotationMode(deltaTime);
        }
        
        private void BeforeSimulationUpdate(float deltaTime)
        {
            // Handle Movement Direction Input
            HandleMovementInput();
            
            if(CurrentState != null && CurrentState.IsFinished(this) || !MoveStates.Contains(CurrentState))
            {
                CurrentState?.OnStateExit(this, MoveStateFlag.None);
                CurrentState = null;
            }

            for(int i = 0; i < MoveStates.Count; ++i)
            {
                if(MoveStates[i] != null && MoveStates[i].CanTransitionHere(this))
                {
                    ChangeState(MoveStates[i]);
                    break;
                }
            }
            
            // Update active physics volume            
            UpdatePhysicsVolumes();

            for(int i = 0; i < CharacterStances.Count; ++i)
            {
                if(CharacterStances[i] == null)
                    continue;

                if(!CharacterStances[i].StanceActivated)
                {
                    if(CharacterStances[i].CanActivateStance(this))
                    {
                        CharacterStances[i].ActivateStance(this);
                    }
                }
                else
                {
                    if(CharacterStances[i].CanDeactivateStance(this))
                    {
                        CharacterStances[i].DeactivateStance(this);
                    }
                }
            }

            for(int i = 0; i < CharacterActions.Count; ++i)
            {
                CharacterActions[i]?.OnBeforeSimulate(this);
            }

            
            // Trigger BeforeSimulationUpdated event
            OnBeforeSimulationUpdate(deltaTime);
        }

        /// <summary>
        /// 이동 값을 입력 및 처리
        /// </summary>
        protected virtual void HandleMovementInput()
        {
            if(!CurrentState.IsLocomotive)
                return;

            Vector2 inputMove = new Vector2()
            {
                x = CheckFloatKey("Horizontal"),
                y = CheckFloatKey("Vertical"),
            };
            
            Vector3 movementDirection = Vector3.zero;
            movementDirection += Vector3.forward * inputMove.y;
            movementDirection += Vector3.right * inputMove.x;
            
            if(UseGravity)
            {
                movementDirection = movementDirection.RelativeTo(cameraTransform, GetUpVector());
            }
            else
            {
                movementDirection = movementDirection.RelativeTo(cameraTransform, camera.transform.up);
            }
            
            
            SetMovementDirection(movementDirection);
        }
        protected virtual void HandleRotationInput()
        {
        }

        private void SimulationUpdate(float deltaTime)
        {
            // Calculate desired velocity for current movement mode

            CalcDesiredVelocity(deltaTime);

            CurrentState?.OnSimulate(this);            
            
            // Update rotation
            UpdateRotation(deltaTime);
            
            // Append input rotation (eg: AddYawInput, etc)

            ConsumeRotationInput();
        }
        
        private void AfterSimulationUpdate(float deltaTime)
        {
            CurrentState?.OnAfterSimulate(this);

            OnAfterSimulationUpdate(deltaTime);
        }

        private void CharacterMovementUpdate(float deltaTime)
        {
            // Perform movement            
            physicsSystem.Move(deltaTime);
            
            // Trigger CharacterMovementUpdated event            
            ApplyMovement(deltaTime);
            
            // If not using root motion, flush root motion accumulated deltas.
            // This prevents accumulation while character is toggling root motion.
            if (!useRootMotion && rootMotionController)
                rootMotionController.FlushAccumulatedDeltas();
        }
        
        /// <summary>
        /// Perform this character simulation, ie: update velocity, position, rotation, etc.
        /// Automatically called when enableAutoSimulation is true.
        /// </summary>        
        public void Simulate(float deltaTime)
        {
            if (_isPaused)
                return;
            
            BeforeSimulationUpdate(deltaTime);
            SimulationUpdate(deltaTime);
            AfterSimulationUpdate(deltaTime);
            CharacterMovementUpdate(deltaTime);
        }
        
        /// <summary>
        /// If enableAutoSimulation is true, perform this character simulation.
        /// </summary>
        private void OnLateFixedUpdate()
        {
            // Simulate this character
            Simulate(Time.deltaTime);
        }
        
        /// <summary>
        /// Pause / Resume Character.
        /// When paused, a character prevents any interaction (no movement, no rotation, no collisions, etc.)
        ///  If clearState is true, will clear any pending movement, forces and rotations.
        /// </summary>
        public void Pause(bool pause, bool clearState = true)
        {
            _isPaused = pause;
            physicsSystem.collider.enabled = !_isPaused;
            
            if (clearState)
            {
                ClearState();
            }

            if(IsPaused) OnPause();
            else OnResume();
        }
        public virtual void ClearState()
        {
            _movementDirection = Vector3.zero;
            _rotationInput = Vector3.zero;
            
            physicsSystem.Velocity = Vector3.zero;
            physicsSystem.ClearAccumulatedForces();
        }
        protected virtual void OnPause()
        {

        }
        protected virtual void OnResume()
        {

        }


        
        public void ChangeState(MoveState newState)
        {
            if(CurrentState != null)
            {
                CurrentState.OnStateExit(this, newState == null ? MoveStateFlag.None : newState.StateFlag);
            }

            if(newState == null)
            {
                physicsSystem.Velocity = Vector3.zero;
                physicsSystem.ClearAccumulatedForces();
            }
            else
            {
                newState.OnStateEnter(this, CurrentState == null ? MoveStateFlag.None : CurrentState.StateFlag);
            }

            CurrentState = newState;
        }

        
        #endregion

        #region MonoBehaviour
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void Reset()
        {
            _speedMultiplier = 1;
            
            _rotationMode = RotationMode.OrientRotationToMovement;
            _rotationRate = 540.0f;
                        

            _buoyancy = 1.0f;

            _gravity = new Vector3(0.0f, -9.81f, 0.0f);
            _gravityScale = 1.0f;
            
            _useRootMotion = false;
            
            _applyStandingDownwardForce = false;            
            _standingDownwardForceScale = 1.0f;

            #if UNITY_EDITOR
            if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
            #endif
            _physicsSystem.ForceReset();
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void OnValidate()
        {
            rotationRate = _rotationRate;
            
            buoyancy = _buoyancy;

            gravityScale = _gravityScale;
            
            useRootMotion = _useRootMotion;

            if (_physicsSystem == null)
                _physicsSystem = GetComponent<CharacterPhysics>();

            standingDownwardForceScale = _standingDownwardForceScale;
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void Awake()
        {
            // Cache components
            CacheComponents();
            
            // Set starting movement mode
            ChangeState(InitialState);
            //ChangeState();
        }

        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void Update()
        {

        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void OnEnable()
        {
            
            #if UNITY_EDITOR
            if(_physicsSystem == null) _physicsSystem = GetComponent<CharacterPhysics>();
            _physicsSystem.hideFlags |= HideFlags.HideInInspector;
            #endif
            
            // Physics의 Event 구독
            physicsSystem.Collided += OnCollided;
            physicsSystem.FoundGround += OnFoundGround;
            
            if (_enableAutoSimulation)
                EnableAutoSimulationCoroutine(true);
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void OnDisable()
        {
            // Physics의 Event 구독 해제            
            physicsSystem.Collided -= OnCollided;
            physicsSystem.FoundGround -= OnFoundGround;
            
            if (_enableAutoSimulation)
                EnableAutoSimulationCoroutine(false);
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void Start()
        {
            if (UseGravity)
            {
                physicsSystem.SetPosition(transform.position, true);
            }
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent(out PhysicsVolume volume))
            {
                AddPhysicsVolume(volume);
            }
        }
        
        /// <summary>
        /// override하겠다면 반드시 base Method를 호출할 것
        /// </summary>
        protected virtual void OnTriggerExit(Collider other)
        {
            if(other.TryGetComponent(out PhysicsVolume volume))
            {
                RemovePhysicsVolume(volume);
            }
        }
        
        /// <summary>
        /// FixedUpdate 종료시마다 호출
        /// </summary>        
        private IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return waitTime;

                OnLateFixedUpdate();
            }
        }

        #endregion
    }
}