///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using CHG.EventDriven;
using TempNamespace.EventDriven;
using TempNamespace.EventDriven.Arguments;
using TMPro;
using UnityEngine;


namespace TempNamespace
{
	/// <summary>
	/// TODO: Event Test용 임시 UI. 추후 추가 작업할 것
	/// </summary>
	public class InteractorUI : MonoBehaviour
	{
		#region Inspector Fields
		[SerializeField, Tooltip("Temp Test Text")]
		private TextMeshProUGUI _text;
		
		/// <summary>
		/// Temp Test Text
		/// </summary>
		public TextMeshProUGUI Text
		{
			get => _text;
			set => _text = value;
		}
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
		}
        void OnEnable()
        {
            GlobalEventManager.Instance.Subscribe<InteractorMessageArgs>("FocusInteractor", SetText);
			GlobalEventManager.Instance.Subscribe("UnfocusInteractor", Clear);
        }
        void OnDisable()
        {
            if(GlobalEventManager.IsAvailable)
			{
				GlobalEventManager.Instance.Unsubscribe<InteractorMessageArgs>("FocusInteractor", SetText);
				GlobalEventManager.Instance.Unsubscribe("UnfocusInteractor", Clear);
			}
        }
		void SetText(InteractorMessageArgs args)
		{
			Text.text = args.InteractorMessage;
		}
		void Clear()
		{
			Text.text = string.Empty;
		}
    }
}