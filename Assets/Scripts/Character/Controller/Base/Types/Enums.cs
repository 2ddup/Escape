
using System;

///스크립트 생성 일자 - 2025 - 03 - 19
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1.1
namespace TempNamespace.Character.Controller
{
    /// <summary>
    /// 이동 상태의 Flag
    /// </summary>
	[Flags]
	public enum MoveStateFlag
	{   
        /// <summary>
        /// 값 없음
        /// </summary>
        None = 0,
        /// <summary>
        /// 전후좌우 이동 기능 포함
        /// </summary>
		Locomotive = 1 << 0,
        /// <summary>
        /// 중력을 적용하지 않음
        /// </summary>
		DontUseGravity = 1 << 1,
        /// <summary>
        /// 지면 위에 있음
        /// </summary>
		OnGround = 1 << 2,
    }

	/// <summary>
    /// 축 기반 제한 Enum
    /// </summary>
    public enum AxisConstraint
    {
        None,
        ConstrainXAxis,
        ConstrainYAxis,
        ConstrainZAxis
    }

    /// <summary>
    /// 충돌한 방향(캐릭터 캡슐 기준)
    /// </summary>
    
    public enum HitLocation
    {
        None = 0,
        Sides = 1,
        Above = 2,
        Below = 4,
    }

    /// <summary>
    /// 이동 가능 여부 Flag
    /// </summary>

    [Flags]
    public enum MobilityFlags
    {
        Default = 0,
        
        /// <summary>
        /// 걸어갈 수 있는가?
        /// </summary>

        Walkable = 1 << 0,
        NotWalkable = 1 << 1,

        /// <summary>
        /// 서 있을 수 있는가?
        /// </summary>

        Perchable = 1 << 2,
        NotPerchable = 1 << 3,

        /// <summary>
        /// 밟고 올라갈 수 있는가?
        /// </summary>

        CanStepOn = 1 << 4,
        CanNotStepOn = 1 << 5,

        /// <summary>
        /// 타고 올라갈 수 있는가?
        /// </summary>

        CanRideOn = 1 << 6,
        CanNotRideOn = 1 << 7
    }	
    public enum InputType
	{
		KeyDown,
		Pushing,
		KeyUp
	}
}