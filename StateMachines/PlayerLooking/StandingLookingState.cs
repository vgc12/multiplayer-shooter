using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class StandingLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        public StandingLookingState(PlayerLookingContext context,IBaseStateMachine<PlayerLookingStateMachine.PlayerLookingState> stateMachine) : base(stateMachine, PlayerLookingStateMachine.PlayerLookingState.Standing)
        {
            _context = context;
        }


        protected override void UpdateState()
        {
            base.UpdateState();
          
            if(!_context.PlayerMovementContext.ObjectAbove)
                _context.CameraHolderTransform.position = _context.InitialHeadPosition.position;

       
        }

     
        public override void EnterState()
        {
           
        }

        public override void ExitState()
        {
            
        }

        protected override void CheckSubStates()
        {
            SetSubState(PlayerLookingStateMachine.PlayerLookingState.Walking);
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