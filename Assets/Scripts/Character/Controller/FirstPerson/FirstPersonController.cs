///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using TempNamespace.Utilities;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
	public class FirstPersonController : PhysicsBasedController
	{
		// #region Inspecter Fields
		// [Header("First Person")]
        // // [Tooltip("1인칭 카메라 Parent")]
        // // public GameObject cameraHolder;
		// #endregion

		// #region Fields
		// #endregion
		
        // /// <summary>
        // /// Add input (affecting Yaw).
        // /// This is applied to the Character's rotation.
        // /// </summary>

        // public virtual void AddControlYawInput(float value)
        // {
        //     if (value != 0.0f)
        //         AddYawInput(value);
        // }

        // /// <summary>
        // /// Add input (affecting Pitch).
        // /// This is applied to the cameraParent's local rotation.
        // /// </summary>

        // public virtual void AddControlPitchInput(float value, float minPitch = -80.0f, float maxPitch = 80.0f)
        // {
        //     if (value != 0.0f)
        //         cameraPitch = PhysicsUtilities.ClampAngle(cameraPitch + value, minPitch, maxPitch);
        // }
        

        // protected override void Reset()
        // {
        //     base.Reset();
		// 	//1인칭 = 캐릭터의 자체 회전을 금지
		// 	SetRotationMode(RotationMode.None);
        // }

        // protected virtual void UpdateCameraHolderPosition()
        // {
        //     cameraHolder.transform.localPosition = Vector3.Lerp(cameraHolder.transform.localPosition, 
        //         new Vector3(cameraHolder.transform.localPosition.x, height, cameraHolder.transform.localPosition.z), Time.deltaTime * 10);
        // }
        // protected virtual void UpdateCameraHolderRotation()
        // {
        //     cameraHolder.transform.localRotation = Quaternion.Euler(cameraPitch, 0.0f, 0.0f);
        // }
		// protected virtual void LateUpdate()
		// {
        //     UpdateCameraHolderPosition();
		// 	UpdateCameraHolderRotation();
		// }

    }
}