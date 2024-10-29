using System;
using System.Collections;
using System.Threading.Tasks;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines
{
    public class SlidingMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;
        private readonly bool _canStopSliding = false;
        private float _initialMagnitude = 0.5f;
        
        public SlidingMovementState(PlayerMovementContext context, PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine, PlayerMovementStateMachine.MovementState.Sliding)
        {
            _context = context;
        }

        
        public override void EnterState()
        {
  
            
            //await SlideTimerAsync();
           _initialMagnitude = _context.Rb.linearVelocity.magnitude;

            _context.SlideDirection = _context.Orientation.forward.normalized;
          
            _context.IsSliding = true;
            
            _context.CrouchTime = 0;
        }

       

        protected override void FixedUpdateState()
        {
          
            _context.MoveInput = _context.InputActions.Player.Move.ReadValue<Vector2>();

            _context.MoveInput = new Vector2(_context.MoveInput.x, 0);
            
            _context.MoveDirection = (_context.Orientation.right * _context.MoveInput.x);
            _context.Rb.AddForce(_context.MoveDirection.normalized * 30, ForceMode.Force);
            SlidingMovement();
  
          

            if (_context.PreviousSpeed - _context.Rb.linearVelocity.magnitude > _context.SpeedChangeThreshold && _canStopSliding)
            {
                _context.CanSlide = false;
                _context.ShouldNotBeCrouching = true;
            }
            
            _context.PreviousSpeed = _context.Rb.linearVelocity.magnitude;
        }

        private void SlidingMovement()
        {
            if (_context.OnSlope)
            {
                var dir = _context.GetSlopeMovementDirection(_context.SlideDirection);
                _context.Rb.AddForce(dir * _context.SlideForce, ForceMode.Force);
                return;
            }
            _context.Rb.AddForce(_context.SlideDirection * (_context.SlideForce + _initialMagnitude ), ForceMode.Force);
            
        }

        public override void ExitState()
        {

            
            _context.PreviousSpeed = 0;
          
            _context.IsSliding = false;
          
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