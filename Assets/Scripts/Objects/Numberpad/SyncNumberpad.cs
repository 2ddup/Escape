///스크립트 생성 일자 - 2025 - 04 - 09
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using UnityEngine;


namespace TempNamespace
{
	public class SyncNumberpad : MonoBehaviour
	{
		[SerializeField, Tooltip("동기화할 Numberpad")]
		private Numberpad _targetNumberpad;
		
		/// <summary>
		/// 동기화할 Numberpad
		/// </summary>
		public Numberpad TargetNumberpad
		{
		   get => _targetNumberpad;
		   set => _targetNumberpad = value;
		}
		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(stream.isWriting)
			{
				stream.SendNext(TargetNumberpad.CurrentInput);
			}
			else
			{
				TargetNumberpad.CurrentInput = (string)stream.ReceiveNext();
			}
		}
	}
}