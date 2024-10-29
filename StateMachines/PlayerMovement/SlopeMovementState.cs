 using StateMachines.General;
 using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class SlopeMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        public SlopeMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base(stateMachine, PlayerMovementStateMachine.MovementState.Slope)
        {
            _context = context;
            
        }

        public override void EnterState()
        {
            Debug.Log("slope");
            _context.Rb.useGravity =false;
            _context.ExitingSlope = false;
        }
        
        protected override void FixedUpdateState()
        {
            
            ControlSpeed();
            
            _context.Rb.AddForce(_context.GetSlopeMovementDirection(_context.MoveDirection) * _context.Speed, ForceMode.Force);

            if (_context.Rb.linearVelocity.y > 0)
            {
                   
                _context.Rb.AddForce(Vector3.down * 8f, ForceMode.Force);
            }
         
        }

        
        public override void ExitState()
        {
            _context.ExitingSlope = true;
            _context.Rb.useGravity = true;
        }

        protected override void CheckSubStates()
        {
            if(_context.DashPressed && _context.DashesRemaining > 0 )
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Dash);
            }
            else if(_context.ReadyToJump && _context.JumpPressed)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Jumping);
            }
            else if(_context.Crouching && !_context.CrouchPressed)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Crouching);
            }
            else
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Idle);
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

        private void ControlSpeed()
        {
            if (_context.Rb.linearVelocity.magnitude > _context.Speed)
                _context.Rb.linearVelocity = _context.Rb.linearVelocity.normalized * _context.Speed;
        }
    }
}