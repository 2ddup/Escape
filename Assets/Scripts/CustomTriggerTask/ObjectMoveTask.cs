///스크립트 생성 일자 - 2025 - 03 - 11
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using CHG.Utilities.Triggers;
using DG.Tweening;


namespace TempNamespace
{
	public class ObjectMoveTask : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("이동할 Transform")]
		private Transform _targetTransform;
		
		[SerializeField, Tooltip("목표 지점")]
		private Vector3 _targetPosition;

		[SerializeField, Tooltip("이동 시간(0 = 즉시 이동)")]
		private float _duration;
		

		[SerializeField, Tooltip("World 좌표 기준인가?")]
		private bool _isWorldPosition;
		
		/// <summary>
		/// World 좌표 기준인가?
		/// </summary>
		public bool IsWorldPosition
		{
			get => _isWorldPosition;
			set => _isWorldPosition = value;
		}
		
		#endregion

		#region Fields
		private bool moveCompleted = false;
		#endregion

		#region Properties
		/// <summary>
		/// 이동할 Transform
		/// </summary>
		public Transform TargetTransform
		{
			get => _targetTransform;
			set => _targetTransform = value;
		}
		/// <summary>
		/// 목표 지점
		/// </summary>
		public Vector3 TargetPosition
		{
			get => _targetPosition;
			set => _targetPosition = value;
		}
		/// <summary>
		/// 이동 시간(0 =  즉시 이동)
		/// </summary>
		public float Duration
		{
		   get => _duration;
		   set => _duration = value;
		}
        #endregion

        #region Methods
        protected override void OnTaskStart()
        {
            base.OnTaskStart();
			if(Duration != 0)
			{
				if(IsWorldPosition)
				{
					TargetTransform.DOMove(TargetPosition, Duration).OnComplete(() => {moveCompleted = true;});
				}
				else
				{
					TargetTransform.DOLocalMove(TargetPosition, Duration).OnComplete(() => {moveCompleted = true;});
				}
			}
			else
			{
				if(IsWorldPosition)
					TargetTransform.position = TargetPosition;
				else
					TargetTransform.localPosition = TargetPosition;
				
				moveCompleted = true;
			}
        }
        protected override TaskResult Execute()
		{
			if(moveCompleted)
				return TaskResult.Completed;
			else
				return TaskResult.Running;
		}
        protected override void OnTaskStop()
        {
            base.OnTaskStop();
        }
        #endregion
    }
}