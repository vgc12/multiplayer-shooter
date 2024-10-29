using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class WalkingLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        public WalkingLookingState(PlayerLookingContext context,IBaseStateMachine<PlayerLookingStateMachine.PlayerLookingState> stateMachine) : base(stateMachine, PlayerLookingStateMachine.PlayerLookingState.Standing)
        {
            _context = context;
        }


        protected override void UpdateState()
        {
            base.UpdateState();

            _context.PlayerCamera.transform.localRotation = _context.PlayerMovementContext.MoveInput.x switch
            {
                > 0 => Quaternion.Slerp(_context.PlayerCamera.transform.localRotation,
                    Quaternion.Euler(_context.PlayerCamera.transform.localRotation.x,
                        _context.PlayerCamera.transform.localRotation.y, -5f), Time.deltaTime * 10f),
                < 0 => Quaternion.Slerp(_context.PlayerCamera.transform.localRotation,
                    Quaternion.Euler(_context.PlayerCamera.transform.localRotation.x,
                        _context.PlayerCamera.transform.localRotation.y, 5f), Time.deltaTime * 10f),
                _ => Quaternion.Slerp(_context.PlayerCamera.transform.localRotation,
                    Quaternion.Euler(_context.PlayerCamera.transform.localRotation.x,
                        _context.PlayerCamera.transform.localRotation.y, 0), Time.deltaTime * 10f)
            };
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