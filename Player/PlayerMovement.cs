

using System;
using System.Collections;
//using EditorScripts;
using EventChannels;
using UI;
using Unity.Netcode;
using Unity.Netcode.Components;

using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Player
{
     [RequireComponent(typeof(NetworkRigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Shared")]
        private PlayerInputActions _playerInputActions;
        private Rigidbody _rb;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;


        [Header("Movement")] 
        [SerializeField] private float sprintSpeed;
   
        
        private float _speed;
        [SerializeField]
        private float speedMultiplier = 10f;
        public Transform orientation;
        public Vector3 MoveDirection { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public float wallRunSpeed;
     
        [Header("Slope Detection")]
        [SerializeField]
        private float maxSlopeAngle = 35f;
        private RaycastHit _slopeHit;
        private bool _exitingSlope = false;

        [Header("Jumping")]


        //[ScriptableObjectDropdown]
        [SerializeField]
        private PlayerGamePlayInfoScriptableObject playerGamePlayInfo;
        [SerializeField]
        private float airMultiplier = 0.4f;
        [SerializeField]
        private float jumpForce = 5f;
        [SerializeField]
        private float fallMultiplier = 2.5f;
        private bool _objectAbove;
        private readonly Collider[] _crouchColliders = new Collider[1];
        [SerializeField] private LayerMask crouchIgnoreMask;

        [SerializeField]private int dashesRemaining;

       
        [SerializeField] private Transform crouchTransform;
   
        private float _initialHeight;
        
        [Header("Dashing")]
        [SerializeField] private int maxDashes =3;
        [SerializeField] private float dashRegenTime = 2f;
        [SerializeField] private float dashForce = 100f;
        private GenericEventChannelScriptableObject<PlayerDeathEvent> _playerDeathEventChannel;
        private float _dashTimer;
        
        [Header("Ground Detection")]
        [SerializeField] private Transform groundDetectionTransform;
        private bool _isGrounded;
        private bool _readyToJump;
        private readonly Collider[] _groundColliders = new Collider[1];
        private float _rootTransformHeight = 1f;

        [Header("Drag")]
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 2f;

        private Vector3 _networkPosition;
        private Vector3 _networkVelocity;
        private Vector3 _estimatedPosition;
        [SerializeField] private float positionErrorThreshold;
  
      
        public bool Sliding { get; set; }
        
        public bool Crouching { get; set; }
        public bool Moving { get; private set; }
        public bool WallRunning { get; set; }
        
   

        private bool _isPaused;
        


        
        public override void OnNetworkSpawn()
        { 
           // crouchTransform.GetComponent<Renderer>().material.color = Color.red;

            if (!IsOwner)
            {
                enabled = false;
            }
        }

        


        private void Awake()
        {
            
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Enable();
        
            _playerInputActions.Player.Jump.performed += Jump;
            _playerInputActions.Player.Sprint.canceled += Dash;
            _playerDeathEventChannel = EventChannelAccessor.Instance.playerDeathEventChannel;
            _playerDeathEventChannel.OnEventRaised += OnPlayerKilled;
            

            //_initialHeight = crouchTransform.localScale.y;
            _pauseEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
            _pauseEventChannel.OnEventRaised += OnPause;
            

            _speed = sprintSpeed;
            _rootTransformHeight = transform.localScale.y;
            _readyToJump = true;
            _isGrounded = true;
            _exitingSlope = false;
            _speed *= speedMultiplier;

        }

       
        private void OnPause(PauseEvent pauseEvent)
        {
            if(!IsOwner || pauseEvent.PlayerId != OwnerClientId)
                return;
            
            _isPaused = pauseEvent.IsPaused;
            
        }

        private void Start()
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void OnPlayerKilled(PlayerDeathEvent playerDeathEvent)
        {
            /*
            if (playerDeathEvent.KillerNetworkObjectReference.TryGet(out var networkObject, NetworkManager.Singleton) &&
                networkObject.OwnerClientId == OwnerClientId)
            {
                dashesRemaining = 3;
            }
            */
        }

        private void Dash(InputAction.CallbackContext ctx)
        {
            if (dashesRemaining <= 0 || WallRunning || _isPaused) return;
            _rb.AddForce(dashForce * orientation.forward, ForceMode.Impulse);
            dashesRemaining--;
        }

        
        private void StateHandler()
        { 
            if (WallRunning)
            {
              
                _speed = wallRunSpeed * speedMultiplier;
            }
            else
            {
             
                _speed = sprintSpeed * speedMultiplier;
            }
           
        }

  
    

        private void Jump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !_readyToJump || !_isGrounded || WallRunning || _isPaused) return;
            _readyToJump = false;
            _exitingSlope = true;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
         
            StartCoroutine(ResetJump());
        }

        private IEnumerator ResetJump()
        {
            yield return new WaitForSeconds(.4f);
            _readyToJump = true;
            _exitingSlope = false;
        }

    
        private void Update()
        {
            _dashTimer += Time.deltaTime;
            
            if (_dashTimer > dashRegenTime && dashesRemaining < maxDashes)
            {
                dashesRemaining++;
                _dashTimer = 0;
            }
            
            _isGrounded= Physics.OverlapSphereNonAlloc(groundDetectionTransform.position, 0.5f,
                _groundColliders, playerGamePlayInfo.groundLayerMask) > 0;
      
            
        }

        private void FixedUpdate()
        {
           
            MovePlayer();
          
        }



        private void MovePlayer()
        {
        
            if (!_isPaused)
            {
                 
                 MoveInput = _playerInputActions.Player.Move.ReadValue<Vector2>();
                 Moving = MoveInput != Vector2.zero;
            }

            if (!Sliding)
            {
                MoveDirection = (orientation.forward * MoveInput.y) + (orientation.right * MoveInput.x);
                
            }

            ControlSpeed();
            
            HandleFalling();

            StateHandler();
            
           
            if (OnSlope() && !_exitingSlope)
            {
                _rb.AddForce(GetSlopeMovementDirection(MoveDirection) * _speed, ForceMode.Force);

                if (_rb.linearVelocity.y > 0)
                {
                   
                    _rb.AddForce(Vector3.down * 8f, ForceMode.Force);
                }
            }

            // on ground
            else if(_isGrounded && !WallRunning)
                _rb.AddForce(MoveDirection.normalized * _speed, ForceMode.Force);

            // in air
            else if(!_isGrounded)
                _rb.AddForce(MoveDirection.normalized * (_speed * airMultiplier), ForceMode.Force);

            // turn gravity off while on slope
            if (!WallRunning)
            {
                _rb.useGravity = !OnSlope();
            }

        }

        private void HandleFalling()
        {
            if(_rb.linearVelocity.y < 0 && !WallRunning && !_isGrounded)
            {
                _rb.linearVelocity += ((fallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up).normalized;
            
            }

            _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
        }

       


        private void ControlSpeed()
        {
      
            if (OnSlope() && !_exitingSlope)
            {
                if (_rb.linearVelocity.magnitude > _speed)
                    _rb.linearVelocity = _rb.linearVelocity.normalized * _speed;
            }

            else
            {
                Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

               
                if (flatVel.magnitude > _speed)
                {
                    Vector3 limitedVel = flatVel.normalized * _speed;
                    _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
                }
            }
        }

        internal bool OnSlope()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _rootTransformHeight * .5f + .3f))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
               
                return angle < maxSlopeAngle && angle != 0f;
            }

            return false;
        }

        internal Vector3 GetSlopeMovementDirection(Vector3 moveDirection) => Vector3.ProjectOnPlane(moveDirection, _slopeHit.normal).normalized;

        private void OnDisable()
        {
            _playerInputActions.Player.Disable();
        }
        
        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }
        
 
     
        public override void OnNetworkDespawn()
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            base.OnNetworkDespawn();
            
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _playerInputActions.Player.Jump.performed -= Jump;
            _playerInputActions.Player.Sprint.canceled -= Dash;
            _playerDeathEventChannel.OnEventRaised -= OnPlayerKilled;
            _pauseEventChannel.OnEventRaised -= OnPause;
        }
    }
}
