///스크립트 생성 일자 - 2025 - 03 - 12
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using CHG.EventDriven.Arguments;
using TempNamespace.Character.Controller;
using TempNamespace.Character.Equipment;
using TempNamespace.Items;
using UnityEngine;

namespace TempNamespace.EventDriven.Arguments
{	
    public class EquipArgs : BaseEventArgs
	{
		public EquipArgs(int playerID, IEquippable equipment)
		{
			this.playerID = playerID;
			this.equipment = equipment;
		}

		public int playerID;
		public IEquippable equipment;
	}
    public class InteractorMessageArgs : BaseEventArgs
    {
        public InteractorMessageArgs(string interactorMessage)
        {
            this.InteractorMessage = interactorMessage;
        }

        public string InteractorMessage {get; set;}
    }
    public class PauseControlArgs : BaseEventArgs
    {
        /// <summary>
        /// 캐릭터 컨트롤을 일시정지
        /// </summary>
        public PauseControlArgs()
        {
        }
    }
    public class ResumeControlArgs : BaseEventArgs
    {
        /// <summary>
        /// 캐릭터 컨트롤을 재개
        /// </summary>
        public ResumeControlArgs()
        {
        }
    }


    /// <summary>
    /// 카메라 설정 변경 요청
    /// </summary>
    public class ChangeCameraFocusArgs : BaseEventArgs
    {
        public Transform newTarget;
        public ChangeCameraFocusArgs(Transform newTarget)
        {
            this.newTarget = newTarget;
        }
    }

    public class ChangePerspectiveArgs : BaseEventArgs
    {
        public DynamicPerspectiveController.ControllerPerspective newPerspective;
        public ChangePerspectiveArgs(DynamicPerspectiveController.ControllerPerspective newPerspective)
        {
            this.newPerspective = newPerspective;
        }
    }

    public class RagdollArgs : BaseEventArgs
    {
        public Character.Character character;
        public bool isOn;

        public RagdollArgs(Character.Character target, bool isOn)
        {
            this.character = target;
            this.isOn = isOn;
        }
    }
}