using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class DefaultLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        public DefaultLookingState(PlayerLookingContext context,IBaseStateMachine<PlayerLookingStateMachine.PlayerLookingState> stateMachine) : base(stateMachine, PlayerLookingStateMachine.PlayerLookingState.Default)
        {
            _context = context;
            IsRootState = true;
        }


        protected override void UpdateState()
        {
            base.UpdateState();
            _context.PlayerCamera.transform.localPosition = new Vector3(0, 0, 0);
            _context.RotationTarget = _context.CameraHolderTransform;
            _context.ThirdPersonRotationObject.position = _context.CameraHolderTransform.position;
            _context.ThirdPersonRotationObject.rotation = _context.CameraHolderTransform.rotation;
            

            MoveCamera();
        }

        private void MoveCamera()
        {
            
            _context.MouseDelta = _context.InputActions.Player.Look.ReadValue<Vector2>();
          
            
            var mouseX = _context.MouseDelta.x * Time.deltaTime * _context.SensX;
            var mouseY = _context.MouseDelta.y * Time.deltaTime * _context.SensY;

            _context.YRotation += mouseX;
            _context.XRotation -= mouseY;

            _context.XRotation = Mathf.Clamp(_context.XRotation, -90f, 90f);

            _context.RotationTarget.rotation = Quaternion.Euler(_context.XRotation, _context.YRotation, 0);
            _context.Orientation.rotation = Quaternion.Euler(0, _context.YRotation, 0);
            _context.SlideHeadPosition.rotation = _context.Orientation.rotation;
        }

        public override void EnterState()
        {
            _context.RotationTarget = _context.CameraHolderTransform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void ExitState()
        {
            
        }

        protected override void CheckSubStates()
        {
            if (_context.PlayerMovementContext.IsWallRunning)
            {
                SetSubState(PlayerLookingStateMachine.PlayerLookingState.WallRunning);
            }
            else if (_context.PlayerMovementContext.CrouchPressed)
            {
                SetSubState(PlayerLookingStateMachine.PlayerLookingState.Crouch);
            }
            else
            {
                SetSubState(PlayerLookingStateMachine.PlayerLookingState.Standing);
            }
        }

        protected override void CheckSwitchState()
        {
            if (_context.IsDead)
            {
                SwitchState(PlayerLookingStateMachine.PlayerLookingState.Dead);
            }
            else if(_context.PlayerMovementContext.IsSlamming)
            {
                SwitchState(PlayerLookingStateMachine.PlayerLookingState.Slamming);
            }
        }

        public override void OnTriggerEnter(Collider other)
        {
            
        }

        public override void OnTriggerExit(Collider other)
        {

        }

        public override void OnTriggerStay(Collider other)
        {
            
        }
    }
}