///스크립트 생성 일자 - 2025 - 03 - 31
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.4

using TempNamespace.Character.Equipment;
using TempNamespace.Items;
using UnityEngine;


namespace TempNamespace.Character.Equipment
{
	public abstract class Equipment : MonoBehaviour, IEquippable
	{
		protected Character _equippedCharacter;
        public Character EquippedCharacter => _equippedCharacter;

		[SerializeField, Tooltip("장비의 종류")]
		protected EquipmentType _equipmentType;
        public EquipmentType EquipmentType => _equipmentType;

        public abstract void OnEquip(Character character);

        public abstract void OnUnequip(Character character);
    }
}