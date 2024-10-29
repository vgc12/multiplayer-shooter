using System.Threading.Tasks;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class DashMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        public DashMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base( stateMachine, PlayerMovementStateMachine.MovementState.Dash)
        {
            
            _context = context;
        }

        public override async void EnterState()
        {
           
            _context.DashPressed = false;
            
            _context.Rb.AddForce(_context.DashForce  * _context.Orientation.forward , ForceMode.Impulse);
            _context.DashesRemaining--;
            

            await ResetJumpAsync();
      
        }

        private async Task ResetJumpAsync()
        {
            await Awaitable.WaitForSecondsAsync(.4f);
            _context.ReadyToJump = true;
        }
    

        public override void ExitState()
        {
            
        }

        protected override void CheckSubStates()
        {
           
        }

        protected override void CheckSwitchState()
        {
            SwitchState(PlayerMovementStateMachine.MovementState.Idle);
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