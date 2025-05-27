///스크립트 생성 일자 - 2025 - 04 - 07
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using CHG.Utilities.Attribute;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace
{
	public class Numberpad : MonoBehaviour
	{
		#region Inspector Fields

		[Header("Settings")]
		[SerializeField, Tooltip("입력이 가능한 상태인가?")]
		private bool _available;
		

		[SerializeField, Tooltip("정답 문자열")]
		private string _correction;
		[SerializeField, Tooltip("오답 발생시 일시정지 시간"), Min(0)]
		private float _pauseTime;


		[Header("Panel")]
		[SerializeField, Tooltip("텍스트 패널"), Required(true)]
		private TextMeshPro _panelText;
		

		[SerializeField, Tooltip("숫자 패널 Renderer")]
		private MeshRenderer _numPanel;

		[SerializeField, Tooltip("평상시 Material")]
		private Material _neutralMtrl;
		[SerializeField, Tooltip("정답 Material")]
		private Material _correctMtrl;
		[SerializeField, Tooltip("오답 Material")]
		private Material _wrongMtrl;
		
		[Header("Events")]
		[SerializeField, Tooltip("정답 이벤트")]
		private UnityEvent _onCorrect;
		[SerializeField, Tooltip("오답 이벤트")]
		private UnityEvent _onWrong;
		[SerializeField, Tooltip("Reset 이벤트")]
		private UnityEvent _onReset;

		#endregion

		#region Fields
		Transform _transform;
		private string _currentInput;
		
		bool isPause = false;

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
		
		/// <summary>
		/// 입력이 가능한 상태인가?
		/// </summary>
		public bool Available
		{
		   get => _available;
		   set => _available = value;
		}
		
		/// <summary>
		/// 오답 발생시 일시정지 시간
		/// </summary>
		public float PauseTime
		{
		   get => _pauseTime;
		   set => _pauseTime = value;
		}
		/// <summary>
		/// 패널 텍스트
		/// </summary>
		public TextMeshPro PanelText
		{
		   get => _panelText;
		   set => _panelText = value;
		}
		/// <summary>
		/// 정답 문자열
		/// </summary>
		public string Correction
		{
		   get => _correction;
		   set => _correction = value;
		}
		/// <summary>
		/// 평상시 Material
		/// </summary>
		public Material NeutralMtrl
		{
		   get => _neutralMtrl;
		   set => _neutralMtrl = value;
		}
		
		/// <summary>
		/// 오답 Material
		/// </summary>
		public Material WrongMtrl
		{
		   get => _wrongMtrl;
		   set => _wrongMtrl = value;
		}
		
		/// <summary>
		/// 정답 Material
		/// </summary>
		public Material CorrectMaterial
		{
		   get => _correctMtrl;
		   set => _correctMtrl = value;
		}
		
		/// <summary>
		/// 숫자 패널 Renderer
		/// </summary>
		public MeshRenderer NumPanel
		{
		   get => _numPanel;
		   set => _numPanel = value;
		}
		/// <summary>
		/// 현재 입력된 값
		/// </summary>
		public string CurrentInput
		{
			get => _currentInput;
			set
			{
				if(_currentInput == value)
					return;
				_currentInput = value;
				PanelText.text = _currentInput;

				CheckNumpad();		
			}
		}
		/// <summary>
		/// 오답 이벤트
		/// </summary>
		public UnityEvent OnWrong
		{
		   get => _onWrong;
		   set => _onWrong = value;
		}
		
		/// <summary>
		/// 정답시 이벤트
		/// </summary>
		public UnityEvent OnCorrect
		{
		   get => _onCorrect;
		   set => _onCorrect = value;
		}		
		/// <summary>
		/// Reset 이벤트
		/// </summary>
		public UnityEvent OnReset
		{
		   get => _onReset;
		   set => _onReset = value;
		}
		#endregion

		#region	Events
		public virtual void OnInput(string str)
		{
			if(isPause || !Available)
				return;
			CurrentInput += str;
		}
		#endregion
		
		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();

			CurrentInput = _currentInput;
		}
		#endregion

		#region Methods
		void CheckNumpad()
		{
			if(CurrentInput.Length >= Correction.Length)
			{
				if(CurrentInput == Correction)
				{
					NumPanel.material = CorrectMaterial;
					OnCorrect?.Invoke();

					isPause = true;
					Available = false;
				}
				else
				{
					NumPanel.material = WrongMtrl;
					OnWrong?.Invoke();

					isPause = true;

					Invoke("ResetPanel", PauseTime);
				}
			}
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			_transform = GetComponent<Transform>();
		}
		public void ResetPanel()
		{
			CurrentInput = string.Empty;
			NumPanel.material = NeutralMtrl;

			isPause = false;

			OnReset?.Invoke();
		}
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