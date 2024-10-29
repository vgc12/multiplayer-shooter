using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerMovement
{
    public class PlayerMovementBaseState : BaseState<PlayerMovementStateMachine.MovementState> 
    {
        private readonly PlayerMovementContext _context;
        
            
        public PlayerMovementBaseState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base(stateMachine, PlayerMovementStateMachine.MovementState.Default)
        {
            _context = context;
            
            IsRootState = true;
        }

        

        public override void EnterState()
        {
            
            _context.Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        protected override void UpdateState()
        {
            base.UpdateState();
            UpdateDashTimer();
            GetMovementInput();
            CheckForWall();

     
        }

        protected override void FixedUpdateState()
        {
            base.FixedUpdateState();
            ControlSpeed();
            if (_context.Rb.useGravity)
            {
                HandleFalling();
            }
        }

        private void GetMovementInput()
        {
            _context.MoveInput = _context.InputActions.Player.Move.ReadValue<Vector2>();

            _context.MoveDirection = (_context.Orientation.forward * _context.MoveInput.y) +
                                     (_context.Orientation.right * _context.MoveInput.x);

            if (_context.InputActions.Player.Crouch.IsPressed() && !_context.Crouching)
            {
                Crouch();
            }
            else
            {
                UnCrouch();
            }
        }
        
        private void Crouch()
        {
            if(!_context.Grounded || !_context.CrouchRePressed)
                return;
            _context.CrouchPressed = true;
            _context.CanSlide = true;
            _context.ShouldNotBeCrouching = false;

        }
        
        private void UnCrouch()
        {
            _context.CrouchPressed = false;
         
            _context.ShouldNotBeCrouching = true;
        }

        private void UpdateDashTimer()
        {
            _context.DashTimer += Time.deltaTime;
           
            if (!(_context.DashTimer > _context.DashRegenTime) || _context.DashesRemaining >= _context.MaxDashes) return;
            
            _context.DashesRemaining++;
            _context.DashTimer = 0;
        }

        private void ControlSpeed()
        {

            if (_context.OnSlope)
            {
                return;
            }
            Vector3 flatVel = new Vector3(_context.Rb.linearVelocity.x, 0f, _context.Rb.linearVelocity.z);

           
            if (flatVel.magnitude > _context.Speed)
            {
                Vector3 limitedVel = flatVel.normalized * _context.Speed;
                _context.Rb.linearVelocity = new Vector3(limitedVel.x, _context.Rb.linearVelocity.y, limitedVel.z);
            }
            
        }
        private void CheckForWall()
        {
            _context.WallRight = Physics.Raycast(_context.Orientation.position, _context.Orientation.right, out _context.RightWallHit, _context.WallCheckDistance, _context.PlayerGamePlayInfo.wallLayerMask);
         
            _context.WallLeft = Physics.Raycast(_context.Orientation.position, -_context.Orientation.right, out _context.LeftWallHit, _context.WallCheckDistance, _context.PlayerGamePlayInfo.wallLayerMask);
        }


 
        private void HandleFalling()
        {
        
            if (_context.Rb.linearVelocity.y < 0)
            {
          
                _context.Rb.linearVelocity +=  ((_context.FallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up).normalized;
            }
        }

  

        public override void ExitState()
        {
           
        }


        protected override void CheckSubStates()
        {
            var grounded = _context.Grounded;
         
            if (_context.IsPaused)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Paused);
            }
            else if (_context.IsDead)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Dead);
            }
            else if(!grounded && _context.HeightFromGround >=_context.MinJumpHeight && _context.MoveInput.y > 0 && (_context.WallRight|| _context.WallLeft) && _context.CanWallRun)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.WallRunning);
            }
            else if ( _context.ShouldBeSlamming && !grounded)
            {
                UnCrouch();
                SetSubState(PlayerMovementStateMachine.MovementState.GroundSlam);
            }
            else if (_context.CrouchPressed && !_context.JumpPressed )
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Crouching);
            }
            else if(!grounded)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Air);
            }
            else if(_context.JumpPressed && _context.ReadyToJump & !_context.ObjectAbove)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Jumping);
            }
            else if(_context.OnSlope)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Slope);
            }
            else
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Grounded);
            }
        }

        protected override void CheckSwitchState()
        {
         
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