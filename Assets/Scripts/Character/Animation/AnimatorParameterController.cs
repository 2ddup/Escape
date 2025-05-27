///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[RequireComponent(typeof(Animator))]
	public class AnimationParameterController : MonoBehaviour
	{
		#region Constants
		int FrontalHash = Animator.StringToHash("Frontal");
		int HorizontalHash = Animator.StringToHash("Horizontal");
		int GroundHash = Animator.StringToHash("OnGround");
		int CrouchHash = Animator.StringToHash("Crouch");
		int JumpHash = Animator.StringToHash("Jump");
		#endregion
		#region Fields
		Animator animator;
		PhysicsBasedController movement;
		#endregion

		void Awake()
		{
			CacheComponents();
		}
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			animator = GetComponent<Animator>();
			movement = GetComponentInParent<PhysicsBasedController>();
		}
        void Update()
        {
			if(movement.IsPaused)
				return;

			float deltaTime = Time.deltaTime;

			Vector3 moveDirection = movement.transform.InverseTransformDirection(movement.GetMovementDirection());
			animator.SetFloat(HorizontalHash, moveDirection.x, 0.1f, deltaTime);
			animator.SetFloat(FrontalHash, moveDirection.z, 0.1f, deltaTime);

			
            animator.SetBool(GroundHash, movement.IsGrounded());
            //animator.SetBool(CrouchHash, movement.IsCrouched());

            //if (movement.IsFalling())
            //    animator.SetFloat(JumpHash, movement.velocity.y, 0.1f, deltaTime);
        }
    }
}