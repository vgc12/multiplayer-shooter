using System.Collections;
using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class WallRunningLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        public WallRunningLookingState(PlayerLookingContext context,IBaseStateMachine<PlayerLookingStateMachine.PlayerLookingState> stateMachine) : base(stateMachine, PlayerLookingStateMachine.PlayerLookingState.WallRunning)
        {
            _context = context;
            
        }


        protected override void UpdateState()
        {
            base.UpdateState();

           
        }

        public IEnumerator LerpRotation(Transform tr, float from, float to)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 9f;
                tr.localRotation = Quaternion.Euler(tr.localRotation.x, tr.localRotation.y, Mathf.Lerp(from, to, t));
                yield return null;
            }
        }

        public override void EnterState()
        {
            _context.MonoBehaviour.StartCoroutine(LerpRotation(_context.PlayerCamera.transform, 0,
                _context.PlayerMovementContext.WallRight ? _context.WallRunRotation : -_context.WallRunRotation));
        }

        public override void ExitState()
        {
            _context.MonoBehaviour.StartCoroutine(LerpRotation(_context.PlayerCamera.transform, _context.PlayerCamera.transform.rotation.z,0));
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