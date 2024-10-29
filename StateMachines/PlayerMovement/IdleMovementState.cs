using System.Threading.Tasks;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class IdleMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        public IdleMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base( stateMachine, PlayerMovementStateMachine.MovementState.Idle)
        {
            
            _context = context;
        }

        public override void EnterState()
        {
          
        }

 
    

        public override void ExitState()
        {
            
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