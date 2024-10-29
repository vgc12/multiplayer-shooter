using System.Collections.Generic;
using System.Threading.Tasks;
using EventChannels;
using Player;
using StateMachines.General;
using StateMachines.PlayerLooking;
using Unity.Netcode;
using UnityEngine;

namespace StateMachines.PlayerMovement
{
    public class SlamMovementState : BaseState<PlayerMovementStateMachine.MovementState>
    {
        private readonly PlayerMovementContext _context;

        public SlamMovementState(PlayerMovementContext context, PlayerMovementStateMachine stateMachine) : base( stateMachine, PlayerMovementStateMachine.MovementState.GroundSlam)
        {
            
            _context = context;
        }

        public override void EnterState()
        {
            
           // _context.Rb.linearVelocity = initialVelocity.normalized * 5f;
           _context.Rb.linearVelocity = Vector3.zero;
          // _context.Rb.AddForce(Vector3.down * 30f, ForceMode.Impulse);
    
            _context.ShouldBeSlamming = true;
       
            _context.SlamForce = _context.Rb.linearVelocity.magnitude;
            
            _context.SlamDirection = _context.CameraHolderTransform.forward;
            
          
            float angleFromGround = Vector3.Angle(_context.SlamDirection, Vector3.down);
            if (angleFromGround > 70f)
            {
                _context.SlamDirection = Quaternion.AngleAxis(angleFromGround - 70, _context.CameraHolderTransform.right) * _context.SlamDirection;
            }

            if (Physics.Raycast(_context.CameraHolderTransform.position, _context.SlamDirection, out RaycastHit hit, ~_context.PlayerGamePlayInfo.crouchIgnoreMask))
            {
                _context.SlamDirection = _context.Rb.position - hit.point;
                _context.SlamImpactPoint = hit.point;
            }
            _context.IsSlamming = true;
     
 

        }
        
        
        
        protected override void UpdateState()
        {
            if (_context.Rb.linearVelocity.magnitude > _context.SlamForce)
            {
                _context.SlamForce = _context.Rb.linearVelocity.magnitude;
            }
        }

        protected override void FixedUpdateState()
        {
            base.FixedUpdateState();
          
            //_context.Rb.linearVelocity +=  ((_context.FallMultiplier *2 ) * Physics.gravity.y * Time.deltaTime * Vector3.up).normalized;
         
            _context.Rb.AddForce(_context.SlamDirection* 300f, ForceMode.Force);
          
        }

        private async Task ResetSlamForceAsync()
        {
            await Awaitable.WaitForSecondsAsync(.4f);
            _context.SlamForce = 0;
        }
    
        

        public override async void ExitState()
        {
         
            InvokeSlam();
            _context.IsSlamming = false;
            _context.Rb.linearVelocity = Vector3.zero;
            _context.ShouldBeSlamming = false;
            await ResetSlamForceAsync();

        }

        private void InvokeSlam()
        {
            var players = Physics.OverlapSphere(_context.Rb.transform.position,
                _context.SlamForce < 20f ? ((_context.SlamForce / 20f) * 10) / 2 : 5f,
                _context.PlayerGamePlayInfo.groundSlamMask);
            var hitPlayers = new List<ulong>();
            
            foreach (var col in players)
            {
                var player = col.GetComponentInParent<PlayerHealth>();
                var hitPlayer = player.OwnerClientId;
                if(!player || player.OwnerClientId == _context.StateMachine.OwnerClientId || hitPlayers.Contains(hitPlayer))
                    continue;
                hitPlayers.Add(hitPlayer);
                DamageManager.Instance.TryDamagePlayerServerRpc(new PlayerHitEvent(_context.StateMachine.OwnerClientId, 50,(-(_context.StateMachine.transform.position - player.transform.position).normalized + Vector3.up)*4,
                    player.OwnerClientId, _context.SlamForce));
            
               _context.GroundSlamEventChannel.RaiseEvent( new GroundSlamEvent(hitPlayer,_context.Rb.position, _context.SlamForce));
            }
            

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