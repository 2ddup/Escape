///스크립트 생성 일자 - 2025 - 02 - 27
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using TempNamespace.Utilities;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
	public class FirstPersonInput : MonoBehaviour
	{

		[Header("Mouse Rotation")]
        public bool invertLook = true;
        [Tooltip("Mouse look sensitivity")]
        public Vector2 mouseSensitivity = new Vector2(1.0f, 1.0f);
        
        [Tooltip("How far in degrees can you move the camera down.")]
        public float minPitch = -80.0f;
        [Tooltip("How far in degrees can you move the camera up.")]
        public float maxPitch = 80.0f;
        
        private FirstPersonController _character;

		bool isMouseRotating;
		public bool IsMouseRotating
		{
			get => isMouseRotating;
			set
			{
				if(value == false)
				{
					Cursor.lockState = CursorLockMode.None;
				}
				else
				{
					Cursor.lockState = CursorLockMode.Locked;
				}

				isMouseRotating = value;
			}
		}

        private void Start()
        {
			IsMouseRotating = true;
        }

		private void Update()
        {
			if(_character.IsPaused)
			{
				IsMouseRotating = false;
				return;
			}

			if(Input.GetKeyDown(KeyCode.Q))
			{
				IsMouseRotating = !IsMouseRotating;
			}

            
            // // // Crouch input            
            // if(Input.GetKeyDown(KeyCode.C))
            // {
            //     _character.InputEvents.Add("CrouchToggle");
            // }
            
            // // Jump input
            
            // if (Input.GetButtonDown("Jump"))
            //     _character.InputEvents.Add("JumpStart");
            // else if (Input.GetButtonUp("Jump"))
            //     _character.InputEvents.Add("JumpEnd");

            if(!IsMouseRotating)
                return;
            
            

			// MovementInput();			
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            Vector3 movementDirection = Vector3.zero;
            
            movementDirection += Vector3.forward * inputMove.y;
            movementDirection += Vector3.right * inputMove.x;
            
            movementDirection = movementDirection.RelativeTo(_character.cameraTransform, _character.GetUpVector());
            
            _character.SetMovementDirection(movementDirection);
            
			
            // //if (_character.IsFlying())
            // {                
            //     movementDirection = Vector3.zero;
                
            //     // Strafe
                
            //     movementDirection += _character.GetRightVector() * inputMove.x;
                
            //     // Forward, along camera view direction (if any) or along character's forward if camera not found 

            //     Vector3 forward = _character.camera 
            //         ? _character.cameraTransform.forward 
            //         : _character.GetForwardVector();
                
            //     movementDirection += forward * inputMove.y;
                
            //     // Vertical movement
                
            //     // if (_character.jumpInputPressed)
            //     //     movementDirection += Vector3.up;
                
            //     _character.SetMovementDirection(movementDirection);
            // }
        }
		void Awake()
		{
			CacheComponents();
		}
		/// <summary>
		/// 컴퍼넌트를 캐싱
		/// </summary>	
		protected virtual void CacheComponents()
		{
            _character = GetComponent<FirstPersonController>();
		}
	}
}