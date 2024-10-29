using System.Collections;
using EventChannels;
using UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Player
{
   
    public class WallRunning : NetworkBehaviour
    {
        [Header("Wall Running")]
        //[ScriptableObjectDropdown]
        [SerializeField]
        private PlayerGamePlayInfoScriptableObject playerGamePlayInfo;
   
        public float wallRunForce;
        private bool _canWallRun;
        public float wallClimbSpeed = 5f;
        public float wallJumpUpForce = 5f;
        public float wallJumpSideForce = 5f;
        private bool _kickedOffWall;
    
        [Header("input")]
        private PlayerInputActions _playerInputActions;

        [Header("Detection")] [SerializeField] private float wallCheckDistance;
        [SerializeField] private float minJumpHeight;
        private RaycastHit _leftWallHit;
        internal RaycastHit LeftWallHit => _leftWallHit;
        private RaycastHit _rightWallHit;
        internal RaycastHit RightWallHit => _rightWallHit;
        public bool WallLeft { get; private set; }
        public bool WallRight { get; private set; }
        [SerializeField] private Transform orientation;
        private PlayerMovement _playerMovement;
        private PlayerLooking _playerLooking;
        private Rigidbody _rb;
        private Vector2 _direction;
        private Collider _collider;
    
    
        [Header("Effects")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float wallRunFOV;
        [SerializeField] private float wallRunRotation;
        [SerializeField] private Animator animator;


    

        [Header("Exiting")]
        [SerializeField] private float speedChangeThreshold;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;
        private float _previousSpeed;
        private bool _exitingWall;
        public float exitWallTime;
        private float _exitWallTimer;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
            }
        }

        private bool _upwardsRunning;
        private bool _downwardsRunning;
        public bool useGravity;
        [SerializeField]
        private float gravityCounterforce;

        private bool _jumpPressed;
        
      
        
        private void Awake()
        {
        
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Sprint.performed += SprintPressed;
            _playerInputActions.Player.Sprint.canceled += SprintPressed;
            _playerInputActions.Player.Crouch.performed += CrouchPressed;
            _playerInputActions.Player.Crouch.canceled += CrouchPressed;
            _playerInputActions.Player.Jump.performed += WallJump;
            _playerInputActions.Enable();
            _rb = GetComponent<Rigidbody>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerLooking = GetComponentInChildren<PlayerLooking>();
            _canWallRun = true;
            _pauseEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
            _pauseEventChannel.OnEventRaised += OnPause;
        }

        private void CrouchPressed(InputAction.CallbackContext ctx)
        {
            if(ctx.performed){
                _downwardsRunning = true;
            }
            else
            {
                _downwardsRunning = false;
            }
        }

        private void SprintPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                _upwardsRunning = true;
            }
            else if(ctx.canceled)
            {
                _upwardsRunning = false;
            }
        }

        public void OnPause(PauseEvent pauseEvent)
        {
            if(!IsOwner || pauseEvent.PlayerId != OwnerClientId)
                return;

            _exitingWall = pauseEvent.IsPaused;
        }

        // Update is called once per frame
        private void Update()
        {
            CheckForWall();
        
            _direction = _playerInputActions.Player.Move.ReadValue<Vector2>();

          
        
            if (_direction.y > 0 && (WallRight || WallLeft) && AboveGround && !_exitingWall && _canWallRun)
            {
           
                if (!_playerMovement.WallRunning )
                {
                    StartWallRun();
                }
            
            
            
            }
            else if (_exitingWall)
            {
                if (_playerMovement.WallRunning)
                {
                    StopWallRun();
                }

           
            }
            else
            {
                if(_playerMovement.WallRunning)
                    StopWallRun();
            }
        }

        private void FixedUpdate()
        {
            if (_playerMovement.WallRunning)
            {
                WallRunningMovement();
            }
  
        
        }

        private void StartWallRun()
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            
            //animator.CrossFade(_wallRight? "LeanLeft" : "LeanRight", 0.1f);
           

            StartCoroutine(_playerLooking.LerpRotation(playerCamera.transform, 0,
                WallRight ? wallRunRotation : -wallRunRotation));
        
            _playerMovement.WallRunning = true;
            _previousSpeed = _rb.linearVelocity.magnitude;
            
        }
    
        private void StopWallRun()
        {
           // animator.CrossFade("Idle", 0.1f);

            StartCoroutine(_playerLooking.LerpRotation(playerCamera.transform, playerCamera.transform.rotation.z, 0));

            if (!_kickedOffWall)
            {
                _exitWallTimer = exitWallTime;
            }
        
            StartCoroutine(CountDownTimer());
            _kickedOffWall = false;
        
            _playerMovement.WallRunning = false;
        }

        private IEnumerator CountDownTimer()
        {
            _canWallRun = false;
            yield return new WaitForSeconds(_exitWallTimer);
            _canWallRun = true;
            _exitingWall = false;
        }

        private void WallRunningMovement()
        {
            _rb.useGravity = useGravity;


            var wallNormal = WallRight ? _rightWallHit.normal :_leftWallHit.normal;
     
        
            var wallForward = Vector3.Cross(wallNormal, transform.up);


            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
                wallForward = -wallForward;

           
            if (Vector3.Angle(wallForward, orientation.forward) > 90 ||
                _previousSpeed - _rb.linearVelocity.magnitude > speedChangeThreshold || _rb.linearVelocity.magnitude < 1f) 
            {
                _exitingWall = true;
                _exitWallTimer = 1f;
                _kickedOffWall = true;
                _rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
                return;
            }

            _previousSpeed = _rb.linearVelocity.magnitude;
            _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        
            if (_upwardsRunning)
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, wallClimbSpeed, _rb.linearVelocity.z);
            
            if (_downwardsRunning)
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, -wallClimbSpeed, _rb.linearVelocity.z);
   
        
            if (!(WallLeft && _direction.x > 0) && !(WallRight && _direction.x < 0))
                _rb.AddForce(-wallNormal * 100, ForceMode.Force);

            if (useGravity)
            {
                _rb.AddForce(transform.up* gravityCounterforce, ForceMode.Force);
            }
        
        }


        private void CheckForWall()
        {
            WallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, playerGamePlayInfo.wallLayerMask);
         
            WallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, playerGamePlayInfo.wallLayerMask);
        }



        private void WallJump(InputAction.CallbackContext ctx)
        {
            if(!_playerMovement.WallRunning)
                return;
            _exitingWall = true;
            _exitWallTimer = exitWallTime;
        
            Vector3 wallNormal = WallRight ? _rightWallHit.normal : _leftWallHit.normal;
            Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(forceToApply, ForceMode.Impulse);
        
        }
    
        private bool AboveGround => !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, playerGamePlayInfo.groundLayerMask);


        public override void OnDestroy()
        {
            base.OnDestroy();
            _playerInputActions.Player.Sprint.performed -= SprintPressed;
            _playerInputActions.Player.Sprint.canceled -= SprintPressed;
            _playerInputActions.Player.Crouch.performed -= CrouchPressed;
            _playerInputActions.Player.Crouch.canceled -= CrouchPressed;
            _playerInputActions.Player.Jump.performed -= WallJump;
            _pauseEventChannel.OnEventRaised -= OnPause;
        }
    }
}
