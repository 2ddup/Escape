///스크립트 생성 일자 - 2025 - 03 - 25
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TempNamespace.Character.Controller
{	
	[Serializable]
	public class KeyValues<T>
	{
		public string name;
		public T value;
	}
	public abstract class CharacterInputHandler : MonoBehaviour
	{
		#region Constant
		protected const float kMinValue = 0.001f;
		#endregion

		#region Inspector Fields
		[Header("Values")]
		[SerializeField, Tooltip("버튼 타입 값")]
		protected List<KeyValues<bool>> _buttonValues;
		[SerializeField, Tooltip("토글 타입 값")]
		private List<KeyValues<bool>> _toggleValues;
		#endregion

		#region Fields
		/// Fixed Values
		//Movement
		private float _horizontal;
		private float _vertical;

		//Rotations
		private float _pitch;
		private float _yaw;
		private float _roll;
		///
		#endregion
		
		#region Properties
		
		public float Horizontal
		{
			get => _horizontal;
			set => _horizontal = value;
		}		
		public float Vertical
		{
			get => _vertical;
			set => _vertical = value;
		}
		
		public float Pitch
		{
			get => _pitch;
			set => _pitch = value;
		}
		
		public float Yaw
		{
			get => _yaw;
			set => _yaw = value;
		}
		
		public float Roll
		{
			get => _roll;
			set => _roll = value;
		}
		
		/// <summary>
		/// 버튼 타입 값
		/// </summary>
		public List<KeyValues<bool>> ButtonValues
		{
		   get => _buttonValues;
		   set => _buttonValues = value;
		}
		/// <summary>
		/// 토글 타입 값
		/// </summary>
		public List<KeyValues<bool>> ToggleValues
		{
		   get => _toggleValues;
		   set => _toggleValues = value;
		}
		#endregion

		#region	Events

		#endregion
		
		#region Methods
		#endregion

		#region MonoBehaviour Methods
		#endregion
		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void Reset()
		{
		}
		protected virtual void OnValidate()
		{
		}
		#endif
		#endregion
	}
}