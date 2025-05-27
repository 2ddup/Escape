///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using System;
using CHG.EventDriven;
using DG.Tweening;
using TempNamespace.EventDriven.Arguments;
using TempNamespace.Utilities;
using UnityEngine;


namespace TempNamespace.Character.Controller
{
	/// <summary>
	/// 다양한 시야 각도를 지원하는 Cinemachine 연동 Controller
	/// </summary>
	public class DynamicPerspectiveController : PhysicsBasedController
	{
        #region Member Enums
		public enum ControllerPerspective
		{
			/// <summary>
			/// 1인칭
			/// </summary>
			FirstPerson,
			/// <summary>
			/// 3인칭
			/// </summary>
			ThirdPerson,
			/// <summary>
			/// 탑-다운
			/// </summary>
			TopDown,
			/// <summary>
			/// 아이소메트릭
			/// </summary>
			Isometric,
			/// <summary>
			/// 사이드 스크롤(유사 2D)
			/// </summary>
			SideScroll,
		}

        #endregion

        #region Inspector Fields
		[SerializeField, Tooltip("캐릭터 컨트롤러의 시점")]
		private ControllerPerspective _perspective;
		
        #endregion

        #region Fields

        #endregion

        #region Properties

		/// <summary>
		/// 캐릭터 컨트롤러의 시점
		/// 외부에서 변경하고 싶다면 SetPerspective 사용
		/// </summary>
		public ControllerPerspective Perspective
		{
		   get => _perspective;
		   protected set => _perspective = value;
		}
        #endregion

		#region Events
		private void OnChangePerspective(ChangePerspectiveArgs args)
		{
			ChangePerspective(args.newPerspective);
		}
		#endregion

        #region MonoBehaviour Methodes

        protected override void OnEnable()
        {
            base.OnEnable();
			GlobalEventManager.Instance.Subscribe<ChangePerspectiveArgs>("ChangePerspective", OnChangePerspective);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
			if(GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe<ChangePerspectiveArgs>("ChangePerspective", OnChangePerspective);
			}
        }

		protected virtual void LateUpdate() 
		{
			switch(Perspective)
			{
				case ControllerPerspective.FirstPerson:
					HandleFirstPersonPerspective();
					break;
			}
				
		}
        #endregion

        #region Methods

		#region Change Perspective
		public void ChangePerspective(ControllerPerspective newPerspective)
		{
			if(Perspective == newPerspective)
			{
				return;
			}

			switch(newPerspective)
			{
				case ControllerPerspective.FirstPerson:
					ToFirstPerson();
					break;
				case ControllerPerspective.ThirdPerson:
					ToThirdPerson();
					break;
				case ControllerPerspective.TopDown:
					ToTopDown();
					break;
				case ControllerPerspective.SideScroll:
					ToSideScroll();
					break;
				case ControllerPerspective.Isometric:
					ToIsometric();
					break;
			}
		}

        protected virtual void ToFirstPerson()
		{
			Perspective = ControllerPerspective.FirstPerson;
			//카메라 방향 = 회전 방향
			rotationMode = RotationMode.None;
			CameraHolder.SetParent(transform);

			physicsSystem.SetPlaneConstraint(AxisConstraint.None);
		}
		protected virtual void ToThirdPerson()
		{
			Perspective = ControllerPerspective.ThirdPerson;
			//카메라 방향 ≠ 회전 방향
			rotationMode = RotationMode.OrientRotationToMovement;
			CameraHolder.SetParent(transform.parent);

			physicsSystem.SetPlaneConstraint(AxisConstraint.None);
		}
		protected virtual void ToTopDown()
		{
			Perspective = ControllerPerspective.TopDown;
			//이동 방향에 맞춰 회전
			rotationMode = RotationMode.OrientRotationToMovement;
			CameraHolder.SetParent(transform.parent);

			physicsSystem.SetPlaneConstraint(AxisConstraint.None);
		}
		protected virtual void ToSideScroll()
		{
			Perspective = ControllerPerspective.SideScroll;
			//이동 방향에 맞춰 회전
			rotationMode = RotationMode.OrientRotationToMovement;
			CameraHolder.SetParent(transform.parent);

			physicsSystem.SetPlaneConstraint(AxisConstraint.ConstrainXAxis);
		}        
		protected virtual void ToIsometric()
        {
			Perspective = ControllerPerspective.Isometric;
			//이동 방향에 맞춰 회전
			rotationMode = RotationMode.OrientRotationToMovement;
			CameraHolder.SetParent(transform.parent);

			physicsSystem.SetPlaneConstraint(AxisConstraint.None);
        }

		#endregion
		
		//TODO: 카메라 설정을 외부 파일로 분리하기 전까지 임시 값
		float tempPitchLimit = 80;
		float cameraPitch = 0;
        protected virtual void HandleFirstPersonPerspective()
        {
			//Async Height
			CameraHolder.transform.localPosition = Vector3.Lerp(CameraHolder.transform.localPosition, 
                 new Vector3(CameraHolder.localPosition.x, height, CameraHolder.transform.localPosition.z), Time.deltaTime * 10);

			cameraPitch = Mathf.Clamp(cameraPitch + CheckFloatKey("Pitch"), -tempPitchLimit, tempPitchLimit);	
			//
			CameraHolder.transform.localRotation = Quaternion.Euler(cameraPitch, 0.0f, 0.0f);
			//
			float yaw = FloatValues.Find(x => x.name == "Yaw").value;
			AddYawInput(yaw);
        }

        #endregion
    }
}