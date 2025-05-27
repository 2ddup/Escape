///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace TempNamespace.Character.Controller
{
	[Serializable]	
	public struct InputCodes
	{
		public enum ValueType
		{
			Button,
			Toggle
		}
		public enum KeyType
		{
			Down,
			Up
		}

		public string valueName;
		public ValueType valueType;
		public KeyType inputType;
		public KeyCode code;
	}
	public class KeyboardMouseInputHandler : CharacterInputHandler
	{
		#region Inspector Fields
		//TODO: 추후 변경
		public string horizontal = "Horizontal";
		public string vertical = "Vertical";
		public string pitch = "Mouse Y";
		public string yaw = "Mouse X";
		//

		[Header("Key Codes")]
		[SerializeField, Tooltip("체크할 입력 값 목록")]
		private InputCodes[] _inputCondes;

		[Header("Mouse")]
		[SerializeField, Tooltip("마우스 회전 감도")]
		private float _mouseSensitivity = 60;
		
		[SerializeField, Tooltip("X축 회전 반전 여부")]
		private bool _isInverted;
		
		/// <summary>
		/// X축 회전 반전 여부
		/// </summary>
		public bool IsInverted
		{
		   get => _isInverted;
		   set => _isInverted = value;
		}
        #endregion

        #region Fields
        #endregion

        #region Properties
		/// <summary>
		/// 체크할 입력 값 목록
		/// </summary>
		public InputCodes[] InputCodes
		{
		   get => _inputCondes;
		   set => _inputCondes = value;
		}
		
		/// <summary>
		/// 마우스 회전 감도
		/// </summary>
		public float MouseSensitivity
		{
		   get => _mouseSensitivity;
		   set => _mouseSensitivity = value;
		}
        #endregion

        #region	Events

        #endregion

        #region Methods
        private void SolveInput(InputCodes inputCode)
        {
			bool invoked = false;
			if(inputCode.inputType == Controller.InputCodes.KeyType.Down && Input.GetKeyDown(inputCode.code))
			{
				invoked = true;
			}
			else if(inputCode.inputType == Controller.InputCodes.KeyType.Up && Input.GetKeyUp(inputCode.code))
			{
				invoked = true;
			}

			if(!invoked)
				return;
		

            if(inputCode.valueType == Controller.InputCodes.ValueType.Toggle)
			{
				KeyValues<bool> value = ToggleValues.Find(x => x.name == inputCode.valueName);
				
				if(value == null)
				{
					return;
				}
				else
				{
					value.value = !value.value;
				}
			}
			else if(inputCode.valueType == Controller.InputCodes.ValueType.Button)
			{
				KeyValues<bool> value = ButtonValues.Find(x => x.name == inputCode.valueName);
				
				if(value == null)
				{
					return;
				}
				else
				{
					value.value = inputCode.inputType == Controller.InputCodes.KeyType.Down;
				}
			}
        }
        #endregion

        #region MonoBehaviour Methods
		
		protected virtual void OnEnable()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		protected virtual void OnDisable()
		{
			Cursor.lockState = CursorLockMode.None;

			Horizontal = 0;
			Vertical = 0;
			
			Pitch = 0;
			Yaw = 0;
			Roll = 0;
		}

        void FixedUpdate()
        {
			//TODO: 추후 변경
			Profiler.BeginSample("Fixed Input Update");
			
			Horizontal = Input.GetAxis(horizontal);
			Vertical = Input.GetAxis(vertical);
			Profiler.EndSample();
			//
        }
        void Update()
        {
			//TODO: 추후 변경
			Profiler.BeginSample("Input Update");

			Pitch = Input.GetAxisRaw(pitch) * MouseSensitivity * (IsInverted ? -1 : 1);
			Yaw = Input.GetAxisRaw(yaw) * MouseSensitivity;
			//

            for(int i = 0; i < InputCodes.Length; ++i)
			{				
				SolveInput(InputCodes[i]);
			}
			Profiler.EndSample();

			//TODO: 추후 변경
			if(Input.GetKeyDown(KeyCode.Q))
			{
			
				Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

			}
        }
        #endregion

        #region UnityEditor Only Methods
#if UNITY_EDITOR

#endif
        #endregion
    }
}