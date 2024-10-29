using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerMovement
{
    public class GroundedMovementState :  BaseState<PlayerMovementStateMachine.MovementState>
    {
     
        private readonly PlayerMovementContext _context;


        public override void EnterState()
        {
   
            _context.Rb.linearDamping = _context.GroundDrag;
            
        }

  
        protected override void FixedUpdateState()
        {
            base.FixedUpdateState();
            
            _context.Rb.AddForce(_context.MoveDirection.normalized * _context.Speed, ForceMode.Force);
        }
        
     
        
        public override void ExitState()
        {
            
        }

     
        
        protected override void CheckSubStates()
        {
    
            if(_context.InputActions.Player.Sprint.triggered && _context.DashesRemaining > 0 && !_context.Crouching)
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

        public GroundedMovementState(PlayerMovementContext context, IBaseStateMachine<PlayerMovementStateMachine.MovementState> stateMachine) : base(stateMachine, PlayerMovementStateMachine.MovementState.Grounded)
        {
            _context = context;
        }
    }
}