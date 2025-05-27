///스크립트 생성 일자 - 2025 - 04 - 08
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using TempNamespace.Objects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TempNamespace
{
	[RequireComponent(typeof(PhotonView))]
	public class NetworkStageManager : StageManager
	{
        public override bool IsPersistent => false;

        #region Inspector Fields
		#endregion

		#region Fields
		int clearedPlayer = 0;
		private PhotonView _photonView;
		#endregion
		
		#region Properties
		public PhotonView View
		{
			get => _photonView;
			set => _photonView = value;
		}

		#endregion
		
		#region Methods
		public override void LoadNewLevel(int nextLevel)
		{			
			PhotonNetwork.LoadLevelAsync(nextLevel);
		}
		public override void LoadNewLevel(string nextLevelName)
		{
			PhotonNetwork.LoadLevelAsync(nextLevelName);
		}
        public override void ClearStage()
        {
			View.RPC("AddClearCount", PhotonTargets.MasterClient);
        }
        public override void ResetStage()
        {
			PhotonNetwork.LoadLevelAsync(SceneManager.GetActiveScene().name);
        }

		[PunRPC]
		void AddClearCount()
		{
			++clearedPlayer;
			if(clearedPlayer == PhotonNetwork.playerList.Length)
			{
				LoadNewLevel(NextSceneName);
			}
		}

		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected override void CacheComponents()
		{
			base.CacheComponents();
			_photonView = GetComponent<PhotonView>();
		}
		#endregion

		#region MonoBehaviour Methods
		protected override void Awake()
		{
			CacheComponents();
		}
        #endregion
    }
}