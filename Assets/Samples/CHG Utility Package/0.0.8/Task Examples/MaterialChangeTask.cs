///스크립트 생성 일자 - 2025 - 03 - 10
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using UnityEngine;
using CHG.Utilities.Triggers;


namespace CHG
{
	public class MaterialChangeTask : TriggerTask
	{
		#region Inspector Fields
		[SerializeField, Tooltip("Material을 변경할 Renderer")]
		private Renderer _targetRenderer;

		[SerializeField, Tooltip("새로운 Material")]
		private Material _newMaterial;
		
		/// <summary>
		///새로운 Material
		/// </summary>
		public Material NewMaterial
		{
			get => _newMaterial;
			set => _newMaterial = value;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Material을 변경할 Renderer
		/// </summary>
		public Renderer TargetRenderer
		{
			get => _targetRenderer;
			set => _targetRenderer = value;
		}
		#endregion
		
		#region Methods
		protected override TaskResult Execute()
		{
			if(TargetRenderer != null)
			{
				TargetRenderer.material = NewMaterial;
			}
			
			return TaskResult.Completed;
		}
		#endregion
	}
}