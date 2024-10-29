using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerLooking
{
    public class DeadLookingState : BaseState<PlayerLookingStateMachine.PlayerLookingState>
    {
        private readonly PlayerLookingContext _context;
        private Transform _ragdollTorso;
        public DeadLookingState(PlayerLookingContext context, PlayerLookingStateMachine playerLookingStateMachine) : base(playerLookingStateMachine, PlayerLookingStateMachine.PlayerLookingState.Dead)
        {
            _context = context;
            IsRootState = true;
        }

        public override void EnterState()
        {

            _ragdollTorso = _context.LatestRagdollObject.GetComponent<PlayerDeathRagdoll>().torso;


        }


        protected override void UpdateState()
        {
            base.UpdateState();
            _context.RotationTarget = _context.ThirdPersonRotationObject;
           // _context.ThirdPersonRotationObject.position = _context.LatestRagdollObject.transform.position;
            
            _context.PlayerCamera.transform.position = _context.ThirdPersonPositionTransform.position;
            _context.ThirdPersonRotationObject.position = Vector3.Slerp(_context.RotationTarget.position, _ragdollTorso.position, Time.deltaTime * 3);
            _context.PlayerCamera.transform.LookAt(_ragdollTorso);
            _context.MouseDelta = _context.InputActions.Player.Look.ReadValue<Vector2>();
          
            
            var mouseX = _context.MouseDelta.x * Time.deltaTime * _context.SensX;
            var mouseY = _context.MouseDelta.y * Time.deltaTime * _context.SensY;

            _context.YRotation += mouseX;
            _context.XRotation -= mouseY;

            _context.XRotation = Mathf.Clamp(_context.XRotation, 2, 88f);

            _context.RotationTarget.rotation = Quaternion.Euler(_context.XRotation, _context.YRotation, 0);
          //  _context.Orientation.rotation = Quaternion.Euler(0, _context.YRotation, 0);
           // _context.SlideHeadPosition.rotation = _context.Orientation.rotation;
        }

        public override void ExitState()
        {
            
        }

        protected override void CheckSubStates()
        {
            
        }

        protected override void CheckSwitchState()
        {
            if (!_context.IsDead)
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