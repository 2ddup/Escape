///스크립트 생성 일자 - 2025 - 02 - 26
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using System.Collections.Generic;
using TempNamespace.Utilities;
using UnityEngine;

namespace TempNamespace.Character.Controller.Physics
{
	/// <summary>
	/// 캐릭터 객체의 물리 엔진 처리를 담당할 컴퍼넌트 <br/>
    /// 이 컴퍼넌트는 캐릭터 물리 처리의 기반이므로 상속해서 사용하지 말 것
	/// </summary>
	[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider)), DisallowMultipleComponent]
	public sealed class CharacterPhysics : MonoBehaviour
	{
        #region Enums
        
        public enum DepenetrationBehaviour
        {
            IgnoreNone = 0,

            IgnoreStatic = 1 << 0,
            IgnoreDynamic = 1 << 1,
            IgnoreKinematic = 1 << 2
        }
        #endregion

		#region Constants
        //물리 오류 방지용 작은 값의 기준치
        private const float SmallNumber = 0.0001f;

        //충돌 방향 계산을 위한 반구 크기
        private const float HemisphereLimit = 0.01f;

        //최대 충돌 개수
        private const int MaxCollisionCount = 16;
        private const int MaxOverlapCount = 16;

        //Sweep Test 무시 값
        private const float SweepEdgeRejectDistance = 0.0015f;

        private const float MinDistanceToGround = 0.019f;
        private const float MaxDistanceToGround = 0.024f;
        private const float AvgDistanceToGround = (MinDistanceToGround + MaxDistanceToGround) * 0.5f;

        //걷기 가능한 경사 한계점
        private const float MinWalkableSlopeLimit = 1.000000f; //1 Radian(Degree로는 약 57도)
        private const float MaxWalkableSlopeLimit = 0.017452f; //0.017452 Radian(Degree로는 약 1도)

        //겹침 기준값
        private const float PenetrationOffset = 0.00125f;
        
        //충돌시 오프셋 값
        //일반적인 접촉 시 사용
        private const float ContactOffset = 0.01f;
        //미세 접촉 확인시 사용
        private const float SmallContactOffset = 0.001f;

        #endregion

		#region Inspector Properties

		[Header("Collider")]
        [SerializeField, Tooltip("캐릭터의 캡슐 컬라이더 반경")]
        private float radius;
        [SerializeField, Tooltip("캐릭터의 캡슐 컬라이더 높이")]
        private float height;
                
        [Header("Collision")]
        [SerializeField, Tooltip("캐릭터의 충돌 레이어")]
        private LayerMask collisionLayers = 1;
        
        [SerializeField, Tooltip("Trigger 객체들과의 충돌 체크 여부")]
        private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        
		[SerializeField, Tooltip("Dynamic Rigidbody 객체들과의 충돌 처리")]
		private bool enablePhysicsInteraction;
        [SerializeField, Tooltip("다른 캐릭터에게 밀려날 수 있는지 여부")]
        private bool allowPushed;

        [Header("Settings")]
        [SerializeField, Tooltip("걸어서 올라갈 수 있는 최대 각도(Degree)")]
        private float slopeLimit;

        [SerializeField, Tooltip("한 번 걸어서 올라갈 수 있는 최대 높이(Meter)")]
        private float stepOffset;

        [SerializeField, Tooltip("캐릭터가 가장자리에 서 있을 수 있는 허용치 값")]
        private float perchOffset;
        [Tooltip("Perching(가장자리에 서 있는) 상황에서 한 번에 올라갈 수 있는 최대 높이를 조정")]
        [SerializeField]
        private float perchAdditionalHeight;


        [Tooltip("상단 충돌을 체크할 때 사각형 기준으로 체크하기")]
        [SerializeField]
        private bool useFlatTop;

        [Tooltip("지면 접지를 체크할 때 사각형 기준으로 체크하기")]
        [SerializeField]
        private bool useFlatBaseForGroundChecks;

        [SerializeField, Tooltip("지면 충돌 체크를 간략화된 법선 체크로 처리")]
        bool useFastGeomNormalPath;

        [SerializeField, Tooltip("캐릭터의 특정 축 이동을 막기")]
        private AxisConstraint planeConstraint;


        [Header("Platform")]
        [SerializeField, Tooltip("탑승한 플랫폼의 이동 거리를 캐릭터 위치에 적용")]
        bool impartPlatformMovement;
		[SerializeField, Tooltip("탑승한 플랫폼의 회전을 캐릭터 회전에 적용")]
		bool impartPlatformRotation;
		[SerializeField, Tooltip("점프/추락할 때 캐릭터의 속력에 플랫폼 속력을 추가")]
		bool impartPlatformVelocity;

        [Space(5)]
        [Header("Etc Settings")]
        [SerializeField, Tooltip("최소 이동 거리")]
        private float minMoveDistance;
		[Tooltip("1회 이동당 Sweep Test 회수")]
		public int maxSweepIterations;
		[Tooltip("1회 이동당 겹침 상태 해결 처리 회수")]
		public int maxDepenetrationIterations;
        //private Advanced _advanced;

        #endregion


        #region Private Fields
        //Components
        private Transform _transform;
        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        
        //Capsule Collider의 상단/중심/하단 기준점 캐싱
        private Vector3 capsuleTop;
        private Vector3 capsuleCenter;
        private Vector3 capsuleBottom;
        //회전값 적용해서 캐싱
        private Vector3 transformedCapsuleCenter;
        private Vector3 transformedCapsuleTop;
        private Vector3 transformedCapsuleBottom;
        //현재 캐릭터 기준 Up Vector
        private Vector3 characterUp;
        //이동 방향 제한 Vector 캐싱
        private Vector3 constraintPlaneNormal;
        //최근 프레임의 이동 가능 플래그 캐싱
        public CollisionFlags collisionFlags { get; private set; }
        //

        //충돌 거부 대상을 저장
        private readonly HashSet<Rigidbody> ignoredRigidbodies = new HashSet<Rigidbody>();
        private readonly HashSet<Collider> ignoredColliders = new HashSet<Collider>();
        //
        
        //충돌 결과를 캐싱
        private readonly RaycastHit[] rayHits = new RaycastHit[MaxCollisionCount];
        private readonly Collider[] overlaps = new Collider[MaxOverlapCount];
        private int currentCollisionCount;
        private readonly CollisionResult[] collisionResults = new CollisionResult[MaxCollisionCount];
        //

        //현재 캐릭터의 속도와 외부에서 가해지는 힘
        private Vector3 velocity;
        
        private float forceScale = 1.0f;
        private Vector3 pendingForces;
        private Vector3 pendingImpulses;
        private Vector3 pendingLaunchVelocity;
        //

        //지면 접지
        private bool isLanded;
        private FindGroundResult foundGround;
        private FindGroundResult currentGround;
        //
        private float minSlopeLimit;

        private bool constrainedToGround = true;
        private float unconstrainedTimer;
        //

        //특정 Rigidbody와 함께 이동
        //접지할 플랫폼
        private Rigidbody parentPlatform;
        private MovingPlatform movingPlatform;
        
        private Vector3 lastVelocityOfPlatform;
        //

        #endregion

        #region Properties        
        /// <summary>
        /// 캐싱된 캐릭터 Transform
        /// </summary>
        public new Transform transform
        {
            get
            {
                #if UNITY_EDITOR
                if (_transform == null)
                    _transform = GetComponent<Transform>();
                #endif

                return _transform;
            }
        }

        /// <summary>
        /// 캐싱된 캐릭터 Rigidbody
        /// </summary>
        public new Rigidbody rigidbody
        {
            get
            {
                #if UNITY_EDITOR
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();
                #endif
                return _rigidbody;
            }
        }

        /// <summary>
        /// 객체의 CapsuleCollider(Collider로 얻어오기)
        /// </summary>
        public new Collider collider
        {
            get
            {
                #if UNITY_EDITOR
                if (_capsuleCollider == null)
                    _capsuleCollider = GetComponent<CapsuleCollider>();
                #endif
                return _capsuleCollider;
            }
        }
        /// <summary>
        /// 객체의 CapsuleCollider(CapsuleCollider로 얻어오기)
        /// </summary>
        public CapsuleCollider capsuleCollider
        {
            get
            {
                #if UNITY_EDITOR
                if (_capsuleCollider == null)
                    _capsuleCollider = GetComponent<CapsuleCollider>();
                #endif
                return _capsuleCollider;                
            }
        }
        /// <summary>
        /// 캐릭터의 현재 위치       
        /// </summary>
        public Vector3 Position
        {
            get => transform.position;
            set => SetPosition(value);
        }

        /// <summary>
        /// 캐릭터의 현재 위치와 지면으로부터의 평균 높이로 발 위치 추론
        /// </summary>
        public Vector3 FootPosition 
            => Position - (transform.up * AvgDistanceToGround);

        /// <summary>
        /// 캐릭터의 현재 회전값
        /// </summary>
        public Quaternion Rotation
        {
            get => transform.rotation;
            set => SetRotation(value);
        }

        /// <summary>
        /// 캐릭터의 충돌 중심점
        /// </summary>
        public Vector3 CharacterCenter => Position + Rotation * capsuleCenter;

        /// <summary>
        /// The character's updated position.
        /// </summary>
        public Vector3 updatedPosition { get; private set; }

        /// <summary>
        /// The character's updated rotation.
        /// </summary>
        public Quaternion updatedRotation { get; private set; }

        public ref Vector3 Velocity => ref velocity;
        public float Speed => velocity.magnitude;

        public float Radius
        {
            get => radius;
            set => SetDimensions(value, height);
        }
        public float Height
        {
            get => height;
            set => SetDimensions(radius, value);
        }

        public float SlopeLimit
        {
            get => slopeLimit;

            set
            {
                slopeLimit = Mathf.Clamp(value, 0.0f, 89.0f);

                minSlopeLimit = Mathf.Cos((slopeLimit + SmallNumber) * Mathf.Deg2Rad);
            }
        }

        public float StepOffset
        {
            get => stepOffset;
            set => stepOffset = Mathf.Max(0.0f, value);
        }

        public float PerchOffset
        {
            get => perchOffset;
            set => perchOffset = Mathf.Clamp(value, 0.0f, radius);
        }

        public float PerchAdditionalHeight
        {
            get => perchAdditionalHeight;
            set => perchAdditionalHeight = Mathf.Max(0.0f, value);
        }

        public bool UseFlatTop
        {
            get => useFlatTop;
            set => useFlatTop = value;
        }
        public bool UseFlatBaseForGroundChecks
        {
            get => useFlatBaseForGroundChecks;
            set => useFlatBaseForGroundChecks = value;
        }

        public LayerMask CollisionLayers
        {
            get => collisionLayers;
            set => collisionLayers = value;
        }

        public QueryTriggerInteraction TriggerInteraction
        {
            get => triggerInteraction;
            set => triggerInteraction = value;
        }

        public bool DetectCollisions
        {
            get => capsuleCollider.enabled;
            set
            {
                _capsuleCollider.enabled = value;
            }
        } 


        //
        /// <summary>
        /// 특정 방향으로의 이동이 막혀있는 상태인가?
        /// </summary>
        public bool IsConstrainedToPlane => planeConstraint != AxisConstraint.None;
        /// <summary>
        /// 지면 접지가 활성화된 상태인가?
        /// </summary>
        public bool ConstrainToGround
        {
            get => constrainedToGround;
            set => constrainedToGround = value;
        }

        /// <summary>
        /// 지면에 닿아있고 지면 접지가 일시정지되지 않은 상황인가?
        /// </summary>
        public bool IsConstrainedToGround => constrainedToGround && unconstrainedTimer == 0.0f;
        
        /// <summary>
        /// 지면 접지의 일시정지 종료까지 남은 시간
        /// </summary>
        public float UnconstrainedTimer => unconstrainedTimer;

        /// <summary>
        /// 마지막으로 Move 메서드를 사용했을 때 지면에 닿아있었나?
        /// </summary>
        public bool WasOnGround { get; private set; }

        /// <summary>
        /// 지금 캐릭터가 지면에 닿아있는가?
        /// </summary>
        public bool IsOnGround => currentGround.hitGround;

        /// <summary>
        /// 마지막으로 Move 메서드를 사용했을 때 이동 가능한 지면에 있었는가?
        /// </summary>
        public bool wasOnWalkableGround { get; private set; }

        /// <summary>
        /// 지금 이동 가능한 지면 위에 있는가?
        /// </summary>
        public bool isOnWalkableGround => currentGround.isWalkableGround;

        /// <summary>
        /// 마지막으로 Move 메서드를 사용했을 때 걸을 수 있는 지면 위에 있는 상황에서 지면 접지가 활성화됐었는가?
        /// </summary>

        public bool wasGrounded { get; private set; }

        /// <summary>
        /// 걸을 수 있는 지면 위에 있는 상황에서 지면 접지가 활성화됐는가?
        /// </summary>

        public bool IsGrounded => isOnWalkableGround && IsConstrainedToGround;

        /// <summary>
        /// 현재 지면과의 접지점
        /// </summary>
        public Vector3 GroundPoint => currentGround.point;

        /// <summary>
        /// 현재 지면의 법선 벡터
        /// </summary>

        public Vector3 GroundNormal => currentGround.normal;

        /// <summary>
        /// 현재 지면의 표면 법선 벡터
        /// </summary>
        public Vector3 GroundSurfaceNormal => currentGround.surfaceNormal;

        /// <summary>
        /// 현재 지면의 Collider
        /// </summary>

        public Collider GroundCollider => currentGround.collider;

        /// <summary>
        /// 현재 지면의 Transform
        /// </summary>

        public Transform GroundTransform => currentGround.transform;

        /// <summary>
        /// 현재 지면의 Rigidbody
        /// </summary>

        public Rigidbody GroundRigidbody => currentGround.rigidbody;

        /// <summary>
        /// 현재 캐릭터의 아래에 위치한 지면의 값을 저장하는 구조체
        /// </summary>
        public FindGroundResult CurrentGround => currentGround;

        /// <summary>
        /// 현재 캐릭터가 딛고 서 있는 플랫폼의 정보를 저장하는 구조체
        /// </summary>
        public MovingPlatform MovingPlatform => movingPlatform;

        /// <summary>
        /// 지면에 도착하면서 발생한 속도
        /// </summary>
        public Vector3 LandedVelocity { get; private set; }

        /// <summary>
        /// 플랫폼 위에 있을 때 주변 충돌처리 비활성화
        /// ※주변에 장애물이 없는 게 확실할 때 사용할 것
        /// </summary>

        public bool fastPlatformMove { get; set; }

        /// <summary>
        /// 탑승한 플랫폼의 이동 거리를 캐릭터 위치에 적용
        /// </summary>

        public bool ImpartPlatformMovement
        {
            get => impartPlatformMovement;
            set => impartPlatformMovement = value;
        }
        /// <summary>
        /// 탑승한 플랫폼의 회전을 캐릭터 회전에 적용
        /// </summary>

        public bool ImpartPlatformRotation
        {
            get => impartPlatformRotation;
            set => impartPlatformRotation = value;
        }

        /// <summary>
        /// 점프/추락할 때 캐릭터의 속력에 플랫폼 속력을 추가
        /// </summary>

        public bool ImpartPlatformVelocity
        {
            get => impartPlatformVelocity;
            set => impartPlatformVelocity = value;
        }

        /// <summary>
        /// 이동할 때 다른 Dynamic Rigidbody들과 충돌 처리
        /// </summary>

        public bool EnablePhysicsInteraction
        {
            get => enablePhysicsInteraction;
            set => enablePhysicsInteraction = value;
        }

        /// <summary>
        /// 다른 캐릭터들에게 밀려날 수 있는가?
        /// </summary>

        public bool AllowPushed
        {
            get => allowPushed;
            set => allowPushed = value;
        }

        /// <summary>
        /// 가해지는 힘의 계수
        /// </summary>
        public float ForceScale
        {
            get => forceScale;
            set => forceScale = Mathf.Max(0.0f, value);
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// 캐릭터가 Collider와 충돌할 지 여부를 결정하는 외부 Callback
        /// </summary>        
        public Func<Collider, bool> ColliderFilterCallback;
        
        /// <summary>
        /// 충돌한 지점의 이동 가능성 여부를 확인하는 외부 Callback
        /// </summary>
        public Func<Collider, MobilityFlags> CheckMobilityCallback;

        #endregion

        #region Events

        public delegate void CollidedEventHandler(ref CollisionResult collisionResult);
        public delegate void FoundGroundEventHandler(ref FindGroundResult foundGround);
        

        /// <summary>
        /// 이동 중 무언가와 충돌했을 때 호출되는 이벤트
        /// </summary>
        public event CollidedEventHandler Collided;
        /// <summary>
        /// Sweep 테스트 과정에서 지면을 찾았다면 호출되는 이벤트
        /// </summary>
        public event FoundGroundEventHandler FoundGround;

        /// <summary>
        /// 충돌 이벤트 발생시키기
        /// </summary>
        private void OnCollided()
        {
            if (Collided == null)
                return;

            for (int i = 0; i < currentCollisionCount; i++)
                Collided.Invoke(ref collisionResults[i]);
        }

        /// <summary>
        /// 지면 발견 이벤트 발생시키기
        /// </summary>
        private void OnFoundGround()
        {
            FoundGround?.Invoke(ref currentGround);
        }
        #endregion

        #region Calculate Geometry Normal Vector
        private Vector3 FindOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            const float kThickness = (ContactOffset - SweepEdgeRejectDistance) * 0.5f;

            Vector3 result = inHit.normal;
            
            Vector3 rayOrigin = inHit.point - sweepDirDenorm;
            
            float rayLength = sweepDirDenorm.magnitude * 2f;
            Vector3 rayDirection = sweepDirDenorm / sweepDirDenorm.magnitude;

            if (Raycast(rayOrigin, rayDirection, rayLength, collisionLayers, out RaycastHit hitResult, kThickness))
                result = hitResult.normal;

            return result;
        }

        private static Vector3 FindBoxOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            Transform localToWorld = inHit.transform;

            Vector3 localContactNormal = localToWorld.InverseTransformDirection(inHit.normal);
            Vector3 localTraceDirDenorm = localToWorld.InverseTransformDirection(sweepDirDenorm);

            Vector3 bestLocalNormal = localContactNormal;
            float bestOpposingDot = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                if (localContactNormal[i] > SmallNumber)
                {
                    float traceDotFaceNormal = localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = 1.0f;
                    }
                }
                else if (localContactNormal[i] < -SmallNumber)
                {
                    float traceDotFaceNormal = -localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = -1.0f;
                    }
                }
            }

            return localToWorld.TransformDirection(bestLocalNormal);
        }

        private static Vector3 FindBoxOpposingNormal(Vector3 displacement, Vector3 hitNormal, Transform hitTransform)
        {
            Transform localToWorld = hitTransform;

            Vector3 localContactNormal = localToWorld.InverseTransformDirection(hitNormal);
            Vector3 localTraceDirDenorm = localToWorld.InverseTransformDirection(displacement);

            Vector3 bestLocalNormal = localContactNormal;
            float bestOpposingDot = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                if (localContactNormal[i] > SmallNumber)
                {
                    float traceDotFaceNormal = localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = 1.0f;
                    }
                }
                else if (localContactNormal[i] < -SmallNumber)
                {
                    float traceDotFaceNormal = -localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = -1.0f;
                    }
                }
            }

            return localToWorld.TransformDirection(bestLocalNormal);
        }

        private static Vector3 FindTerrainOpposingNormal(ref RaycastHit inHit)
        {
            TerrainCollider terrainCollider = inHit.collider as TerrainCollider;

            if (terrainCollider != null)
            {
                Vector3 localPoint = terrainCollider.transform.InverseTransformPoint(inHit.point);

                TerrainData terrainData = terrainCollider.terrainData;

                Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal(localPoint.x / terrainData.size.x,
                    localPoint.z / terrainData.size.z);

                return interpolatedNormal;
            }

            return inHit.normal;
        }

        /// <summary>
        /// Helper method to retrieve real surface normal, usually the most 'opposing' to sweep direction.
        /// </summary>
        
        private Vector3 FindGeomOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            // SphereCollider or CapsuleCollider

            if (inHit.collider is SphereCollider _ || inHit.collider is CapsuleCollider _)
            {
                // We don't compute anything special, inHit.normal is the correct one.

                return inHit.normal;
            }

            // BoxCollider

            if (inHit.collider is BoxCollider _)
            {
                return FindBoxOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Non-Convex MeshCollider (MUST BE read / write enabled!)

            if (inHit.collider is MeshCollider nonConvexMeshCollider && !nonConvexMeshCollider.convex)
            {
                Mesh sharedMesh = nonConvexMeshCollider.sharedMesh;
                if (sharedMesh && sharedMesh.isReadable && !useFastGeomNormalPath)
                    return MeshUtility.FindMeshOpposingNormal(sharedMesh, ref inHit);

                // No read / write enabled, fallback to a raycast...

                return FindOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Convex MeshCollider

            if (inHit.collider is MeshCollider convexMeshCollider && convexMeshCollider.convex)
            {
                // No data exposed by Unity to compute normal. Fallback to a raycast...

                return FindOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Terrain collider
            
            if (inHit.collider is TerrainCollider && !useFastGeomNormalPath)
            {
                return FindTerrainOpposingNormal(ref inHit);
            }
            
            return inHit.normal;
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// 입력된 속력에 마찰과 감속을 계산해서 리턴
        /// </summary>
        /// <param name="currentVelocity">속력</param>
        /// <param name="friction">마찰 계수</param>
        /// <param name="deceleration">감속도, 진행 방향의 역으로 가해지는 힘의 양</param>
        /// <param name="deltaTime">DeltaTime</param>
        /// <returns>감속 계산이 적용된 속력</returns>
        private static Vector3 ApplyVelocityBraking(Vector3 currentVelocity, float friction, float deceleration, float deltaTime)
        {
            // 마찰도 감속도 없다면 그냥 return
            bool isZeroFriction = friction == 0.0f;
            bool isZeroBraking = deceleration == 0.0f;

            if (isZeroFriction && isZeroBraking)
                return currentVelocity;
            
            Vector3 oldVel = currentVelocity;
            
            //감속도 계산
            Vector3 revAcceleration = isZeroBraking ? Vector3.zero : -deceleration * currentVelocity.normalized;

            // 마찰과 감속 적용
            currentVelocity += (-friction * currentVelocity + revAcceleration) * deltaTime;

            //방향이 반전될 정도라면 0을 return
            if (Vector3.Dot(currentVelocity, oldVel) <= 0.0f)
                return Vector3.zero;

            // 감속된 속도가 지나치게 작다면 0을 return
            float sqrSpeed = currentVelocity.sqrMagnitude;
            if (sqrSpeed <= 0.00001f || !isZeroBraking && sqrSpeed <= 0.01f)
                return Vector3.zero;

            return currentVelocity;
        }

        /// <summary>
        /// Determines how far is the desiredVelocity from maximum speed.
        /// </summary>
        /// <param name="desiredVelocity">The target velocity.</param>
        /// <param name="maxSpeed">The maximum allowed speed.</param>
        /// <returns>Returns the analog input modifier in the 0 - 1 range.</returns>
        private static float ComputeAnalogInputModifier(Vector3 desiredVelocity, float maxSpeed)
        {
            if (maxSpeed > 0.0f && desiredVelocity.sqrMagnitude > 0.0f)
                return Mathf.Clamp01(desiredVelocity.magnitude / maxSpeed);

            return 0.0f;
        }

        /// <summary>
        /// Calculates a new velocity for the given state, applying the effects of friction or braking friction and acceleration or deceleration.
        /// </summary>
        /// <param name="currentVelocity">Character's current velocity.</param>
        /// <param name="desiredVelocity">Target velocity</param>
        /// <param name="maxSpeed">The maximum speed when grounded. Also determines maximum horizontal speed when falling (i.e. not-grounded).</param>
        /// <param name="acceleration">The rate of change of velocity when accelerating (i.e desiredVelocity != Vector3.zero).</param>
        /// <param name="deceleration">The rate at which the character slows down when braking (i.e. not accelerating or if character is exceeding max speed).
        /// This is a constant opposing force that directly lowers velocity by a constant value.</param>
        /// <param name="friction">Setting that affects movement control. Higher values allow faster changes in direction.</param>
        /// <param name="brakingFriction">Friction (drag) coefficient applied when braking (whenever desiredVelocity == Vector3.zero, or if character is exceeding max speed).</param>
        /// <param name="deltaTime">The simulation deltaTime. Defaults to Time.deltaTime.</param>
        /// <returns>Returns the updated velocity</returns>
        private static Vector3 CalcVelocity(Vector3 currentVelocity, Vector3 desiredVelocity, float maxSpeed,
            float acceleration, float deceleration, float friction, float brakingFriction, float deltaTime)
        {
            // Compute requested move direction

            float desiredSpeed = desiredVelocity.magnitude;
            Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

            // Requested acceleration (factoring analog input)

            float analogInputModifier = ComputeAnalogInputModifier(desiredVelocity, maxSpeed);
            Vector3 requestedAcceleration = acceleration * analogInputModifier * desiredMoveDirection;

            // Actual max speed (factoring analog input)

            float actualMaxSpeed = Mathf.Max(0.0f, maxSpeed * analogInputModifier);

            // Friction
            // Only apply braking if there is no input acceleration,
            // or we are over our max speed and need to slow down to it

            bool isZeroAcceleration = requestedAcceleration.IsZero();
            bool isVelocityOverMax = currentVelocity.IsExceeding(actualMaxSpeed);

            if (isZeroAcceleration || isVelocityOverMax)
            {
                // Pre-braking currentVelocity

                Vector3 oldVelocity = currentVelocity;

                // Apply friction and braking

                currentVelocity = ApplyVelocityBraking(currentVelocity, brakingFriction, deceleration, deltaTime);

                // Don't allow braking to lower us below max speed if we started above it

                if (isVelocityOverMax && currentVelocity.sqrMagnitude < actualMaxSpeed.Square() &&
                    Vector3.Dot(requestedAcceleration, oldVelocity) > 0.0f)
                    currentVelocity = oldVelocity.normalized * actualMaxSpeed;
            }
            else
            {
                // Friction, this affects our ability to change direction

                currentVelocity -= (currentVelocity - desiredMoveDirection * currentVelocity.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
            }

            // Apply acceleration

            if (!isZeroAcceleration)
            {
                float newMaxSpeed = currentVelocity.IsExceeding(actualMaxSpeed) ? currentVelocity.magnitude : actualMaxSpeed;

                currentVelocity += requestedAcceleration * deltaTime;
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, newMaxSpeed);
            }

            // Return new velocity

            return currentVelocity;
        }

        /// <summary>
        /// Helper method to get the velocity of the rigidbody at the worldPoint,
        /// will take the angularVelocity of the rigidbody into account when calculating the velocity.
        /// If the given Rigidbody is a character, will return character's velocity.
        /// </summary>
        private static Vector3 GetRigidbodyVelocity(Rigidbody rigidbody, Vector3 worldPoint)
        {
            if (rigidbody == null)
                return Vector3.zero;

            return rigidbody.TryGetComponent(out CharacterPhysics controller)
                ? controller.Velocity
                : rigidbody.GetPointVelocity(worldPoint);
        }

        /// <summary>
        /// Helper function to create a capsule of given dimensions.
        /// </summary>
        /// <param name="radius">The capsule radius.</param>
        /// <param name="height">The capsule height.</param>
        /// <param name="center">Output capsule center in local space.</param>
        /// <param name="bottomCenter">Output capsule bottom sphere center in local space.</param>
        /// <param name="topCenter">Output capsule top sphere center in local space.</param>

        private static void MakeCapsule(float radius, float height, out Vector3 center, out Vector3 bottomCenter, out Vector3 topCenter)
        {
            radius = Mathf.Max(radius, 0.0f);
            height = Mathf.Max(height, radius * 2.0f);

            center = height * 0.5f * Vector3.up;

            float sideHeight = height - radius * 2.0f;

            bottomCenter = center - sideHeight * 0.5f * Vector3.up;
            topCenter = center + sideHeight * 0.5f * Vector3.up;
        }

        /// <summary>
        /// 캐릭터 캡슐 컬라이더의 Volume을 재설정
        /// </summary>
        /// <param name="characterRadius">캐릭터의 새로운 반경</param>
        /// <param name="characterHeight">캐릭터의 새로운 높이</param>

        public void SetDimensions(float characterRadius, float characterHeight)
        {
            radius = Mathf.Max(characterRadius, 0.0f);
            height = Mathf.Max(characterHeight, characterRadius * 2.0f);

            MakeCapsule(radius, height, out capsuleCenter, out capsuleBottom, out capsuleTop);

#if UNITY_EDITOR
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
#endif

            if (_capsuleCollider)
            {
                _capsuleCollider.radius = radius;
                _capsuleCollider.height = height;
                _capsuleCollider.center = capsuleCenter;
            }
        }

        /// <summary>
        /// 캐릭터의 높이를 재설정
        /// </summary>
        /// <param name="characterHeight">캐릭터의 새로운 높이</param>

        public void SetHeight(float characterHeight)
        {
            height = Mathf.Max(characterHeight, 0.1f);

            MakeCapsule(radius, height, out capsuleCenter, out capsuleBottom, out capsuleTop);

#if UNITY_EDITOR
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
#endif

            if (_capsuleCollider)
            {
                _capsuleCollider.height = height;
                _capsuleCollider.center = capsuleCenter;
            }
        }

        /// <summary>
        /// 컴퍼넌트를 캐싱
        /// </summary>

        private void CacheComponents()
        {
            _transform = GetComponent<Transform>();

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody)
            {
                _rigidbody.drag = 0.0f;
                _rigidbody.angularDrag = 0.0f;

                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }

            _capsuleCollider = GetComponent<CapsuleCollider>();
        }
        
        /// <summary>
        /// Current plane constraint normal.
        /// </summary>

        public Vector3 GetPlaneConstraintNormal()
        {
            return constraintPlaneNormal;
        }

        /// <summary>
        /// Defines the axis that constraints movement, so movement along the given axis is not possible.
        /// </summary>

        public void SetPlaneConstraint(AxisConstraint constrainAxis)
        {
            planeConstraint = constrainAxis;

            switch (planeConstraint)
            {
                case AxisConstraint.None:
                    {
                        constraintPlaneNormal = Vector3.zero;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.None;

                        break;
                    }

                case AxisConstraint.ConstrainXAxis:
                    {
                        constraintPlaneNormal = Vector3.right;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;

                        break;
                    }

                case AxisConstraint.ConstrainYAxis:
                    {
                        constraintPlaneNormal = Vector3.up;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY;

                        break;
                    }

                case AxisConstraint.ConstrainZAxis:
                    {
                        constraintPlaneNormal = Vector3.forward;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns the given DIRECTION (Normalized) vector constrained to current constraint plane (if _constrainToPlane != None)
        /// or given vector (if _constrainToPlane == None).
        /// </summary>

        public Vector3 ConstrainDirectionToPlane(Vector3 direction)
        {
            return ConstrainVectorToPlane(direction).normalized;
        }

        /// <summary>
        /// Constrain the given vector to current PlaneConstraint (if any).
        /// </summary>
        public Vector3 ConstrainVectorToPlane(Vector3 vector)
        {
            return IsConstrainedToPlane ? vector.ProjectOnPlane(constraintPlaneNormal) : vector;
        }

        /// <summary>
        /// Append HitLocation to current CollisionFlags.
        /// </summary>
        private void UpdateCollisionFlags(HitLocation hitLocation)
        {
            collisionFlags |= (CollisionFlags) hitLocation;
        }

        /// <summary>
        /// Determines the hit location WRT capsule for the given normal.
        /// </summary>

        private HitLocation ComputeHitLocation(Vector3 inNormal)
        {
            float verticalComponent = inNormal.Dot(characterUp);

            if (verticalComponent > HemisphereLimit)
                return HitLocation.Below;

            return verticalComponent < -HemisphereLimit ? HitLocation.Above : HitLocation.Sides;
        }

        /// <summary>
        /// Determines if the given collider and impact normal should be considered as walkable ground.
        /// </summary>
        
        private bool IsWalkable(Collider inCollider, Vector3 inNormal)
        {
            // Do not bother if hit is not in capsule bottom sphere

            if (ComputeHitLocation(inNormal) != HitLocation.Below)
                return false;

            // If collision behaviour callback is assigned, check walkable / not walkable flags

            if (CheckMobilityCallback != null)
            {
                MobilityFlags collisionBehaviour = CheckMobilityCallback.Invoke(inCollider);

                if (collisionBehaviour.HasFlag(MobilityFlags.Walkable))
                    return Vector3.Dot(inNormal, characterUp) > MaxWalkableSlopeLimit;

                if (collisionBehaviour.HasFlag(MobilityFlags.NotWalkable))
                    return Vector3.Dot(inNormal, characterUp) > MinWalkableSlopeLimit;
            }

            // Determine if the given normal is walkable
            return Vector3.Dot(inNormal, characterUp) > minSlopeLimit;
        }

        /// <summary>
        /// When moving on walkable ground, and hit a non-walkable, modify hit normal (eg: the blocking hit normal)
        /// since We don't want to be pushed up an unwalkable surface,
        /// or be pushed down into the ground when the impact is on the upper portion of the capsule.
        /// </summary>

        private Vector3 ComputeBlockingNormal(Vector3 inNormal, bool isWalkable)
        {
            if ((IsGrounded || isLanded) && !isWalkable)
            {
                Vector3 actualGroundNormal = isLanded ? foundGround.normal : currentGround.normal;

                Vector3 forward = actualGroundNormal.PerpendicularTo(inNormal);
                Vector3 blockingNormal = forward.PerpendicularTo(characterUp);

                if (Vector3.Dot(blockingNormal, inNormal) < 0.0f)
                    blockingNormal = -blockingNormal;

                if (!blockingNormal.IsZero())
                    inNormal = blockingNormal;

                return inNormal;
            }

            return inNormal;

        }

        /// <summary>
        /// Determines if the given collider should be filtered (ignored) or not.
        /// Return true to filter collider (e.g. Ignore it), false otherwise.
        /// </summary>

        private bool ShouldFilter(Collider otherCollider)
        {
            if (otherCollider == _capsuleCollider || otherCollider.attachedRigidbody == rigidbody)
                return true;
            
            if (ignoredColliders.Contains(otherCollider))
                return true;

            Rigidbody attachedRigidbody = otherCollider.attachedRigidbody;
            if (attachedRigidbody && ignoredRigidbodies.Contains(attachedRigidbody))
                return true;
            
            return ColliderFilterCallback != null && ColliderFilterCallback.Invoke(otherCollider);
        }

        /// <summary>
        /// Makes the character's collider (eg: CapsuleCollider) to ignore all collisions vs otherCollider.
        /// NOTE: The character can still collide with other during a Move call if otherCollider is in CollisionLayers mask.
        /// </summary>

        public void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            if (otherCollider == null)
                return;

            UnityEngine.Physics.IgnoreCollision(_capsuleCollider, otherCollider, ignore);
        }

        /// <summary>
        /// Makes the character to ignore all collisions vs otherCollider.
        /// </summary>

        public void IgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            if (otherCollider == null)
                return;

            if (ignore)
                ignoredColliders.Add(otherCollider);
            else
                ignoredColliders.Remove(otherCollider);
        }

        /// <summary>
        /// Makes the character to ignore collisions vs all colliders attached to the otherRigidbody.
        /// </summary>

        public void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
        {
            if (otherRigidbody == null)
                return;

            if (ignore)
                ignoredRigidbodies.Add(otherRigidbody);
            else
                ignoredRigidbodies.Remove(otherRigidbody);
        }

        /// <summary>
        /// Clear last Move collision results.
        /// </summary>

        private void ClearCollisionResults()
        {
            currentCollisionCount = 0;
        }

        /// <summary>
        /// Add a CollisionResult to collisions list found during Move.
        /// If CollisionResult is vs otherRigidbody add first one only.
        /// </summary>

        private void AddCollisionResult(ref CollisionResult collisionResult)
        {
            
            collisionFlags |= (CollisionFlags)collisionResult.hitLocation;

            if (collisionResult.rigidbody)
            {
                // Do not process as dynamic collisions, any collision against current riding platform

                if (collisionResult.rigidbody == movingPlatform.platform)
                    return;

                // We only care about the first collision with a rigidbody

                for (int i = 0; i < currentCollisionCount; i++)
                {
                    if (collisionResult.rigidbody == collisionResults[i].rigidbody)
                        return;
                }
            }

            if (currentCollisionCount < MaxCollisionCount)
                collisionResults[currentCollisionCount++] = collisionResult;
        }

        /// <summary>
        /// Return the number of collisions found during last Move call.
        /// </summary>

        public int GetCollisionCount()
        {
            return currentCollisionCount;
        }

        /// <summary>
        /// Retrieves a CollisionResult from last Move call list.
        /// </summary>

        public CollisionResult GetCollisionResult(int index)
        {
            return collisionResults[index];
        }

        /// <summary>
        /// Compute the minimal translation distance (MTD) required to separate the given colliders apart at specified poses.
        /// Uses an inflated capsule for better results.
        /// </summary
        private bool ComputeInflatedMTD(Vector3 characterPosition, Quaternion characterRotation, float mtdInflation,
            Collider hitCollider, Transform hitTransform, out Vector3 mtdDirection, out float mtdDistance)
        {
            mtdDirection = Vector3.zero;
            mtdDistance = 0.0f;

            _capsuleCollider.radius = radius + mtdInflation * 1.0f;
            _capsuleCollider.height = height + mtdInflation * 2.0f;

            bool mtdResult = UnityEngine.Physics.ComputePenetration(_capsuleCollider, characterPosition, characterRotation,
                hitCollider, hitTransform.position, hitTransform.rotation, out Vector3 recoverDirection, out float recoverDistance);

            if (mtdResult)
            {
                if (recoverDirection.IsFinite())
                {
                    mtdDirection = recoverDirection;
                    mtdDistance = Mathf.Max(Mathf.Abs(recoverDistance) - mtdInflation, 0.0f) + SmallNumber;
                }
                else
                {
                    Debug.LogWarning($"Warning: ComputeInflatedMTD_Internal: MTD returned NaN " + recoverDirection.ToString("F4"));
                }
            }

            _capsuleCollider.radius = radius;
            _capsuleCollider.height = height;

            return mtdResult;
        }

        /// <summary>
        /// Compute the minimal translation distance (MTD) required to separate the given colliders apart at specified poses.
        /// Uses an inflated capsule for better results, try MTD with a small inflation for better accuracy, then a larger one in case the first one fails due to precision issues.
        /// </summary>

        private bool ComputeMTD(Vector3 characterPosition, Quaternion characterRotation, Collider hitCollider, Transform hitTransform, out Vector3 mtdDirection, out float mtdDistance)
        {
            const float kSmallMTDInflation = 0.0025f;
            const float kLargeMTDInflation = 0.0175f;

            if (ComputeInflatedMTD(characterPosition, characterRotation, kSmallMTDInflation, hitCollider, hitTransform, out mtdDirection, out mtdDistance) ||
                ComputeInflatedMTD(characterPosition, characterRotation, kLargeMTDInflation, hitCollider, hitTransform, out mtdDirection, out mtdDistance))
            {
                // Success

                return true;
            }

            // Failure

            return false;
        }

        /// <summary>
        /// Resolves any character's volume overlaps against specified colliders.
        /// </summary>

        private void ResolveOverlaps(DepenetrationBehaviour depenetrationBehaviour = DepenetrationBehaviour.IgnoreNone)
        {
            if (!DetectCollisions)
                return;

            bool ignoreStatic = (depenetrationBehaviour & DepenetrationBehaviour.IgnoreStatic) != 0;
            bool ignoreDynamic = (depenetrationBehaviour & DepenetrationBehaviour.IgnoreDynamic) != 0;
            bool ignoreKinematic = (depenetrationBehaviour & DepenetrationBehaviour.IgnoreKinematic) != 0;

            for (int i = 0; i < maxDepenetrationIterations; i++)
            {
                Vector3 top = updatedPosition + transformedCapsuleTop;
                Vector3 bottom = updatedPosition + transformedCapsuleBottom;

                int overlapCount = UnityEngine.Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlaps, collisionLayers, TriggerInteraction);
                if (overlapCount == 0)
                    break;

                for (int j = 0; j < overlapCount; j++)
                {
                    Collider overlappedCollider = overlaps[j];

                    if (ShouldFilter(overlappedCollider))
                        continue;

                    Rigidbody attachedRigidbody = overlappedCollider.attachedRigidbody;

                    if (ignoreStatic && attachedRigidbody == null)
                        continue;

                    if (attachedRigidbody)
                    {
                        bool isKinematic = attachedRigidbody.isKinematic;

                        if (ignoreKinematic && isKinematic)
                            continue;

                        if (ignoreDynamic && !isKinematic)
                            continue;
                    }

                    if (ComputeMTD(updatedPosition, updatedRotation, overlappedCollider, overlappedCollider.transform, out Vector3 recoverDirection, out float recoverDistance))
                    {
                        recoverDirection = ConstrainDirectionToPlane(recoverDirection);

                        HitLocation hitLocation = ComputeHitLocation(recoverDirection);

                        bool isWalkable = IsWalkable(overlappedCollider, recoverDirection);

                        Vector3 impactNormal = ComputeBlockingNormal(recoverDirection, isWalkable);

                        updatedPosition += impactNormal * (recoverDistance + PenetrationOffset);

                        if (currentCollisionCount < MaxCollisionCount)
                        {
                            Vector3 point;

                            if (hitLocation == HitLocation.Above)
                                point = updatedPosition + transformedCapsuleTop - recoverDirection * radius;
                            else if (hitLocation == HitLocation.Below)
                                point = updatedPosition + transformedCapsuleBottom - recoverDirection * radius;
                            else
                                point = updatedPosition + transformedCapsuleCenter - recoverDirection * radius;

                            CollisionResult collisionResult = new CollisionResult
                            {
                                startPenetrating = true,

                                hitLocation = hitLocation,
                                isWalkable = isWalkable,

                                position = updatedPosition,

                                velocity = velocity,
                                otherVelocity = GetRigidbodyVelocity(attachedRigidbody, point),

                                point = point,
                                normal = impactNormal,

                                surfaceNormal = impactNormal,

                                collider = overlappedCollider
                            };

                            AddCollisionResult(ref collisionResult);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check the given capsule against the physics world and return all overlapping colliders.
        /// Return overlapped colliders count.
        /// </summary>

        public int OverlapTest(Vector3 characterPosition, Quaternion characterRotation, float testRadius,
            float testHeight, int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction)
        {
            MakeCapsule(testRadius, testHeight, out Vector3 _, out Vector3 bottomCenter, out Vector3 topCenter);

            Vector3 top = characterPosition + characterRotation * topCenter;
            Vector3 bottom = characterPosition + characterRotation * bottomCenter;

            int rawOverlapCount =
                UnityEngine.Physics.OverlapCapsuleNonAlloc(bottom, top, testRadius, results, layerMask, queryTriggerInteraction);

            if (rawOverlapCount == 0)
                return 0;

            int filteredOverlapCount = rawOverlapCount;

            for (int i = 0; i < rawOverlapCount; i++)
            {
                Collider overlappedCollider = results[i];

                if (ShouldFilter(overlappedCollider))
                {
                    if (i < --filteredOverlapCount)
                        results[i] = results[filteredOverlapCount];
                }
            }

            return filteredOverlapCount;
        }

        /// <summary>
        /// Check the given capsule against the physics world and return all overlapping colliders.
        /// Return an array of overlapped colliders.
        /// </summary>

        public Collider[] OverlapTest(Vector3 characterPosition, Quaternion characterRotation, float testRadius,
            float testHeight, int layerMask, QueryTriggerInteraction queryTriggerInteraction, out int overlapCount)
        {
            overlapCount = OverlapTest(characterPosition, characterRotation, testRadius, testHeight, layerMask,
                overlaps, queryTriggerInteraction);

            return overlaps;
        }
        
        /// <summary>
        /// Check the character's capsule against the physics world and return all overlapping colliders.
        /// Return an array of overlapped colliders.
        /// </summary>

        public Collider[] OverlapTest(int layerMask, QueryTriggerInteraction queryTriggerInteraction,
            out int overlapCount)
        {
            overlapCount = 
                OverlapTest(Position, Rotation, Radius, Height, layerMask, overlaps, queryTriggerInteraction);

            return overlaps;
        }
        
        /// <summary>
        /// Checks if any colliders overlaps the character's capsule-shaped volume in world space using testHeight as capsule's height.
        /// Returns true if there is a blocking overlap, false otherwise.
        /// </summary>

        public bool CheckCapsule()
        {
            IgnoreCollision(movingPlatform.platform);

            int overlapCount =
                OverlapTest(Position, Rotation, Radius, Height, CollisionLayers, overlaps, TriggerInteraction);

            IgnoreCollision(movingPlatform.platform, false);

            return overlapCount > 0;
        }

        /// <summary>
        /// Checks if any colliders overlaps the character's capsule-shaped volume in world space using testHeight as capsule's height.
        /// Returns true if there is a blocking overlap, false otherwise.
        /// </summary>

        public bool CheckHeight(float testHeight)
        {
            IgnoreCollision(movingPlatform.platform);

            int overlapCount =
                OverlapTest(Position, Rotation, Radius, testHeight, CollisionLayers, overlaps, TriggerInteraction);

            IgnoreCollision(movingPlatform.platform, false);

            return overlapCount > 0;
        }

        /// <summary>
        /// Return true if the 2D distance to the impact point is inside the edge tolerance (CapsuleRadius minus a small rejection threshold).
        /// Useful for rejecting adjacent hits when finding a ground or landing spot.
        /// </summary>
        
        public bool IsWithinEdgeTolerance(Vector3 characterPosition, Vector3 inPoint, float testRadius)
        {
            float distFromCenterSq = (inPoint - characterPosition).ProjectOnPlane(characterUp).sqrMagnitude;

            float reducedRadius = Mathf.Max(SweepEdgeRejectDistance + SmallNumber,
                testRadius - SweepEdgeRejectDistance);

            return distFromCenterSq < reducedRadius * reducedRadius;
        }

        /// <summary>
        /// Determine whether we should try to find a valid landing spot after an impact with an invalid one (based on the Hit result).
        /// For example, landing on the lower portion of the capsule on the edge of geometry may be a walkable surface, but could have reported an unwalkable surface normal.
        /// </summary>

        private bool ShouldCheckForValidLandingSpot(ref CollisionResult inCollision)
        {
            // See if we hit an edge of a surface on the lower portion of the capsule.
            // In this case the normal will not equal the surface normal, and a downward sweep may find a walkable surface on top of the edge.

            if (inCollision.hitLocation == HitLocation.Below && inCollision.normal != inCollision.surfaceNormal)
            {
                if (IsWithinEdgeTolerance(updatedPosition, inCollision.point, radius))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Verify that the supplied CollisionResult is a valid landing spot when falling.
        /// </summary>

        private bool IsValidLandingSpot(Vector3 characterPosition, ref CollisionResult inCollision)
        {
            // Reject unwalkable ground normals.

            if (!inCollision.isWalkable)
                return false;

            // Reject hits that are above our lower hemisphere (can happen when sliding down a vertical surface).

            if (inCollision.hitLocation != HitLocation.Below)
                return false;

            // Reject hits that are barely on the cusp of the radius of the capsule

            if (!IsWithinEdgeTolerance(characterPosition, inCollision.point, radius))
            {
                inCollision.isWalkable = false;

                return false;
            }

            FindGround(characterPosition, out FindGroundResult groundResult);
            {
                inCollision.isWalkable = groundResult.isWalkableGround;

                if (inCollision.isWalkable)
                {
                    foundGround = groundResult;

                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length distance, against specified colliders (by layerMask) in the Scene.
        /// </summary>
        
        public bool Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, out RaycastHit hitResult,
            float thickness = 0.0f)
        {
            hitResult = default;

            int rawHitCount = thickness == 0.0f
                ? UnityEngine.Physics.RaycastNonAlloc(origin, direction, rayHits, distance, layerMask, TriggerInteraction)
                : UnityEngine.Physics.SphereCastNonAlloc(origin - direction * thickness, thickness, direction, rayHits, distance, layerMask, TriggerInteraction);

            if (rawHitCount == 0)
                return false;
            
            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (hit.distance <= 0.0f || ShouldFilter(hit.collider))
                    continue;

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = rayHits[hitIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts a capsule against all colliders in the Scene and returns detailed information on what was hit.
        /// Returns True when the capsule sweep intersects any collider, otherwise false. 
        /// </summary>
        
        private bool CapsuleCast(Vector3 characterPosition, float castRadius, Vector3 castDirection, float castDistance,
            int layerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = false;

            Vector3 top = characterPosition + transformedCapsuleTop;
            Vector3 bottom = characterPosition + transformedCapsuleBottom;

            int rawHitCount = UnityEngine.Physics.CapsuleCastNonAlloc(bottom, top, castRadius, castDirection, rayHits, castDistance,
                layerMask, TriggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = rayHits[hitIndex];
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Sorts (asc) the given array by distance (insertion sort).
        /// </summary>        

        private static void SortArray(RaycastHit[] array, int length)
        {
            for (int i = 1; i < length; i++)
            {
                RaycastHit key = array[i];
                int flag = 0;

                for (int j = i - 1; j >= 0 && flag != 1;)
                {
                    if (key.distance < array[j].distance)
                    {
                        array[j + 1] = array[j];
                        j--;
                        array[j + 1] = key;
                    }
                    else flag = 1;
                }
            }
        }

        /// <summary>
        /// Casts a capsule against all colliders in the Scene and returns detailed information on what was hit.
        /// Returns True when the capsule sweep intersects any collider, otherwise false. 
        /// Unlike previous version this correctly return (if desired) valid hits for blocking overlaps along with MTD to resolve penetration.
        /// </summary>

        private bool CapsuleCastEx(Vector3 characterPosition, float castRadius, Vector3 castDirection, float castDistance, int layerMask,
            out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, bool ignoreNonBlockingOverlaps = false)
        {
            hitResult = default;

            startPenetrating = default;
            recoverDirection = default;
            recoverDistance = default;

            Vector3 top = characterPosition + transformedCapsuleTop;
            Vector3 bottom = characterPosition + transformedCapsuleBottom;

            int rawHitCount =
                UnityEngine.Physics.CapsuleCastNonAlloc(bottom, top, castRadius, castDirection, rayHits, castDistance, layerMask, TriggerInteraction);

            if (rawHitCount == 0)
                return false;

            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                bool isOverlapping = hit.distance <= 0.0f;
                if (isOverlapping)
                {
                    if (ComputeMTD(characterPosition, updatedRotation, hit.collider, hit.collider.transform, out Vector3 mtdDirection, out float mtdDistance))
                    {
                        mtdDirection = ConstrainDirectionToPlane(mtdDirection);

                        HitLocation hitLocation = ComputeHitLocation(mtdDirection);

                        Vector3 point;
                        if (hitLocation == HitLocation.Above)
                            point = characterPosition + transformedCapsuleTop - mtdDirection * radius;
                        else if (hitLocation == HitLocation.Below)
                            point = characterPosition + transformedCapsuleBottom - mtdDirection * radius;
                        else
                            point = characterPosition + transformedCapsuleCenter - mtdDirection * radius;

                        Vector3 impactNormal = ComputeBlockingNormal(mtdDirection, IsWalkable(hit.collider, mtdDirection));

                        hit.point = point;
                        hit.normal = impactNormal;
                        hit.distance = -mtdDistance;
                    }
                }
            }

            //@Deprecated, this caused memory allocations due the use of IComparer
            //Array.Sort(_hits, 0, rawHitCount, _hitComparer);

            if (rawHitCount > 2)
            {
                SortArray(rayHits, rawHitCount);
            }

            float mostOpposingDot = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                bool isOverlapping = hit.distance <= 0.0f && !hit.point.IsZero();
                if (isOverlapping)
                {
                    // Overlaps

                    float movementDotNormal = Vector3.Dot(castDirection, hit.normal);

                    if (ignoreNonBlockingOverlaps)
                    {
                        // If we started penetrating, we may want to ignore it if we are moving out of penetration.
                        // This helps prevent getting stuck in walls.

                        bool isMovingOut = movementDotNormal > 0.0f;
                        if (isMovingOut)
                            continue;
                    }
                    
                    if (movementDotNormal < mostOpposingDot)
                    {
                        mostOpposingDot = movementDotNormal;
                        hitIndex = i;
                    }
                }
                else if (hitIndex == -1)
                {
                    // Hits
                    // First non-overlapping blocking hit should be used, if no valid overlapping hit was found (ie, hitIndex == -1).

                    hitIndex = i;
                    break;
                }
            }

            if (hitIndex >= 0)
            {
                hitResult = rayHits[hitIndex];

                if (hitResult.distance <= 0.0f)
                {
                    startPenetrating = true;
                    recoverDirection = hitResult.normal;
                    recoverDistance = Mathf.Abs(hitResult.distance);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if the character would collide with anything, if it was moved through the Scene.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        private bool SweepTest(Vector3 sweepOrigin, float sweepRadius, Vector3 sweepDirection, float sweepDistance,
            int sweepLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            // Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
            // when moving almost parallel to an obstacle for small distances).

            hitResult = default;

            bool innerCapsuleHit =
                CapsuleCast(sweepOrigin, sweepRadius, sweepDirection, sweepDistance + sweepRadius, sweepLayerMask,
                    out RaycastHit innerCapsuleHitResult, out startPenetrating) && innerCapsuleHitResult.distance <= sweepDistance;

            float outerCapsuleRadius = sweepRadius + ContactOffset;

            bool outerCapsuleHit =
                CapsuleCast(sweepOrigin, outerCapsuleRadius, sweepDirection, sweepDistance + outerCapsuleRadius,
                    sweepLayerMask, out RaycastHit outerCapsuleHitResult, out _) && outerCapsuleHitResult.distance <= sweepDistance;

            bool foundBlockingHit = innerCapsuleHit || outerCapsuleHit;
            if (!foundBlockingHit)
                return false;

            if (!outerCapsuleHit)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - ContactOffset);
            }
            else if (innerCapsuleHit && innerCapsuleHitResult.distance < outerCapsuleHitResult.distance)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - ContactOffset);
            }
            else
            {
                hitResult = outerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - SmallContactOffset);
            }
            
            return true;
        }

        /// <summary>
        /// Tests if the character would collide with anything, if it was moved through the Scene.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// Unlike previous version this correctly return (if desired) valid hits for blocking overlaps along with MTD to resolve penetration.
        /// </summary>

        private bool SweepTestEx(Vector3 sweepOrigin, float sweepRadius, Vector3 sweepDirection, float sweepDistance, int sweepLayerMask,
            out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, bool ignoreBlockingOverlaps = false)
        {
            // Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
            // when moving almost parallel to an obstacle for small distances).

            hitResult = default;

            bool innerCapsuleHit =
                CapsuleCastEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance + sweepRadius, sweepLayerMask,
                out RaycastHit innerCapsuleHitResult, out startPenetrating, out recoverDirection, out recoverDistance, ignoreBlockingOverlaps) && innerCapsuleHitResult.distance <= sweepDistance;

            if (innerCapsuleHit && startPenetrating)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - SmallContactOffset);

                return true;
            }

            float outerCapsuleRadius = sweepRadius + ContactOffset;

            bool outerCapsuleHit =
                CapsuleCast(sweepOrigin, outerCapsuleRadius, sweepDirection, sweepDistance + outerCapsuleRadius, sweepLayerMask,
                out RaycastHit outerCapsuleHitResult, out _) && outerCapsuleHitResult.distance <= sweepDistance;

            bool foundBlockingHit = innerCapsuleHit || outerCapsuleHit;
            if (!foundBlockingHit)
                return false;

            if (!outerCapsuleHit)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - ContactOffset);
            }
            else if (innerCapsuleHit && innerCapsuleHitResult.distance < outerCapsuleHitResult.distance)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - ContactOffset);
            }
            else
            {
                hitResult = outerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - SmallContactOffset);
            }

            return true;
        }

        private bool ResolvePenetration(Vector3 displacement, Vector3 proposedAdjustment)
        {
            Vector3 adjustment = ConstrainVectorToPlane(proposedAdjustment);
            if (adjustment.IsZero())
                return false;

            // We really want to make sure that precision differences or differences between the overlap test and sweep tests don't put us into another overlap,
            // so make the overlap test a bit more restrictive.

            const float kOverlapInflation = 0.001f;

            if (!(OverlapTest(updatedPosition + adjustment, updatedRotation, radius + kOverlapInflation, height, collisionLayers, overlaps, TriggerInteraction) > 0))
            {
                // Safe to move without sweeping

                updatedPosition += adjustment;

                return true;
            }
            else
            {
                Vector3 lastPosition = updatedPosition;

                // Try sweeping as far as possible, ignoring non-blocking overlaps, otherwise we wouldn't be able to sweep out of the object to fix the penetration.

                bool hit = CapsuleCastEx(updatedPosition, radius, adjustment.normalized, adjustment.magnitude, collisionLayers,
                    out RaycastHit sweepHitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, true);

                if (!hit)
                    updatedPosition += adjustment;
                else
                    updatedPosition += adjustment.normalized * Mathf.Max(sweepHitResult.distance - SmallContactOffset, 0.0f);

                // Still stuck?

                bool moved = updatedPosition != lastPosition;
                if (!moved && startPenetrating)
                {
                    // Combine two MTD results to get a new direction that gets out of multiple surfaces.

                    Vector3 secondMTD = recoverDirection * (recoverDistance + ContactOffset + PenetrationOffset);
                    Vector3 combinedMTD = adjustment + secondMTD;
                    
                    if (secondMTD != adjustment && !combinedMTD.IsZero())
                    {
                        lastPosition = updatedPosition;
                        
                        hit = CapsuleCastEx(updatedPosition, radius, combinedMTD.normalized, combinedMTD.magnitude, 
                            collisionLayers, out sweepHitResult, out _, out _, out _, true);

                        if (!hit)
                            updatedPosition += combinedMTD;
                        else
                            updatedPosition += combinedMTD.normalized * Mathf.Max(sweepHitResult.distance - SmallContactOffset, 0.0f);

                        moved = updatedPosition != lastPosition;
                    }
                }

                // Still stuck?

                if (!moved)
                {
                    // Try moving the proposed adjustment plus the attempted move direction.
                    // This can sometimes get out of penetrations with multiple objects.

                    Vector3 moveDelta = ConstrainVectorToPlane(displacement);
                    if (!moveDelta.IsZero())
                    {
                        lastPosition = updatedPosition;

                        Vector3 newAdjustment = adjustment + moveDelta;
                        hit = CapsuleCastEx(updatedPosition, radius, newAdjustment.normalized, newAdjustment.magnitude, 
                            collisionLayers, out sweepHitResult, out _, out _, out _, true);

                        if (!hit)
                            updatedPosition += newAdjustment;
                        else
                            updatedPosition += newAdjustment.normalized * Mathf.Max(sweepHitResult.distance - SmallContactOffset, 0.0f);

                        moved = updatedPosition != lastPosition;

                        // Finally, try the original move without MTD adjustments, but allowing depenetration along the MTD normal.
                        // This was blocked because ignoreBlockingOverlaps was false for the original move to try a better depenetration normal, but we might be running in to other geometry in the attempt.
                        // This won't necessarily get us all the way out of penetration, but can in some cases and does make progress in exiting the penetration.

                        if (!moved && Vector3.Dot(moveDelta, adjustment) > 0.0f)
                        {
                            lastPosition = updatedPosition;

                            hit = CapsuleCastEx(updatedPosition, radius, moveDelta.normalized, moveDelta.magnitude,
                                collisionLayers, out sweepHitResult, out _, out _, out _, true);

                            if (!hit)
                                updatedPosition += moveDelta;
                            else
                                updatedPosition += moveDelta.normalized * Mathf.Max(sweepHitResult.distance - SmallContactOffset, 0.0f);

                            moved = updatedPosition != lastPosition;
                        }
                    }
                }

                return moved;
            }
        }
        
        /// <summary>
        /// Sweeps the character's volume along its displacement vector, stopping at near hit point if collision is detected or applies full displacement if not.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        private bool MovementSweepTest(Vector3 characterPosition, Vector3 inVelocity, Vector3 displacement,
            out CollisionResult collisionResult)
        {
            collisionResult = default;

            Vector3 sweepOrigin = characterPosition;
            Vector3 sweepDirection = displacement.normalized;

            float sweepRadius = radius;
            float sweepDistance = displacement.magnitude;

            int sweepLayerMask = collisionLayers;
            
            bool hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask, 
                out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance);

            if (startPenetrating)
            {
                // Handle initial penetrations

                Vector3 requestedAdjustment = recoverDirection * (recoverDistance + ContactOffset + PenetrationOffset);

                if (ResolvePenetration(displacement, requestedAdjustment))
                {
                    // Retry original movement

                    sweepOrigin = updatedPosition;
                    hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                        out hitResult, out startPenetrating, out _, out _);
                }
            }

            if (!hit)
                return false;

            HitLocation hitLocation = ComputeHitLocation(hitResult.normal);

            Vector3 displacementToHit = sweepDirection * hitResult.distance;
            Vector3 remainingDisplacement = displacement - displacementToHit;

            Vector3 hitPosition = sweepOrigin + displacementToHit;

            Vector3 surfaceNormal = hitResult.normal;

            bool isWalkable = false;
            bool hitGround = hitLocation == HitLocation.Below;
            
            if (hitGround)
            {
                surfaceNormal = FindGeomOpposingNormal(displacement, ref hitResult);

                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
            }

            collisionResult = new CollisionResult
            {
                startPenetrating = startPenetrating,

                hitLocation = hitLocation,
                isWalkable = isWalkable,

                position = hitPosition,

                velocity = inVelocity,
                otherVelocity = GetRigidbodyVelocity(hitResult.rigidbody, hitResult.point),

                point = hitResult.point,
                normal = hitResult.normal,

                surfaceNormal = surfaceNormal,

                displacementToHit = displacementToHit,
                remainingDisplacement = remainingDisplacement,

                collider = hitResult.collider,

                hitResult = hitResult
            };

            return true;
        }

        /// <summary>
        /// Sweeps the character's volume along its displacement vector, stopping at near hit point if collision is detected.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        public bool MovementSweepTest(Vector3 characterPosition, Vector3 sweepDirection, float sweepDistance,
            out CollisionResult collisionResult)
        {
            return MovementSweepTest(characterPosition, Velocity, sweepDirection * sweepDistance, out collisionResult);
        }

        /// <summary>
        /// Limit the slide vector when falling if the resulting slide might boost the character faster upwards.
        /// </summary>

        private Vector3 HandleSlopeBoosting(Vector3 slideResult, Vector3 displacement, Vector3 inNormal)
        {
            Vector3 result = slideResult;

            float yResult = Vector3.Dot(result, characterUp);
            if (yResult > 0.0f)
            {
                // Don't move any higher than we originally intended.

                float yLimit = Vector3.Dot(displacement, characterUp);
                if (yResult - yLimit > SmallNumber)
                {
                    if (yLimit > 0.0f)
                    {
                        // Rescale the entire vector (not just the Z component) otherwise we change the direction and likely head right back into the impact.

                        float upPercent = yLimit / yResult;
                        result *= upPercent;
                    }
                    else
                    {
                        // We were heading down but were going to deflect upwards. Just make the deflection horizontal.

                        result = Vector3.zero;
                    }

                    // Make remaining portion of original result horizontal and parallel to impact normal.

                    Vector3 lateralRemainder = (slideResult - result).ProjectOnPlane(characterUp);
                    Vector3 lateralNormal = inNormal.ProjectOnPlane(characterUp).normalized;
                    Vector3 adjust = lateralRemainder.ProjectOnPlane(lateralNormal);

                    result += adjust;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate slide vector along a surface.
        /// </summary>

        private Vector3 ComputeSlideVector(Vector3 displacement, Vector3 inNormal, bool isWalkable)
        {
            if (IsGrounded)
            {
                if (isWalkable)
                    displacement = displacement.TangentTo(inNormal, characterUp);
                else
                {
                    Vector3 right = inNormal.PerpendicularTo(GroundNormal);
                    Vector3 up = right.PerpendicularTo(inNormal);

                    displacement = displacement.ProjectOnPlane(inNormal);
                    displacement = displacement.TangentTo(up, characterUp);
                }
            }
            else
            {
                if (isWalkable)
                {
                    if (constrainedToGround)
                        displacement = displacement.ProjectOnPlane(characterUp);
                    
                    displacement = displacement.ProjectOnPlane(inNormal);
                }
                else
                {
                    Vector3 slideResult = displacement.ProjectOnPlane(inNormal);

                    if (constrainedToGround)
                        slideResult = HandleSlopeBoosting(slideResult, displacement, inNormal);
                    
                    displacement = slideResult;
                }
            }

            return ConstrainVectorToPlane(displacement);
        }

        /// <summary>
        /// Resolve collisions of Character's bounding volume during a Move call.
        /// </summary>

        private int SlideAlongSurface(int iteration, Vector3 inputDisplacement, ref Vector3 inVelocity,
            ref Vector3 displacement, ref CollisionResult inHit, ref Vector3 prevNormal)
        {
            if (UseFlatTop && inHit.hitLocation == HitLocation.Above)
            {
                Vector3 surfaceNormal = FindBoxOpposingNormal(displacement, inHit.normal, inHit.transform);

                if (inHit.normal != surfaceNormal)
                {
                    inHit.normal = surfaceNormal;
                    inHit.surfaceNormal = surfaceNormal;
                }
            }

            inHit.normal = ComputeBlockingNormal(inHit.normal, inHit.isWalkable);

            if (inHit.isWalkable && IsConstrainedToGround)
            {
                inVelocity = ComputeSlideVector(inVelocity, inHit.normal, true);
                displacement = ComputeSlideVector(displacement, inHit.normal, true);
            }
            else
            {
                if (iteration == 0)
                {
                    inVelocity = ComputeSlideVector(inVelocity, inHit.normal, inHit.isWalkable);
                    displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);

                    iteration++;
                }
                else if (iteration == 1)
                {
                    Vector3 crease = prevNormal.PerpendicularTo(inHit.normal);

                    Vector3 oVel = inputDisplacement.ProjectOnPlane(crease);

                    Vector3 nVel = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                            nVel = nVel.ProjectOnPlane(crease);

                    if (oVel.Dot(nVel) <= 0.0f || prevNormal.Dot(inHit.normal) < 0.0f)
                    {
                        inVelocity = ConstrainVectorToPlane(inVelocity.ProjectOnPlane(crease));
                        displacement = ConstrainVectorToPlane(displacement.ProjectOnPlane(crease));

                        ++iteration;
                    }
                    else
                    {
                        inVelocity = ComputeSlideVector(inVelocity, inHit.normal, inHit.isWalkable);
                        displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    }
                }
                else
                {
                    inVelocity = Vector3.zero;
                    displacement = Vector3.zero;
                }

                prevNormal = inHit.normal;
            }

            return iteration;
        }

        /// <summary>
        /// Performs collision constrained movement.
        /// This refers to the process of smoothly sliding a moving entity along any obstacles encountered.
        /// Updates _probingPosition.
        /// </summary>
        private void PerformMovement(float deltaTime)
        {
            // Resolve initial overlaps

            DepenetrationBehaviour depenetrationFlags = !enablePhysicsInteraction
                ? DepenetrationBehaviour.IgnoreDynamic
                : DepenetrationBehaviour.IgnoreNone;

            ResolveOverlaps(depenetrationFlags);

            //
            // If grounded, discard velocity vertical component

            if (IsGrounded)
                velocity = Vector3.ProjectOnPlane(velocity, characterUp);

            // Compute displacement

            Vector3 displacement = velocity * deltaTime;

            //
            // If grounded, reorient DISPLACEMENT along current ground normal

            if (IsGrounded)
            {
                displacement = displacement.TangentTo(GroundNormal, characterUp);
                displacement = ConstrainVectorToPlane(displacement);
            }

            //
            // Cache pre movement displacement

            Vector3 inputDisplacement = displacement;

            //
            // Prevent moving into current BLOCKING overlaps, treat those as collisions and slide along 

            int iteration = 0;
            Vector3 prevNormal = default;

            for (int i = 0; i < currentCollisionCount; i++)
            {
                ref CollisionResult collisionResult = ref collisionResults[i];

                bool opposesMovement = Vector3.Dot(displacement, collisionResult.normal) < 0.0f;
                if (!opposesMovement)
                    continue;
                
                // If falling, check if hit is a valid landing spot

                if (IsConstrainedToGround && !isOnWalkableGround)
                {
                    if (IsValidLandingSpot(updatedPosition, ref collisionResult))
                    {
                        isLanded = true;
                        LandedVelocity = collisionResult.velocity;
                    }
                    else
                    {
                        // See if we can convert a normally invalid landing spot (based on the hit result) to a usable one.

                        if (collisionResult.hitLocation == HitLocation.Below)
                        {
                            FindGround(updatedPosition, out FindGroundResult groundResult);

                            collisionResult.isWalkable = groundResult.isWalkableGround;
                            if (collisionResult.isWalkable)
                            {
                                foundGround = groundResult;

                                isLanded = true;
                                LandedVelocity = collisionResult.velocity;
                            }
                        }
                    }

                    // If failed to find a valid landing spot but hit ground, update _foundGround with sweep hit result

                    if (!isLanded && collisionResult.hitLocation == HitLocation.Below)
                    {
                        foundGround.SetFromSweepResult(true, false, updatedPosition, collisionResult.point,
                            collisionResult.normal, collisionResult.surfaceNormal, collisionResult.collider,
                            collisionResult.hitResult.distance);
                    }
                }

                //
                // Slide along blocking overlap

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref velocity, ref displacement,
                    ref collisionResult, ref prevNormal);
            }

            //
            // Perform collision constrained movement (aka: collide and slide)
            
            int maxSlideCount = maxSweepIterations;
            while (DetectCollisions && maxSlideCount-- > 0 && displacement.sqrMagnitude > minMoveDistance.Square())
            {
                bool collided = MovementSweepTest(updatedPosition, velocity, displacement,
                    out CollisionResult collisionResult);

                if (!collided)
                    break;

                // Apply displacement up to hit (near position) and update displacement with remaining displacement

                updatedPosition += collisionResult.displacementToHit;

                displacement = collisionResult.remainingDisplacement;

                // Hit a 'barrier', try to step up

                if (IsGrounded && !collisionResult.isWalkable)
                {
                    if (CanStepUp(collisionResult.collider) &&
                        StepUp(ref collisionResult, out CollisionResult stepResult))
                    {
                        updatedPosition = stepResult.position;

                        displacement = Vector3.zero;
                        break;
                    }
                }

                // If falling, check if hit is a valid landing spot

                if (IsConstrainedToGround && !isOnWalkableGround)
                {
                    if (IsValidLandingSpot(updatedPosition, ref collisionResult))
                    {
                        isLanded = true;
                        LandedVelocity = collisionResult.velocity;
                    }
                    else
                    {
                        // See if we can convert a normally invalid landing spot (based on the hit result) to a usable one.

                        if (ShouldCheckForValidLandingSpot(ref collisionResult))
                        {
                            FindGround(updatedPosition, out FindGroundResult groundResult);

                            collisionResult.isWalkable = groundResult.isWalkableGround;
                            if (collisionResult.isWalkable)
                            {
                                foundGround = groundResult;

                                isLanded = true;
                                LandedVelocity = collisionResult.velocity;
                            }
                        }
                    }

                    // If failed to find a valid landing spot but hit ground, update _foundGround with sweep hit result

                    if (!isLanded && collisionResult.hitLocation == HitLocation.Below)
                    {
                        float sweepDistance = collisionResult.hitResult.distance;
                        Vector3 surfaceNormal = collisionResult.surfaceNormal;

                        foundGround.SetFromSweepResult(true, false, updatedPosition, sweepDistance,
                            ref collisionResult.hitResult, surfaceNormal);
                    }
                }
                
                //
                // Resolve collision (slide along hit surface)

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref velocity, ref displacement,
                    ref collisionResult, ref prevNormal);

                //
                // Cache collision result

                AddCollisionResult(ref collisionResult);
            }

            //
            // Apply remaining displacement

            if (displacement.sqrMagnitude > minMoveDistance.Square())
                updatedPosition += displacement;

            //
            // If grounded, discard vertical movement BUT preserve its magnitude

            if (IsGrounded || isLanded)
            {
                velocity = Vector3.ProjectOnPlane(velocity, characterUp).normalized * velocity.magnitude;
                velocity = ConstrainVectorToPlane(velocity);
            }
        }

        /// <summary>
        /// Determines if can perch on other collider depending CollisionBehavior flags (if any).
        /// </summary>

        private bool CanPerchOn(Collider otherCollider)
        {
            // Validate input collider

            if (otherCollider == null)
                return false;

            // If collision behaviour callback is assigned, use it

            if (CheckMobilityCallback != null)
            {
                MobilityFlags collisionBehaviour = CheckMobilityCallback.Invoke(otherCollider);

                if (collisionBehaviour.HasFlag(MobilityFlags.Perchable))
                    return true;

                if (collisionBehaviour.HasFlag(MobilityFlags.NotPerchable))
                    return false;
            }

            // Default case, managed by perchOffset

            return true;
        }

        /// <summary>
        /// Returns The distance from the edge of the capsule within which we don't allow the character to perch on the edge of a surface.
        /// </summary>
        
        private float GetPerchRadiusThreshold()
        {
	        // Don't allow negative values.
	        
            return Mathf.Max(0.0f, radius - PerchOffset);
        }

        /// <summary>
        /// Returns the radius within which we can stand on the edge of a surface without falling (if this is a walkable surface).
        /// </summary>

        private float GetValidPerchRadius(Collider otherCollider)
        {
            if (!CanPerchOn(otherCollider))
                return 0.0011f;
            
            return Mathf.Clamp(perchOffset, 0.0011f, radius);
        }

        /// <summary>
        /// Check if the result of a sweep test (passed in InHit) might be a valid location to perch, in which case we should use ComputePerchResult to validate the location.
        /// </summary>

        private bool ShouldComputePerchResult(Vector3 characterPosition, ref RaycastHit inHit)
        {
            // Don't try to perch if the edge radius is very small.
	        
            if (GetPerchRadiusThreshold() <= SweepEdgeRejectDistance)
	        {
		        return false;
	        }

            float distFromCenterSq = Vector3.ProjectOnPlane((inHit.point - characterPosition), characterUp).sqrMagnitude;
            float standOnEdgeRadius = GetValidPerchRadius(inHit.collider);

            if (distFromCenterSq <= standOnEdgeRadius.Square())
            {
                // Already within perch radius.

                return false;
            }

            return true;
        }

        /// <summary>
        /// Casts a capsule against specified colliders (by layerMask) in the Scene and returns detailed information on what was hit.
        /// </summary>

        private bool CapsuleCast(Vector3 point1, Vector3 point2, float castRadius, Vector3 castDirection,
            float castDistance, int castLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = false;

            int rawHitCount = UnityEngine.Physics.CapsuleCastNonAlloc(point1, point2, castRadius, castDirection, rayHits,
                castDistance, castLayerMask, TriggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = rayHits[hitIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts a box along a ray and returns detailed information on what was hit.
        /// </summary>

        private bool BoxCast(Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 castDirection,
            float castDistance, int castLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = default;

            int rawHitCount = UnityEngine.Physics.BoxCastNonAlloc(center, halfExtents, castDirection, rayHits, orientation,
                castDistance, castLayerMask, TriggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref rayHits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = rayHits[hitIndex];
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Downwards (along character's up axis) sweep against the world and return the first blocking hit.
        /// </summary>
        
        private bool GroundSweepTest(Vector3 characterPosition, float capsuleRadius, float capsuleHalfHeight,
            float sweepDistance, out RaycastHit hitResult, out bool startPenetrating)
        {
            bool foundBlockingHit;

            if (!UseFlatBaseForGroundChecks)
            {
                Vector3 characterCenter = characterPosition + transformedCapsuleCenter;

                Vector3 point1 = characterCenter - characterUp * (capsuleHalfHeight - capsuleRadius);
                Vector3 point2 = characterCenter + characterUp * (capsuleHalfHeight - capsuleRadius);

                Vector3 sweepDirection = -1.0f * characterUp;

                foundBlockingHit = CapsuleCast(point1, point2, capsuleRadius, sweepDirection, sweepDistance,
                    collisionLayers, out hitResult, out startPenetrating);
            }
            else
            {
                // First test with the box rotated so the corners are along the major axes (ie rotated 45 degrees).

                Vector3 center = characterPosition + transformedCapsuleCenter;
                Vector3 halfExtents = new Vector3(capsuleRadius * 0.707f, capsuleHalfHeight, capsuleRadius * 0.707f);
                
                Quaternion sweepOrientation = Rotation * Quaternion.Euler(0f, -Rotation.eulerAngles.y, 0f);
                Vector3 sweepDirection = -1.0f * characterUp;

                LayerMask sweepLayerMask = collisionLayers;

                foundBlockingHit = BoxCast(center, halfExtents, sweepOrientation * Quaternion.Euler(0.0f, 45.0f, 0.0f),
                    sweepDirection, sweepDistance, sweepLayerMask, out hitResult, out startPenetrating);

                if (!foundBlockingHit && !startPenetrating)
                {
                    // Test again with the same box, not rotated.

                    foundBlockingHit = BoxCast(center, halfExtents, sweepOrientation, sweepDirection, sweepDistance,
                        sweepLayerMask, out hitResult, out startPenetrating);
                }
            }
            
            return foundBlockingHit;
        }

        /// <summary>
        /// Compute distance to the ground from bottom sphere of capsule and store the result in collisionResult.
        /// This distance is the swept distance of the capsule to the first point impacted by the lower hemisphere,
        /// or distance from the bottom of the capsule in the case of a raycast.
        /// </summary>
        
        public void ComputeGroundDistance(Vector3 characterPosition, float sweepRadius, float sweepDistance,
            float castDistance, out FindGroundResult outGroundResult)
        {
            outGroundResult = default;

            // We require the sweep distance to be >= the raycast distance,
            // otherwise the HitResult can't be interpreted as the sweep result.

            if (sweepDistance < castDistance)
                return;

            float characterRadius = radius;
            float characterHeight = height;
            float characterHalfHeight = characterHeight * 0.5f;

            bool foundGround = default;
            bool startPenetrating = default;

            // Sweep test

            if (sweepDistance > 0.0f && sweepRadius > 0.0f)
            {
                // Use a shorter height to avoid sweeps giving weird results if we start on a surface.
                // This also allows us to adjust out of penetrations.

                const float kShrinkScale = 0.9f;
                float shrinkHeight = (characterHalfHeight - characterRadius) * (1.0f - kShrinkScale);

                float capsuleRadius = sweepRadius;
                float capsuleHalfHeight = characterHalfHeight - shrinkHeight;

                float actualSweepDistance = sweepDistance + shrinkHeight;

                foundGround = GroundSweepTest(characterPosition, capsuleRadius, capsuleHalfHeight, actualSweepDistance,
                    out RaycastHit hitResult, out startPenetrating);

                if (foundGround || startPenetrating)
                {
                    // Reject hits adjacent to us, we only care about hits on the bottom portion of our capsule.
                    // Check 2D distance to impact point, reject if within a tolerance from radius.

                    if (startPenetrating || !IsWithinEdgeTolerance(characterPosition, hitResult.point, capsuleRadius))
                    {
                        // Use a capsule with a slightly smaller radius and shorter height to avoid the adjacent object.
                        // Capsule must not be nearly zero or the trace will fall back to a line trace from the start point and have the wrong length.

                        const float kShrinkScaleOverlap = 0.1f;
                        shrinkHeight = (characterHalfHeight - characterRadius) * (1.0f - kShrinkScaleOverlap);

                        capsuleRadius = Mathf.Max(0.0011f, capsuleRadius - SweepEdgeRejectDistance - SmallNumber);
                        capsuleHalfHeight = Mathf.Max(capsuleRadius, characterHalfHeight - shrinkHeight);

                        actualSweepDistance = sweepDistance + shrinkHeight;

                        foundGround = GroundSweepTest(characterPosition, capsuleRadius, capsuleHalfHeight,
                            actualSweepDistance, out hitResult, out startPenetrating);
                    }

                    if (foundGround && !startPenetrating)
                    {
                        // Reduce hit distance by shrinkHeight because we shrank the capsule for the trace.
                        // We allow negative distances here, because this allows us to pull out of penetrations.

                        float maxPenetrationAdjust = Mathf.Max(MaxDistanceToGround, characterRadius);
                        float sweepResult = Mathf.Max(-maxPenetrationAdjust, hitResult.distance - shrinkHeight);

                        Vector3 sweepDirection = -1.0f * characterUp;
                        Vector3 hitPosition = characterPosition + sweepDirection * sweepResult;

                        Vector3 surfaceNormal = hitResult.normal;

                        bool isWalkable = false;
                        bool hitGround = sweepResult <= sweepDistance &&
                                         ComputeHitLocation(hitResult.normal) == HitLocation.Below;
                        
                        if (hitGround)
                        {
                            if (UseFlatBaseForGroundChecks)
                                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
                            else
                            {
                                surfaceNormal = FindGeomOpposingNormal(sweepDirection * sweepDistance, ref hitResult);

                                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
                            }
                        }

                        outGroundResult.SetFromSweepResult(hitGround, isWalkable, hitPosition, sweepResult,
                            ref hitResult, surfaceNormal);

                        if (outGroundResult.isWalkableGround)
                            return;
                    }
                }
            }

            // Since we require a longer sweep than raycast, we don't want to run the raycast if the sweep missed everything.
            // We do however want to try a raycast if the sweep was stuck in penetration.

            if (!foundGround && !startPenetrating)
                return;

            // Ray cast

            if (castDistance > 0.0f)
            {
                Vector3 rayOrigin = characterPosition + transformedCapsuleCenter;
                Vector3 rayDirection = -1.0f * characterUp;

                float shrinkHeight = characterHalfHeight;
                float rayLength = castDistance + shrinkHeight;

                foundGround = Raycast(rayOrigin, rayDirection, rayLength, collisionLayers, out RaycastHit hitResult);

                if (foundGround && hitResult.distance > 0.0f)
                {
                    // Reduce hit distance by shrinkHeight because we started the ray higher than the base.
                    // We allow negative distances here, because this allows us to pull out of penetrations.

                    float MaxPenetrationAdjust = Mathf.Max(MaxDistanceToGround, characterRadius);
                    float castResult = Mathf.Max(-MaxPenetrationAdjust, hitResult.distance - shrinkHeight);
                    
                    if (castResult <= castDistance && IsWalkable(hitResult.collider, hitResult.normal))
                    {
                        outGroundResult.SetFromRaycastResult(true, true, outGroundResult.position,
                            outGroundResult.groundDistance, castResult, ref hitResult);

                        return;
                    }
                }
            }

            // No hits were acceptable.

            outGroundResult.isWalkable = false;
        }

        /// <summary>
        /// Compute the sweep result of the smaller capsule with radius specified by GetValidPerchRadius(),
        /// and return true if the sweep contacts a valid walkable normal within inMaxGroundDistance of impact point.
        /// This may be used to determine if the capsule can or cannot stay at the current location if perched on the edge of a small ledge or unwalkable surface. 
        /// </summary>

        private bool ComputePerchResult(Vector3 characterPosition, float testRadius, float inMaxGroundDistance,
            ref RaycastHit inHit, out FindGroundResult perchGroundResult)
        {
            perchGroundResult = default;

            if (inMaxGroundDistance <= 0.0f)
                return false;

            // Sweep further than actual requested distance, because a reduced capsule radius means we could miss some hits that the normal radius would contact.

            float inHitAboveBase = Mathf.Max(0.0f, Vector3.Dot(inHit.point - characterPosition, characterUp));
            float perchCastDist = Mathf.Max(0.0f, inMaxGroundDistance - inHitAboveBase);
            float perchSweepDist = Mathf.Max(0.0f, inMaxGroundDistance);

            float actualSweepDist = perchSweepDist + radius;
            ComputeGroundDistance(characterPosition, testRadius, actualSweepDist, perchCastDist, out perchGroundResult);

            if (!perchGroundResult.isWalkable)
                return false;
            else if (inHitAboveBase + perchGroundResult.groundDistance > inMaxGroundDistance)
            {
                // Hit something past max distance

                perchGroundResult.isWalkable = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sweeps a vertical cast to find the ground for the capsule at the given location.
        /// Will attempt to perch if ShouldComputePerchResult() returns true for the downward sweep result.
        /// No ground will be found if collision is disabled (eg: detectCollisions == false).
        /// </summary>

        public void FindGround(Vector3 characterPosition, out FindGroundResult outGroundResult)
        {
            // No collision, no ground...
            if (!DetectCollisions)
            {
                outGroundResult = default;
                return;
            }

            // Increase height check slightly if walking,
            // to prevent ground height adjustment from later invalidating the ground result.

            float heightCheckAdjust = IsGrounded ? MaxDistanceToGround + SmallNumber : -MaxDistanceToGround;
            float sweepDistance = Mathf.Max(MaxDistanceToGround, StepOffset + heightCheckAdjust);

            // Sweep ground

            ComputeGroundDistance(characterPosition, radius, sweepDistance, sweepDistance, out outGroundResult);

            // outGroundResult.hitResult is now the result of the vertical ground check.
            // See if we should try to "perch" at this location.

            if (outGroundResult.hitGround && !outGroundResult.isRaycastResult)
            {
                Vector3 positionOnGround = outGroundResult.position;

                if (ShouldComputePerchResult(positionOnGround, ref outGroundResult.hitResult))
                {
                    float maxPerchGroundDistance = sweepDistance;
                    if (IsGrounded)
                        maxPerchGroundDistance += PerchAdditionalHeight;

                    float validPerchRadius = GetValidPerchRadius(outGroundResult.collider);

                    if (ComputePerchResult(positionOnGround, validPerchRadius, maxPerchGroundDistance,
                        ref outGroundResult.hitResult, out FindGroundResult perchGroundResult))
                    {
                        // Don't allow the ground distance adjustment to push us up too high,
                        // or we will move beyond the perch distance and fall next time.

                        float moveUpDist = AvgDistanceToGround - outGroundResult.groundDistance;
                        if (moveUpDist + perchGroundResult.groundDistance >= maxPerchGroundDistance)
                        {
                            outGroundResult.groundDistance = AvgDistanceToGround;
                        }

                        // If the regular capsule is on an unwalkable surface but the perched one would allow us to stand,
                        // override the normal to be one that is walkable.

                        if (!outGroundResult.isWalkableGround)
                        {
                            // Ground distances are used as the distance of the regular capsule to the point of collision,
                            // to make sure AdjustGroundHeight() behaves correctly.

                            float groundDistance = outGroundResult.groundDistance;
                            float raycastDistance = Mathf.Max(MinDistanceToGround, groundDistance);

                            outGroundResult.SetFromRaycastResult(true, true, outGroundResult.position, groundDistance,
                                raycastDistance, ref perchGroundResult.hitResult);
                        }
                    }
                    else
                    {
                        // We had no ground (or an invalid one because it was unwalkable), and couldn't perch here,
                        // so invalidate ground (which will cause us to start falling).

                        outGroundResult.isWalkable = false;
                    }
                }
            }
        }

        /// <summary>
        /// Adjust distance from ground, trying to maintain a slight offset from the ground when walking (based on current GroundResult).
        /// Only if character isConstrainedToGround == true.
        /// </summary>

        private void AdjustGroundHeight()
        {
            // If we have a ground check that hasn't hit anything, don't adjust height.

            if (!currentGround.isWalkableGround || !IsConstrainedToGround)
                return;

            float lastGroundDistance = currentGround.groundDistance;
            
            if (currentGround.isRaycastResult)
            {
                if (lastGroundDistance < MinDistanceToGround && currentGround.raycastDistance >= MinDistanceToGround)
                {
                    // This would cause us to scale unwalkable walls

                    return;
                }
                else
                {
                    // Falling back to a raycast means the sweep was unwalkable (or in penetration).
                    // Use the ray distance for the vertical adjustment.

                    lastGroundDistance = currentGround.raycastDistance;
                }
            }

            // Move up or down to maintain ground height.

            if (lastGroundDistance < MinDistanceToGround || lastGroundDistance > MaxDistanceToGround)
            {
                float initialY = Vector3.Dot(updatedPosition, characterUp);
                float moveDistance = AvgDistanceToGround - lastGroundDistance;

                Vector3 displacement = characterUp * moveDistance;

                Vector3 sweepOrigin = updatedPosition;
                Vector3 sweepDirection = displacement.normalized;

                float sweepRadius = radius;
                float sweepDistance = displacement.magnitude;

                int sweepLayerMask = collisionLayers;

                bool hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                    out RaycastHit hitResult, out bool startPenetrating, out _, out _, true);

                if (!hit && !startPenetrating)
                {
                    // No collision, apply full displacement

                    updatedPosition += displacement;
                    currentGround.groundDistance += moveDistance;
                }
                else if (moveDistance > 0.0f)
                {
                    // Moving up

                    updatedPosition += sweepDirection * hitResult.distance;

                    float currentY = Vector3.Dot(updatedPosition, characterUp);
                    currentGround.groundDistance += currentY - initialY;
                }
                else
                {
                    // Moving down

                    updatedPosition += sweepDirection * hitResult.distance;

                    float currentY = Vector3.Dot(updatedPosition, characterUp);
                    currentGround.groundDistance = currentY - initialY;
                }
            }

            // Adjust root transform position (accounting offset and skinWidth)
        }
        
        /// <summary>
        /// Determines if the character is able to step up on given collider.
        /// </summary>

        private bool CanStepUp(Collider otherCollider)
        {
            // Validate input collider
            if (otherCollider == null)
                return false;

            // If collision behaviour callback assigned, use it

            if (CheckMobilityCallback != null)
            {
                MobilityFlags collisionBehaviour = CheckMobilityCallback.Invoke(otherCollider);

                if (collisionBehaviour.HasFlag(MobilityFlags.CanStepOn))
                    return true;

                if (collisionBehaviour.HasFlag(MobilityFlags.CanNotStepOn))
                    return false;
            }

            // Default case, managed by stepOffset
            return true;
        }

        /// <summary>
        /// Move up steps or slope.
        /// Does nothing and returns false if CanStepUp(collider) returns false, true if the step up was successful.
        /// </summary>

        private bool StepUp(ref CollisionResult inCollision, out CollisionResult stepResult)
        {
            stepResult = default;

            // Don't bother stepping up if top of capsule is hitting something.

            if (inCollision.hitLocation == HitLocation.Above)
                return false;

            // We need to enforce max step height off the actual point of impact with the ground.
            
            float characterInitialGroundPositionY = Vector3.Dot(inCollision.position, characterUp);
            float groundPointY = characterInitialGroundPositionY;
            
            float actualGroundDistance = Mathf.Max(0.0f, currentGround.GetDistanceToGround());
            characterInitialGroundPositionY -= actualGroundDistance;

            float stepTravelUpHeight = Mathf.Max(0.0f, StepOffset - actualGroundDistance);
            float stepTravelDownHeight = StepOffset + MaxDistanceToGround * 2.0f;

            bool hitVerticalFace =
                !IsWithinEdgeTolerance(inCollision.position, inCollision.point, radius + ContactOffset);

            if (!currentGround.isRaycastResult && !hitVerticalFace)
                groundPointY = Vector3.Dot(GroundPoint, characterUp);
            else
                groundPointY -= currentGround.groundDistance;
            
            // Don't step up if the impact is below us, accounting for distance from ground.

            float initialImpactY = Vector3.Dot(inCollision.point, characterUp);
            if (initialImpactY <= characterInitialGroundPositionY)
                return false;
            
            // Step up, treat as vertical wall

            Vector3 sweepOrigin = inCollision.position;
            Vector3 sweepDirection = characterUp;

            float sweepRadius = radius;
            float sweepDistance = stepTravelUpHeight;

            int sweepLayerMask = collisionLayers;

            bool foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out RaycastHit hitResult, out bool startPenetrating);

            if (startPenetrating)
                return false;

            if (!foundBlockingHit)
                sweepOrigin += sweepDirection * sweepDistance;
            else
                sweepOrigin += sweepDirection * hitResult.distance;

            // Step forward (lateral displacement only)

            Vector3 displacement = inCollision.remainingDisplacement;
            Vector3 displacement2D = ConstrainVectorToPlane(Vector3.ProjectOnPlane(displacement, characterUp));

            sweepDistance = displacement.magnitude;
            sweepDirection = displacement2D.normalized;            

            foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out hitResult, out startPenetrating);

            if (startPenetrating)
                return false;

            if (!foundBlockingHit)
                sweepOrigin += sweepDirection * sweepDistance;
            else
            {
                // Could not hurdle the 'barrier', return

                return false;
            }

            // Step down

            sweepDirection = -characterUp;
            sweepDistance = stepTravelDownHeight;

            foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out hitResult, out startPenetrating);

            if (!foundBlockingHit || startPenetrating)
                return false;

            // See if this step sequence would have allowed us to travel higher than our max step height allows.

            float deltaY = Vector3.Dot(hitResult.point, characterUp) - groundPointY;
            if (deltaY > StepOffset)
                return false;

            // Is position on step clear ?

            Vector3 positionOnStep = sweepOrigin + sweepDirection * hitResult.distance;

            if (OverlapTest(positionOnStep, updatedRotation, radius, height, collisionLayers, overlaps, TriggerInteraction) > 0)
                return false;
            
            // Reject unwalkable surface normals here.

            Vector3 surfaceNormal = FindGeomOpposingNormal(sweepDirection * sweepDistance, ref hitResult);

            bool isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
            if (!isWalkable)
            {
                // Reject if normal opposes movement direction.

                bool normalTowardsMe = Vector3.Dot(displacement, surfaceNormal) < 0.0f;
                if (normalTowardsMe)
                    return false;

                // Also reject if we would end up being higher than our starting location by stepping down.

                if (Vector3.Dot(positionOnStep, characterUp) > Vector3.Dot(inCollision.position, characterUp))
                    return false;
            }

            // Reject moves where the downward sweep hit something very close to the edge of the capsule.
            // This maintains consistency with FindGround as well.

            if (!IsWithinEdgeTolerance(positionOnStep, hitResult.point, radius + ContactOffset))
                return false;
            
            // Don't step up onto invalid surfaces if traveling higher.

            if (deltaY > 0.0f && !CanStepUp(hitResult.collider))
                return false;

            // Output new position on step.
            
            stepResult = new CollisionResult
            {
                position = positionOnStep
            };

            return true;
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Jump, etc.
        /// </summary>

        public void PauseGroundConstraint(float unconstrainedTime = 0.1f)
        {
            unconstrainedTimer = Mathf.Max(0.0f, unconstrainedTime);
        }

        /// <summary>
        /// Updates current ground result.
        /// </summary>

        private void UpdateCurrentGround(ref FindGroundResult inGroundResult)
        {
            WasOnGround = IsOnGround;

            wasOnWalkableGround = isOnWalkableGround;

            wasGrounded = IsGrounded;

            currentGround = inGroundResult;
        }

        /// <summary>
        /// Handle collisions of Character's bounding volume during a Move call.
        /// Unlike previous, this do not modifies / updates character's velocity.
        /// </summary>

        private int SlideAlongSurface(int iteration, Vector3 inputDisplacement, ref Vector3 displacement,
            ref CollisionResult inHit, ref Vector3 prevNormal)
        {
            inHit.normal = ComputeBlockingNormal(inHit.normal, inHit.isWalkable);

            if (inHit.isWalkable && IsConstrainedToGround)
                displacement = ComputeSlideVector(displacement, inHit.normal, true);
            else
            {
                if (iteration == 0)
                {
                    displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    iteration++;
                }
                else if (iteration == 1)
                {
                    Vector3 crease = prevNormal.PerpendicularTo(inHit.normal);//prevNormal.perpendicularTo(inHit.normal);

                    Vector3 oVel = Vector3.ProjectOnPlane(inputDisplacement, crease);

                    Vector3 nVel = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                            nVel = Vector3.ProjectOnPlane(nVel, crease);

                    if (Vector3.Dot(oVel, nVel) <= 0.0f || Vector3.Dot(prevNormal, inHit.normal) < 0.0f)
                    {
                        displacement = ConstrainVectorToPlane(Vector3.Project(displacement, crease));
                        ++iteration;
                    }
                    else
                    {
                        displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    }
                }
                else
                {
                    displacement = Vector3.zero;
                }

                prevNormal = inHit.normal;
            }

            return iteration;
        }

        /// <summary>
        /// Perform collision constrained movement.
        /// This is exclusively used to move the character when standing on a moving platform as this will not update character's state.
        /// </summary>

        private void MoveAndSlide(Vector3 displacement)
        {
            //
            // Perform collision constrained movement (aka: collide and slide)

            Vector3 inputDisplacement = displacement;

            int iteration = default;
            Vector3 prevNormal = default;

            int maxSlideCount = maxSweepIterations;
            while (maxSlideCount-- > 0 && displacement.sqrMagnitude > minMoveDistance.Square())
            {
                bool collided = MovementSweepTest(updatedPosition, default, displacement, out CollisionResult collisionResult);
                if (!collided)
                    break;

                // Apply displacement up to hit (near position) and update displacement with remaining displacement

                updatedPosition += collisionResult.displacementToHit;

                displacement = collisionResult.remainingDisplacement;

                //
                // Resolve collision (slide along hit surface)

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref displacement, ref collisionResult, ref prevNormal);

                //
                // Cache collision result

                AddCollisionResult(ref collisionResult);
            }

            //
            // Apply remaining displacement

            if (displacement.sqrMagnitude > minMoveDistance.Square())
                updatedPosition += displacement;
        }

        /// <summary>
        /// Determines if the character is able to ride on (use it as moving platform) given collider.
        /// </summary>

        private bool CanRideOn(Collider otherCollider)
        {
            // Validate input collider

            if (otherCollider == null || otherCollider.attachedRigidbody == null)
                return false;

            // If collision behaviour callback assigned, use it

            if (CheckMobilityCallback != null)
            {
                MobilityFlags collisionBehaviour = CheckMobilityCallback.Invoke(otherCollider);

                if (collisionBehaviour.HasFlag(MobilityFlags.CanRideOn))
                    return true;

                if (collisionBehaviour.HasFlag(MobilityFlags.CanNotRideOn))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Make collision detection ignore active platform collider(s).
        /// </summary>

        private void IgnoreCurrentPlatform(bool ignore)
        {
            IgnoreCollision(movingPlatform.platform, ignore);
        }

        /// <summary>
        /// Allows you to explicitly attach this to a moving 'platform' so it no depends of ground state.
        /// </summary>

        public void AttachTo(Rigidbody parent)
        {
            parentPlatform = parent;
        }

        /// <summary>
        /// Update current active moving platform (if any).
        /// </summary>

        private void UpdateCurrentPlatform()
        {
            lastVelocityOfPlatform = Vector3.zero;
            
            movingPlatform.lastPlatform = movingPlatform.platform;

            if (parentPlatform)
                movingPlatform.platform = parentPlatform;
            else if (IsGrounded && CanRideOn(GroundCollider))
                movingPlatform.platform = GroundCollider.attachedRigidbody;
            else
                movingPlatform.platform = null;

            if (movingPlatform.platform != null)
            {
                Transform platformTransform = movingPlatform.platform.transform;

                movingPlatform.position = updatedPosition;
                movingPlatform.localPosition = platformTransform.InverseTransformPoint(updatedPosition);

                movingPlatform.rotation = updatedRotation;
                movingPlatform.localRotation = Quaternion.Inverse(platformTransform.rotation) * updatedRotation;
                
                lastVelocityOfPlatform = Velocity;
            }
        }

        /// <summary>
        /// Update moving platform data and move /rotate character with it (if allowed).
        /// </summary>
        private void UpdatePlatformMovement(float deltaTime)
        {
            Vector3 lastPlatformVelocity = movingPlatform.platformVelocity;

            if (!movingPlatform.platform)
                movingPlatform.platformVelocity = Vector3.zero;
            else
            {
                Transform platformTransform = movingPlatform.platform.transform;

                Vector3 newPositionOnPlatform = platformTransform.TransformPoint(movingPlatform.localPosition);
                Vector3 deltaPosition = newPositionOnPlatform - movingPlatform.position;

                movingPlatform.deltaPosition = deltaPosition;
                movingPlatform.platformVelocity = deltaTime > 0.0f ? deltaPosition / deltaTime : Vector3.zero;

                if (impartPlatformRotation)
                {
                    Quaternion newRotationOnPlatform = platformTransform.rotation * movingPlatform.localRotation;
                    Quaternion deltaRotation = newRotationOnPlatform * Quaternion.Inverse(movingPlatform.rotation);

                    movingPlatform.deltaRotation = deltaRotation;

                    Vector3 newForward = Vector3
                        .ProjectOnPlane(deltaRotation * updatedRotation * Vector3.forward, characterUp).normalized;
                    
                    updatedRotation = Quaternion.LookRotation(newForward, characterUp);
                }
            }

            if (impartPlatformMovement && movingPlatform.platformVelocity.sqrMagnitude > 0.0f)
            {
                if (fastPlatformMove)
                    updatedPosition += movingPlatform.platformVelocity * deltaTime;
                else
                {
                    IgnoreCurrentPlatform(true);

                    MoveAndSlide(movingPlatform.platformVelocity * deltaTime);

                    IgnoreCurrentPlatform(false);
                }
            }

            if (impartPlatformVelocity && movingPlatform.lastPlatform && movingPlatform.platform != movingPlatform.lastPlatform)
            {
                velocity -= movingPlatform.platformVelocity;

                velocity += lastPlatformVelocity;
            }
            
            if (impartPlatformVelocity && movingPlatform.lastPlatform == null && movingPlatform.platform)
            {
                velocity = lastVelocityOfPlatform - movingPlatform.platformVelocity;
            }
        }

        /// <summary>
        /// Compute collision response impulses for character vs rigidbody or character vs character.
        /// </summary>

        private void ComputeDynamicCollisionResponse(ref CollisionResult inCollisionResult,
            out Vector3 characterImpulse, out Vector3 otherImpulse)
        {
            characterImpulse = default;
            otherImpulse = default;

            float massRatio = 0.0f;

            Rigidbody otherRigidbody = inCollisionResult.rigidbody;
            if (!otherRigidbody.isKinematic || otherRigidbody.TryGetComponent(out CharacterPhysics _))
            {
                float mass = rigidbody.mass;
                massRatio = mass / (mass + inCollisionResult.rigidbody.mass);
            }

            Vector3 normal = inCollisionResult.normal;

            float velocityDotNormal = Vector3.Dot(inCollisionResult.velocity, normal);
            float otherVelocityDotNormal = Vector3.Dot(inCollisionResult.otherVelocity, normal);

            if (velocityDotNormal < 0.0f)
                characterImpulse += velocityDotNormal * normal;

            if (otherVelocityDotNormal > velocityDotNormal)
            {
                Vector3 relVel = (otherVelocityDotNormal - velocityDotNormal) * normal;

                characterImpulse += relVel * (1.0f - massRatio);
                otherImpulse -= relVel * massRatio;
            }
        }

        /// <summary>
        /// Compute and apply collision response impulses for dynamic collisions (eg: character vs rigidbodies or character vs other character).
        /// </summary>

        private void ResolveDynamicCollisions()
        {
            if (!enablePhysicsInteraction)
                return;

            for (int i = 0; i < currentCollisionCount; i++)
            {
                ref CollisionResult collisionResult = ref collisionResults[i];
                if (collisionResult.isWalkable)
                    continue;

                Rigidbody otherRigidbody = collisionResult.rigidbody;
                if (otherRigidbody == null)
                    continue;

                ComputeDynamicCollisionResponse(ref collisionResult, out Vector3 characterImpulse, out Vector3 otherImpulse);

                if (otherRigidbody.TryGetComponent(out CharacterPhysics otherCharacter))
                {
                    if (AllowPushed)
                    {
                        Velocity += characterImpulse;
                        otherCharacter.Velocity += otherImpulse * ForceScale;
                    }
                }
                else
                {
                    velocity += characterImpulse;

                    if (!otherRigidbody.isKinematic)
                    {
                        otherRigidbody.AddForceAtPosition(otherImpulse * ForceScale, collisionResult.point,
                            ForceMode.VelocityChange);
                    }
                }
            }

            if (IsGrounded)
                velocity = Vector3.ProjectOnPlane(velocity, characterUp).normalized * velocity.magnitude;

            velocity = ConstrainVectorToPlane(velocity);
        }

        /// <summary>
        /// 캐릭터의 Position 설정
        /// updateGround가 true라면 가까운 지면을 찾아 발 위치 맞춤
        /// </summary>

        public void SetPosition(Vector3 newPosition, bool updateGround = false)
        {
            updatedPosition = newPosition;

            if (updateGround)
            {
                FindGround(updatedPosition, out FindGroundResult groundResult);
                {
                    UpdateCurrentGround(ref groundResult);

                    AdjustGroundHeight();

                    UpdateCurrentPlatform();
                }
            }

            transform.position = updatedPosition;
        }

        /// <summary>
        /// 캐릭터의 Rotation 설정
        /// </summary>
        public void SetRotation(Quaternion newRotation)
        {
            updatedRotation = newRotation;

            transform.rotation = updatedRotation;
        }

        /// <summary>
        /// SetPosition + SetRotation
        /// updateGround가 true라면 가까운 지면을 찾아 발 위치 맞춤
        /// </summary>
        public void SetPositionAndRotation(Vector3 newPosition, Quaternion newRotation, bool updateGround = false)
        {
            SetPosition(newPosition, updateGround);
            SetRotation(newRotation);
        }

        /// <summary>
        /// Orient the character's towards the given direction (in world space) using maxDegreesDelta as the rate of rotation change.
        /// </summary>
        /// <param name="worldDirection">The target direction in world space.</param>
        /// <param name="maxDegreesDelta">Change in rotation per second (Deg / s).</param>
        /// <param name="updateYawOnly">If True, the rotation will be performed on the Character's plane (defined by its up-axis).</param>

        public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
        {
            Vector3 characterUp = transform.up;

            if (updateYawOnly)
                worldDirection = Vector3.ProjectOnPlane(worldDirection, characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            Rotation = Quaternion.RotateTowards(Rotation, targetRotation, maxDegreesDelta);
        }

        /// <summary>
        /// 이동 계산에 사용할 값들을 캐싱해두기
        /// </summary>
        private void UpdateCachedFields()
        {
            isLanded = false;
            foundGround = default;

            updatedPosition = transform.position;
            updatedRotation = transform.rotation;

            characterUp = updatedRotation * Vector3.up;

            transformedCapsuleCenter = updatedRotation * capsuleCenter;
            transformedCapsuleTop = updatedRotation * capsuleTop;
            transformedCapsuleBottom = updatedRotation * capsuleBottom;

            collisionFlags = CollisionFlags.None;
        }

        /// <summary>
        /// Clears any accumulated forces, including any pending launch velocity.
        /// </summary>

        public void ClearAccumulatedForces()
        {
            pendingForces = Vector3.zero;
            pendingImpulses = Vector3.zero;
            pendingLaunchVelocity = Vector3.zero;
        }

        /// <summary>
        /// 캐릭터에 Force 값 누적
        /// Move 메서드에서 한 번에 처리
        /// </summary>
        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            switch (forceMode)
            {
                case ForceMode.Force:
                    {
                        pendingForces += force / rigidbody.mass;
                        break;
                    }

                case ForceMode.Acceleration:
                    {
                        pendingForces += force;
                        break;
                    }

                case ForceMode.Impulse:
                    {
                        pendingImpulses += force / rigidbody.mass;
                        break;
                    }

                case ForceMode.VelocityChange:
                    {
                        pendingImpulses += force;
                        break;
                    }
            }
        }

        /// <summary>
        /// 폭발 등의 효과로 발생하는 Force를 캐릭터에 누적 처리
        /// </summary>
        public void AddExplosionForce(float strength, Vector3 origin, float radius, float upwardModifier, ForceMode forceMode = ForceMode.Force)
        {
            Vector3 delta = CharacterCenter - origin;
            float deltaMagnitude = delta.magnitude;
            if (deltaMagnitude > radius)
                return;

            Vector3 explosionDirection = delta.normalized;
            float attenuation = 1.0f - Mathf.Clamp01(deltaMagnitude / radius);
            Vector3 force = explosionDirection * (strength * attenuation);

            if (upwardModifier != 0.0f)
            {
                force += Vector3.up * (upwardModifier * attenuation);
            }

            AddForce(force, forceMode);
        }

        /// <summary>
        /// Set a pending launch velocity on the Character. This velocity will be processed next Move call.
        /// </summary>
        /// <param name="launchVelocity">The desired launch velocity.</param>
        /// <param name="overrideVerticalVelocity">If true replace the vertical component of the Character's velocity instead of adding to it.</param>
        /// <param name="overrideLateralVelocity">If true replace the XY part of the Character's velocity instead of adding to it.</param>

        public void LaunchCharacter(Vector3 launchVelocity, bool overrideVerticalVelocity = false, bool overrideLateralVelocity = false)
        {
            // Compute final velocity

            Vector3 finalVelocity = launchVelocity;

            // If not override, add lateral velocity to given launch velocity

            Vector3 characterUp = transform.up;

            if (!overrideLateralVelocity)
                finalVelocity += Vector3.ProjectOnPlane(velocity, characterUp);

            // If not override, add vertical velocity to given launch velocity

            if (!overrideVerticalVelocity)
                finalVelocity += Vector3.Project(velocity, characterUp);

            pendingLaunchVelocity = finalVelocity;
        }

        /// <summary>
        /// Updates character's velocity, will apply and clear any pending forces and impulses.
        /// </summary>

        private void UpdateVelocity(Vector3 newVelocity, float deltaTime)
        {
            // Assign new velocity

            velocity = newVelocity;
            
            // Add pending accumulated forces

            velocity += pendingForces * deltaTime;
            velocity += pendingImpulses;

            // Apply pending launch velocity

            if (pendingLaunchVelocity.sqrMagnitude > 0.0f)
                velocity = pendingLaunchVelocity;
            
            // Clear accumulated forces

            ClearAccumulatedForces();

            // Apply plane constraint (if any)

            velocity = ConstrainVectorToPlane(velocity);
        }

        /// <summary>
        /// Moves the character along the given velocity vector.
        /// This performs collision constrained movement resolving any collisions / overlaps found during this movement.
        /// </summary>
        /// <param name="newVelocity">The updated velocity for current frame. It is typically a combination of vertical motion due to gravity and lateral motion when your character is moving.</param>
        /// <param name="deltaTime">The simulation deltaTime. If not assigned, it defaults to Time.deltaTime.</param>
        /// <returns>Return CollisionFlags. It indicates the direction of a collision: None, Sides, Above, and Below.</returns>
        
        public CollisionFlags Move(Vector3 newVelocity, float deltaTime)
        {
            UpdateCachedFields();

            ClearCollisionResults();
            
            UpdateVelocity(newVelocity, deltaTime);

            UpdatePlatformMovement(deltaTime);

            PerformMovement(deltaTime);
            
            if (IsGrounded || isLanded)
                FindGround(updatedPosition, out foundGround);
            
            UpdateCurrentGround(ref foundGround);
            {
                if (unconstrainedTimer > 0.0f)
                {
                    unconstrainedTimer -= deltaTime;
                    if (unconstrainedTimer <= 0.0f)
                        unconstrainedTimer = 0.0f;
                }
            }

            AdjustGroundHeight();

            UpdateCurrentPlatform();

            ResolveDynamicCollisions();
            
            SetPositionAndRotation(updatedPosition, updatedRotation);

            OnCollided();

            if (!wasOnWalkableGround && IsOnGround)
                OnFoundGround();

            return collisionFlags;
        }

        /// <summary>
        /// 캐릭터를 현재 Velocity 값 기준으로 이동
        /// 이동 과정에서 충돌 / 겹침, 이동 제약 등의 사항을 한 번에 처리
        /// </summary>
        /// <param name="deltaTime">Delta Time</param>

        public CollisionFlags Move(float deltaTime)
        {
            return Move(velocity, deltaTime);
        }

        /// <summary>
        /// Update the character's velocity using a friction-based physical model and move the character along its updated velocity.
        /// This performs collision constrained movement resolving any collisions / overlaps found during this movement.
        /// </summary>
        /// <param name="desiredVelocity">Target velocity</param>
        /// <param name="maxSpeed">The maximum speed when grounded. Also determines maximum horizontal speed when falling (i.e. not-grounded).</param>
        /// <param name="acceleration">The rate of change of velocity when accelerating (i.e desiredVelocity != Vector3.zero).</param>
        /// <param name="deceleration">The rate at which the character slows down when braking (i.e. not accelerating or if character is exceeding max speed).
        /// This is a constant opposing force that directly lowers velocity by a constant value.</param>
        /// <param name="friction">Setting that affects movement control. Higher values allow faster changes in direction.</param>
        /// <param name="brakingFriction">Friction (drag) coefficient applied when braking (whenever desiredVelocity == Vector3.zero, or if character is exceeding max speed).</param>
        /// <param name="gravity">The current gravity force.</param>
        /// <param name="onlyHorizontal">Determines if the vertical velocity component should be ignored when falling (i.e. not-grounded) preserving gravity effects.</param>
        /// <param name="deltaTime">The simulation deltaTime.</param>
        /// <returns>Return CollisionFlags. It indicates the direction of a collision: None, Sides, Above, and Below.</returns>

        public CollisionFlags SimpleMove(Vector3 desiredVelocity, float maxSpeed, float acceleration,
            float deceleration, float friction, float brakingFriction, Vector3 gravity, bool onlyHorizontal, float deltaTime)
        {
            if (IsGrounded)
            {
                // Calc new velocity

                Velocity = CalcVelocity(Velocity, desiredVelocity, maxSpeed, acceleration, deceleration, friction,
                    brakingFriction, deltaTime);
            }
            else
            {
                // Calc not grounded velocity

                Vector3 worldUp = -1.0f * gravity.normalized;
                Vector3 v = onlyHorizontal ? Vector3.ProjectOnPlane(Velocity, worldUp) : Velocity;

                if (onlyHorizontal)
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, worldUp);

                // On not walkable ground ?

                if (IsOnGround)
                {
                    // If moving into a 'wall', limit contribution.
                    // Allow movement parallel to the wall, but not into it because that may push us up.

                    Vector3 actualGroundNormal = GroundNormal;
                    if (Vector3.Dot(desiredVelocity, actualGroundNormal) < 0.0f)
                    {
                        actualGroundNormal = Vector3.ProjectOnPlane(actualGroundNormal, worldUp).normalized;
                        desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, actualGroundNormal);
                    }
                }

                // Calc new velocity

                v = CalcVelocity(v, desiredVelocity, maxSpeed, acceleration, deceleration, friction, brakingFriction, deltaTime);

                // Update character's velocity

                if (onlyHorizontal)
                    Velocity += Vector3.ProjectOnPlane(v - Velocity, worldUp);
                else
                    Velocity += v - Velocity;

                // Apply gravity acceleration

                Velocity += gravity * deltaTime;
            }

            // Perform the movement

            return Move(deltaTime);
        }

        
        /// <summary>
        /// Restore a previous simulation state ensuring proper simulation continuity.
        /// </summary>        
        public void SetState(Vector3 inPosition, Quaternion inRotation, Vector3 inVelocity,
            bool inConstrainedToGround, float inUnconstrainedTimer, bool inHitGround, bool inIsWalkable)
        {
            velocity = inVelocity;

            constrainedToGround = inConstrainedToGround;
            unconstrainedTimer = Mathf.Max(0.0f, inUnconstrainedTimer);

            currentGround.hitGround = inHitGround;
            currentGround.isWalkable = inIsWalkable;

            SetPositionAndRotation(inPosition, inRotation, IsGrounded);
        }

        public void ForceReset()
        {
            Reset();
        }
        #endregion

        #region MonoBehaviour Methods
        private void Reset()
        {
            SetDimensions(0.5f, 2.0f);
            SetPlaneConstraint(AxisConstraint.None);

            slopeLimit = 45.0f;
            stepOffset = 0.45f;
            perchOffset = 0.5f;
            perchAdditionalHeight = 0.4f;
            
            triggerInteraction = QueryTriggerInteraction.Ignore;
			enablePhysicsInteraction = false;
			allowPushed = false;

            minMoveDistance = 0;
            maxSweepIterations = 5;
            maxDepenetrationIterations = 1;

            impartPlatformMovement = false;
            impartPlatformRotation = false;
            impartPlatformVelocity = false;

            useFastGeomNormalPath = false;

            constrainedToGround = true;

            forceScale = 1.0f;
        }
        private void Awake()
        {
            CacheComponents();

            SetDimensions(radius, height);
            SetPlaneConstraint(planeConstraint);
        }

        private void OnEnable()
        {
            updatedPosition = transform.position;
            updatedRotation = transform.rotation;
            
            UpdateCachedFields();
        }
		#endregion

        #region UnityEditor Only Method
        #if UNITY_EDITOR
        private void OnValidate()
        {
            this.hideFlags |= HideFlags.HideInInspector;

            SetDimensions(radius, height);
            SetPlaneConstraint(planeConstraint);

            SlopeLimit = slopeLimit;
            StepOffset = stepOffset;
            PerchOffset = perchOffset;
            PerchAdditionalHeight = perchAdditionalHeight;

            minMoveDistance = Mathf.Max(minMoveDistance, 0);
            maxSweepIterations = Mathf.Max(maxSweepIterations, 1);
            maxDepenetrationIterations = Mathf.Max(maxDepenetrationIterations, 1);
        }

        #endif
        #endregion
    }
}