///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using UnityEngine;
using UnityEngine.Events;

namespace TempNamespace.InteractableObjects
{
	public interface IInteractable
	{
		/// <summary>
		/// 상호작용 Transform
		/// </summary>
		Transform transform {get;}

		/// <summary>
		/// 현재 상호작용 가능 여부
		/// </summary>
		bool IsInteractable {get; set;}
		
		/// <summary>
		/// 현재 상호작용중인지 여부
		/// </summary>
		bool IsInteracting {get; set;}
		
		/// <summary>
		/// 상호작용
		/// </summary>
		bool Interact(GameObject interactor);

		/// <summary>
		/// 포커스
		/// </summary>
		void Focus();
		/// <summary>
		/// 포커스 해제
		/// </summary>
		void Unfocus();
	}	
}