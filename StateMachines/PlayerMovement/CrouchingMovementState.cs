using System.Collections;
using StateMachines.General;
using UnityEngine;

namespace StateMachines.PlayerMovement
{
    public class CrouchingMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;

        public CrouchingMovementState(PlayerMovementContext context, PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine, PlayerMovementStateMachine.MovementState.Crouching)
        {
            _context = context;
            _context.Rb.linearDamping = _context.GroundDrag;
        }

        public override void EnterState()
        {
            _context.CrouchTime = 0;
            _context.Rb.linearDamping = _context.AirDrag;
            _context.MonoBehaviour.StopCoroutine(StopCrouching());
            _context.MonoBehaviour.StartCoroutine(Crouch());
        }



        private IEnumerator Crouch()
        {
            

            while(_context.CrouchTime < 1f )
            {
          
                if (_context.FirstPersonColliderTransform.localScale.y < _context.CrouchHeight + .05f)
                {
                    _context.FirstPersonColliderTransform.localScale = new Vector3(1, _context.CrouchHeight, 1);
                    break;
                }
                _context.CrouchTime += Time.deltaTime * 5f;
                _context.FirstPersonColliderTransform.localScale =
                    new Vector3(1f, Mathf.Lerp(_context.FirstPersonColliderTransform.localScale.y, _context.CrouchHeight, _context.CrouchTime), 1f);
                yield return null;
            }
            
        }

        protected override void UpdateState()
        {
            base.UpdateState();
            if(_context.Rb.linearVelocity.magnitude < 3f && _context.FirstPersonColliderTransform.localScale.y <= _context.CrouchHeight)
            {
                _context.CrouchPressed = false;
            }
        }

        private IEnumerator StopCrouching()
        {
            _context.CrouchTime = 0;
            while (_context.CrouchTime < 1f)
            {
               
                if (_context.ObjectAbove)
                {

                    yield return null;
                    continue;
                }   
                if (_context.FirstPersonColliderTransform.localScale.y > _context.InitialHeight - .05f)
                {
                    _context.FirstPersonColliderTransform.localScale = new Vector3(1, _context.InitialHeight, 1);
                }
                    
        
                _context.CrouchTime += Time.deltaTime * 5f;
                
                _context.FirstPersonColliderTransform.localScale =
                    new Vector3(1f, Mathf.Lerp(_context.FirstPersonColliderTransform.localScale.y, _context.InitialHeight, _context.CrouchTime), 1f);
                _context.Crouching = _context.FirstPersonColliderTransform.localScale.y < _context.InitialHeight;
                yield return null;
            }
        
        }
        
        public override void ExitState()
        {
         
           
            _context.CrouchTime = 0;
            SetSubState(PlayerMovementStateMachine.MovementState.Idle);
            _context.CrouchRePressed = false;
            _context.MonoBehaviour.StopCoroutine(Crouch());
            _context.MonoBehaviour.StartCoroutine(StopCrouching());
            _context.MonoBehaviour.StartCoroutine(SlamDelay());


            

        }

        private IEnumerator SlamDelay()
        {
            float t = 0;
            while (t < 1f)
            {
                _context.ShouldBeSlamming = false;
                t += Time.deltaTime;
                yield return null;
            }
        }

        protected override void CheckSubStates()
        {
           
            if (_context.CrouchPressed && _context.CanSlide)
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Sliding);
            }
            else if(_context.DashPressed && _context.DashesRemaining > 0 )
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Dash);
            }
            else
            {
                SetSubState(PlayerMovementStateMachine.MovementState.Idle);
            }
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