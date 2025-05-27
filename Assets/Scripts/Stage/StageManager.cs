///스크립트 생성 일자 - 2025 - 03 - 10
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using CHG.EventDriven;
using CHG.Utilities.Patterns;
using TempNamespace.Objects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TempNamespace
{
	public class StageManager : SingletonMonobehaviour<StageManager>
	{
        public override bool IsPersistent => false;

        #region Inspector Fields
		[SerializeField, Tooltip("다음 Stage Scene의 이름")]
		private string _nextSceneName;
		
		/// <summary>
		/// 다음 Stage Scene의 이름
		/// </summary>
		public string NextSceneName
		{
		   get => _nextSceneName;
		   set => _nextSceneName = value;
		}

        [SerializeField, Tooltip("Scene 시작시 플레이어가 위치할 지점(Elevator 로딩중이라면 Elevator 위치)")]
		private Transform _entryPoint;
		
		[SerializeField, Tooltip("StageManager Start시 호출")]
		private UnityEvent _onStart;
		
		[SerializeField, Tooltip("Stage Clear시 호출")]
		private UnityEvent _onClear;
		
		#endregion

		#region Fields
		
		#endregion
		
		#region Properties
		/// <summary>
		/// StageManager Start시 호출
		/// </summary>
		public UnityEvent OnStart
		{
			get => _onStart;
			set => _onStart = value;
		}
		/// <summary>
		///"Stage Clear시 호출
		/// </summary>
		public UnityEvent OnClear
		{
		   get => _onClear;
		   set => _onClear = value;
		}

		/// <summary>
		/// Scene 시작시 플레이어가 위치할 지점(Elevator 로딩중이라면 Elevator 위치)
		/// </summary>
		public Transform EntryPoint
		{
			get => _entryPoint;
			set => _entryPoint = value;
		}
		#endregion
		
		#region Methods
		public virtual void LoadNewLevel(int nextLevel)
		{			
			SceneManager.LoadSceneAsync(nextLevel);
		}
		public virtual void LoadNewLevel(string nextLevelName)
		{
			SceneManager.LoadSceneAsync(nextLevelName);
		}
		public virtual void ResetStage()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		public virtual void ClearStage()
		{
			OnClear?.Invoke();
			LoadNewLevel(NextSceneName);
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
			
		}
		#endregion

		#region MonoBehaviour Methods
		protected virtual void Awake()
		{
			CacheComponents();
		}
        protected override void Start()
        {
            base.Start();
			OnStart?.Invoke();
        }

        protected virtual void OnEnable()
        {
			GlobalEventManager.Instance.Subscribe("ResetStage", ResetStage);
            GlobalEventManager.Instance.Subscribe("ClearStage", ClearStage);
        }
        protected virtual void OnDisable()
        {
            if(GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe("ResetStage", ResetStage);
				GlobalEventManager.Instance.Unsubscribe("ClearStage", ClearStage);
			}
        }
        #endregion
    }
}