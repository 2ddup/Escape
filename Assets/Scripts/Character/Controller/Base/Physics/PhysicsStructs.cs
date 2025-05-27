///스크립트 생성 일자 - 2025 - 02 - 26
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1
using System;
using UnityEngine;

namespace TempNamespace.Character.Controller.Physics
{
	/// <summary>
    /// 지면 탐색 결과를 저장하는 구조체
    /// </summary>
    public struct FindGroundResult
    {        
        /// <summary>
        /// 지면과 충돌 여부
        /// </summary>
        public bool hitGround;

        /// <summary>
        /// 걸을 수 있는 지형인지 여부
        /// </summary>
        public bool isWalkable;

        /// <summary>
        /// 걸을 수 있는 지형 위에 서 있는가 여부
        /// </summary>
        public bool isWalkableGround => hitGround && isWalkable;

        /// <summary>
        /// 캐릭터 위치(Raycast일 시 Point와 같음)
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// 충돌 정보 저장
        /// </summary>
        public RaycastHit hitResult;
        
        /// <summary>
        /// 충돌 지점
        /// </summary>
        public Vector3 point => hitResult.point;

        /// <summary>
        /// 충돌 지면의 법선벡터
        /// </summary>

        public Vector3 normal => hitResult.normal;

        /// <summary>
        /// Normal of the hit in world space, for the object that was hit by the sweep, if any.
        /// For example if a capsule hits a flat plane, this is a normalized vector pointing out from the plane.
        /// In the case of impact with a corner or edge of a surface, usually the "most opposing" normal (opposed to the query direction) is chosen.
        /// </summary>

        public Vector3 surfaceNormal;

        /// <summary>
        /// The collider of the hit object.
        /// </summary>

        public Collider collider;

        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to a rigidbody then it is null.
        /// </summary>

        public Rigidbody rigidbody => collider ? collider.attachedRigidbody : null;

        /// <summary>
        /// The Transform of the rigidbody or collider that was hit.
        /// </summary>

        public Transform transform
        {
            get
            {
                if (collider == null)
                    return null;

                return collider.transform;                
            }
        }
        
        /// <summary>
        /// The distance to the ground, computed from the swept capsule.
        /// </summary>

        public float groundDistance;

        /// <summary>
        /// True if the hit found a valid walkable ground using a raycast (rather than a sweep test, which happens when the sweep test fails to yield a walkable surface).
        /// </summary>

        public bool isRaycastResult;

        /// <summary>
        /// The distance to the ground, computed from a raycast. Only valid if isRaycast is true.
        /// </summary>

        public float raycastDistance;
        
        /// <summary>
        /// Hit result of the test that found ground.
        /// </summary>



        /// <summary>
        /// Gets the distance to ground, either raycastDistance or distance.
        /// </summary>

        public float GetDistanceToGround()
        {
            return isRaycastResult ? raycastDistance : groundDistance;
        }

        /// <summary>
        /// Initialize this with a sweep test result.
        /// </summary>

        public void SetFromSweepResult(bool hitGround, bool isWalkable, Vector3 position, float sweepDistance,
            ref RaycastHit inHit, Vector3 surfaceNormal)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;
            
            this.position = position;

            collider = inHit.collider;

            groundDistance = sweepDistance;

            isRaycastResult = false;
            raycastDistance = 0.0f;

            hitResult = inHit;

            this.surfaceNormal = surfaceNormal;
        }

        public void SetFromSweepResult(bool hitGround, bool isWalkable, Vector3 position, Vector3 point, Vector3 normal,
            Vector3 surfaceNormal, Collider collider, float sweepDistance)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;
            
            this.position = position;

            this.collider = collider;

            groundDistance = sweepDistance;

            isRaycastResult = false;
            raycastDistance = 0.0f;

            hitResult = new RaycastHit
            {
                point = point,
                normal = normal,

                distance = sweepDistance
            };

