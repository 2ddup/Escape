///스크립트 생성 일자 - 2025 - 04 - 08
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using CHG.EventDriven;
using UnityEngine;


namespace TempNamespace
{
	public class GlobalEventPublisher : MonoBehaviour
	{
		public void PublishEvent(string eventName)
		{
			GlobalEventManager.Instance.Publish(eventName);
		}
	}
}