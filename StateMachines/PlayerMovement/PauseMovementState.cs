using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class PauseMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        //This has no movement code;



        public override void EnterState()
        {
        
        }

        protected override void UpdateState()
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

        public PauseMovementState(IBaseStateMachine<PlayerMovementStateMachine.MovementState> stateMachine) : base(stateMachine, PlayerMovementStateMachine.MovementState.Paused)
        {
        }
    }
}