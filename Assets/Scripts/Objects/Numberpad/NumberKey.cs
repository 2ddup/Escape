///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace
{
	public class NumberKey : MonoBehaviour
	{
		#region Inspector Fields
		[Header("Key")]
		[SerializeField, Tooltip("이 Pad의 Key 값")]
		private string _key;
		
		[SerializeField, Tooltip("Keypad의 Text 표시")]
		private TextMeshPro _text;

		[Header("Events")]
		[SerializeField, Tooltip("키 입력 이벤트")]
		private UnityEvent<string> _onKeyInput;
		
		/// <summary>
		/// 키 입력 이벤트
		/// </summary>
		public UnityEvent<string> OnKeyInput
		{
		   get => _onKeyInput;
		   set => _onKeyInput = value;
		}
		/// <summary>
		/// 이 버튼의 Key 값
		/// </summary>
		public string Key
		{
		   get => _key;
		   set => _key = value;
		}
		/// <summary>
		/// Keypad의 Text 표시
		/// </summary>
		public TextMeshPro Text
		{
		   get => _text;
		   set => _text = value;
		}
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
        #endregion

        #region	Events
        void OnMouseDown()
        {
			OnKeyInput?.Invoke(Key);
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
		}
		#endregion

		
		#region UnityEditor Only Methods
		#if UNITY_EDITOR
		protected virtual void Reset()
		{
		}
		protected virtual void OnValidate()
		{
			if(_text == null)
			{
				_text = GetComponentInChildren<TextMeshPro>();
			}
			_text.text = Key;
		}
		#endif
		#endregion
	}
}