///스크립트 생성 일자 - 2025 - 04 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using CHG.Utilities.Triggers;
using TempNamespace.InteractableObjects;
using UnityEngine;


namespace TempNamespace
{
	public class TempGunEquip : MonoBehaviour
	{
		public SimpleInteractable blueGun;
		public SimpleInteractable redGun;
		int random;
		public void TempEquip()
		{	
			random = (int)PhotonNetwork.room.CustomProperties["GunRandom"];
			
			if(PhotonNetwork.isMasterClient)
			{	
				if(random == 0)
				{
					blueGun.GetComponentInChildren<TriggerTaskScheduler>().Execute();
					Destroy(redGun.gameObject);
				}
				else
				{
					redGun.GetComponentInChildren<TriggerTaskScheduler>().Execute();
					Destroy(blueGun.gameObject);
				}
			}
			else
			{
				if(random == 0)
				{
					redGun.GetComponentInChildren<TriggerTaskScheduler>().Execute();
					Destroy(blueGun.gameObject);
				}
				else
				{
					blueGun.GetComponentInChildren<TriggerTaskScheduler>().Execute();
					Destroy(redGun.gameObject);
				}
			}
		}
	}
}