            this.surfaceNormal = surfaceNormal;
        }

        /// <summary>
        /// Initialize this with a raycast result.
        /// </summary>

        public void SetFromRaycastResult(bool hitGround, bool isWalkable, Vector3 position, float sweepDistance,
            float castDistance, ref RaycastHit inHit)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;

            this.position = position;

            collider = inHit.collider;

            groundDistance = sweepDistance;

            isRaycastResult = true;
            raycastDistance = castDistance;

            float oldDistance = hitResult.distance;

            hitResult = inHit;
            hitResult.distance = oldDistance;

            surfaceNormal = hitResult.normal;
        }
    }
    
    /// <summary>
    /// 충돌 체크의 결과를 저장
    /// </summary>

    public struct CollisionResult
    {
        /// <summary>
        /// 캐릭터가 무언가와 겹쳐있는 상황인가?
        /// </summary>
        public bool startPenetrating;

        /// <summary>
        /// 충돌한 방향(캐릭터의 캡슐 컬라이더 기준)
        /// </summary>

        public HitLocation hitLocation;

        /// <summary>
        /// 걸을 수 있는 지점인가?
        /// </summary>
        public bool isWalkable;

        /// <summary>
        /// 충돌 시점에서 캐릭터 위치
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// 충돌 시점에서 캐릭터 속력
        /// </summary>

        public Vector3 velocity;

        /// <summary>
        /// 충돌한 다른 객체의 속력
        /// </summary>
        public Vector3 otherVelocity;

        /// <summary>
        /// 충돌 지점(World 좌표)
        /// </summary>

        public Vector3 point;

        /// <summary>
        /// 충돌 법선 벡터(World 기준)
        /// </summary>

        public Vector3 normal;

        /// <summary>
        /// Normal of the hit in world space, for the object that was hit by the sweep, if any.
        /// For example if a capsule hits a flat plane, this is a normalized vector pointing out from the plane.
        /// In the case of impact with a corner or edge of a surface, usually the "most opposing" normal (opposed to the query direction) is chosen.
        /// </summary>
        public Vector3 surfaceNormal;

        /// <summary>
        /// The character's displacement up to this hit.
        /// </summary>

        public Vector3 displacementToHit;

        /// <summary>
        /// Remaining displacement after hit.
        /// </summary>

        public Vector3 remainingDisplacement;

        /// <summary>
        /// The collider of the hit object.
        /// </summary>

        public Collider collider;
        
        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to a rigidbody then it is null.
        /// </summary>

        public Rigidbody rigidbody => collider ? collider.attachedRigidbody : null;

        /// <summary>
        /// The Transform of the rigidbody or collider that was hit.
        /// </summary>

        public Transform transform
        {
            get
            {
                if (collider == null)
                    return null;

                Rigidbody rb = collider.attachedRigidbody;
                return rb ? rb.transform : collider.transform;
            }
        }

        /// <summary>
        /// Structure containing information about this hit.
        /// </summary>

        public RaycastHit hitResult;
    }


	/// <summary>
	/// Structure containing information about platform.
	/// </summary>
	public struct MovingPlatform
	{
		/// <summary>
		/// The last frame active platform.
		/// </summary>

		public Rigidbody lastPlatform;

		/// <summary>
		/// The current active platform.
		/// </summary>

		public Rigidbody platform;

		/// <summary>
		/// The character's last position on active platform.
		/// </summary>

		public Vector3 position;

		/// <summary>
		/// The character's last position on active platform in platform's local space.
		/// </summary>

		public Vector3 localPosition;
		
		/// <summary>
		/// The character's delta position for the last evaluated frame.
		/// </summary>

		public Vector3 deltaPosition;

		/// <summary>
		/// The character's last rotation on active platform.
		/// </summary>

		public Quaternion rotation;

		/// <summary>
		/// The character's last rotation on active platform in platform's local space.
		/// </summary>

		public Quaternion localRotation;
		
		/// <summary>
		/// The character's delta rotation for the last evaluated frame.
		/// Only valid if impartPlatformRotation is true.
		/// </summary>
		public Quaternion deltaRotation;

		/// <summary>
		/// 플랫폼의 현재 속력
		/// </summary>
		public Vector3 platformVelocity;
	}
}