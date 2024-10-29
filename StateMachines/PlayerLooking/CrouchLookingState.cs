using System.Threading.Tasks;
using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class CrouchLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        public CrouchLookingState(PlayerLookingContext context,IBaseStateMachine<PlayerLookingStateMachine.PlayerLookingState> stateMachine) : base(stateMachine, PlayerLookingStateMachine.PlayerLookingState.Crouch)
        {
            _context = context;
        }


        protected override void UpdateState()
        {
            base.UpdateState();
          
            
            _context.CameraHolderTransform.position = new Vector3(_context.CameraHolderTransform.position.x,
                Mathf.Lerp(_context.CameraHolderTransform.position.y, _context.SlideHeadPosition.position.y,
                    Time.deltaTime * 10f), _context.CameraHolderTransform.position.z);

           
        }

     
        public override void EnterState()
        {
            _context.CrouchTime = 0f;
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