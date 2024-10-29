using System.Collections;
using StateMachines.General;
using UnityEngine;
using Utilities;

namespace StateMachines.PlayerLooking
{
    public class SlammingLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
 
        private Vector3 _initialRotation;
        public SlammingLookingState(PlayerLookingContext context, PlayerLookingStateMachine playerLookingStateMachine) : base(playerLookingStateMachine, PlayerLookingStateMachine.PlayerLookingState.Slamming)
        {
            _context = context;
            IsRootState = true;
        }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {
            _context.PlayerMovementContext.MonoBehaviour.StartCoroutine(_context.PlayerCamera.transform.Shake(0.3f, _context.ScreenShakeCurve));
            _context.XRotation = _context.CameraHolderTransform.rotation.eulerAngles.x;
            _context.YRotation = _context.CameraHolderTransform.rotation.eulerAngles.y;
        }

  
        protected override void UpdateState()
        {
            base.UpdateState(); 
            
          //  _context.CameraHolderTransform.LookAt(_context.PlayerMovementContext.SlamDirection*100);
          var lookRotation = Quaternion.LookRotation(_context.PlayerMovementContext.SlamDirection - _context.PlayerMovementContext.SlamImpactPoint);
          _context.CameraHolderTransform.rotation = Quaternion.Slerp(_context.CameraHolderTransform.rotation, lookRotation, Time.deltaTime * 15f);

        }

        protected override void CheckSubStates()
        {

        }

        protected override void CheckSwitchState()
        {
            if (!_context.PlayerMovementContext.IsSlamming)
            {
                SwitchState(PlayerLookingStateMachine.PlayerLookingState.Default);
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