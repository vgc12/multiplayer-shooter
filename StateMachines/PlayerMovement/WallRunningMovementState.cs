using System.Threading.Tasks;
using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerMovement
{
    public class WallRunningMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
       // private float initialMagnitude;
        public WallRunningMovementState(PlayerMovementContext context, PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine, PlayerMovementStateMachine.MovementState.WallRunning)
        {
            _context = context;
        }

        public override void EnterState()
        {
           // initialMagnitude = _context.Rb.linearVelocity.magnitude;
           _context.IsWallRunning = true;
            _context.Rb.linearDamping = _context.AirDrag;
            _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, 0f, _context.Rb.linearVelocity.z);
            _context.Rb.useGravity = false;
        }

        protected override void UpdateState()
        {
            if (_context.JumpPressed)
            {
                WallJump();
            }
        }

        protected override void FixedUpdateState()
        {
            WallRunningMovement();
          
        }

        private void WallRunningMovement()
        {
            
            var wallNormal = _context.WallRight ? _context.RightWallHit.normal :_context.LeftWallHit.normal;
     
        
            var wallForward = Vector3.Cross(wallNormal, _context.Orientation.up);


            if ((_context.Orientation.forward - wallForward).magnitude > (_context.Orientation.forward - -wallForward).magnitude)
                wallForward = -wallForward;

           
            if (Vector3.Angle(wallForward, _context.Orientation.forward) > 90 ||
                _context.PreviousSpeed - _context.Rb.linearVelocity.magnitude > _context.WallRunSpeedChangeThreshold || _context.Rb.linearVelocity.magnitude < 1f) 
            {
         
                _context.ExitWallTimer = 1f;
                _context.CanWallRun = false;
                _context.Rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
                return;
            }

            _context.PreviousSpeed = _context.Rb.linearVelocity.magnitude;
         
            _context.Rb.AddForce(wallForward *  _context.WallRunForce , ForceMode.Force);
        
        
            if (_context.UpwardsRunning)
                _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, _context.WallClimbSpeed, _context.Rb.linearVelocity.z);
            
            if (_context.DownwardsRunning)
                _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, -_context.WallClimbSpeed, _context.Rb.linearVelocity.z);
   
        
            if (!(_context.WallLeft && _context.MoveInput.x > 0) && !(_context.WallRight && _context.MoveInput.x < 0))
                _context.Rb.AddForce(-wallNormal * 100, ForceMode.Force);

           
        
        }


  


        private void WallJump()
        {
            _context.Rb.useGravity = true;
            _context.ExitWallTimer = _context.ExitWallTime;
            _context.JumpPressed = false;
            _context.CanWallRun = false;
        
  
            var wallNormal = _context.WallRight ? _context.RightWallHit.normal : _context.LeftWallHit.normal;
            var forceToApply = _context.Orientation.up * _context.WallJumpUpForce + wallNormal * _context.WallJumpSideForce;
            _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, 0f, _context.Rb.linearVelocity.z);
            _context.Rb.AddForce(forceToApply, ForceMode.Impulse);
        
        }

        public override async void ExitState()
        {
            _context.IsWallRunning = false;
            _context.Rb.useGravity = true;
            await ExitWallAsync();
        }

        private async Task ExitWallAsync()
        {
       
            await Awaitable.WaitForSecondsAsync(_context.ExitWallTimer);
            _context.CanWallRun = true;
        }
        
        protected override void CheckSubStates()
        {
           
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