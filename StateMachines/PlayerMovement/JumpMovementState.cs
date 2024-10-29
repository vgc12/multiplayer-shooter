using System.Threading.Tasks;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class JumpMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        public JumpMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base( stateMachine, PlayerMovementStateMachine.MovementState.Jumping)
        {
            
            _context = context;
        }

        public override async void EnterState()
        {
                
            _context.CrouchPressed = false;
            _context.ShouldNotBeCrouching = true;
            _context.ReadyToJump = false;
            _context.JumpPressed = false;
            
         
            _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, 0f, _context.Rb.linearVelocity.z);
      
            _context.Rb.AddForce(Vector3.up * _context.JumpForce, ForceMode.Impulse);
            

            _context.SlamForce = 0;
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
            if(!_context.Grounded && _context.MoveInput.y > 0 && (_context.WallRight|| _context.WallLeft) && _context.CanWallRun)
            {
                SwitchState(PlayerMovementStateMachine.MovementState.WallRunning);
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