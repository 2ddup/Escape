
using System;

///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4
namespace TempNamespace.Character.Equipment
{
	[Flags]
	public enum EquipmentType
	{
		/// <summary>
		/// 장비 종류 없음 = 사용 불가능
		/// </summary>
		None = 0,
		/// <summary>
		/// 손에 드는 물건
		/// </summary>
		Hand = 1 << 0,
		/// <summary>
		/// 등 장비(가방 등)
		/// </summary>
		Back = 1 << 1,
		/// <summary>
		/// 머리
		/// </summary>
		Head = 1 << 2
	}
}