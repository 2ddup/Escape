///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using TempNamespace.Character.Equipment;
using UnityEngine;

namespace TempNamespace.Items
{
	using Character = TempNamespace.Character.Character;

	public interface IItem
	{
	}
	/// <summary>
	/// 사용 가능 아이템 Interface
	/// </summary>
	public interface IUsable : IItem
	{
		void Use();
	}
	/// <summary>
	/// 캐릭터의 장비 가능 아이템 Interface
	/// </summary>
	public interface IEquippable : IItem
	{
		Transform transform {get;}
		/// <summary>
		/// 현재 장비중인 Character Data
		/// </summary>
		Character EquippedCharacter {get;}
		/// <summary>
		/// 장비의 종류
		/// </summary>
		EquipmentType EquipmentType {get;}

		void OnEquip(Character character);
		void OnUnequip(Character character);
	}
}