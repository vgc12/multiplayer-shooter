using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class AirMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        public AirMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base( stateMachine, PlayerMovementStateMachine.MovementState.Air)
        {
            _context = context;
        }

        public override void EnterState()
        {
          
            _context.Rb.useGravity = true;
            _context.Rb.linearDamping = _context.AirDrag;
        }



        
        protected override void FixedUpdateState()
        {
            base.FixedUpdateState();
            _context.Rb.AddForce(_context.MoveDirection.normalized * (_context.Speed * _context.AirMultiplier), ForceMode.Force);
        
        }
        
        
       
        
        public override void ExitState()
        {
            
        }

        protected override void CheckSubStates()
        {
      
            
            if(_context.InputActions.Player.Sprint.triggered && _context.DashesRemaining > 0 )
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Dash);
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
    }
}