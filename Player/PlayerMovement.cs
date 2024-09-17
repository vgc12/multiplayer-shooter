

using System;
using System.Collections;
using EditorScripts;
using EventChannels;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
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

        [Header("Movement")] 
        [SerializeField] private float sprintSpeed;

        [SerializeField] private float walkSpeed;
        private float _speed;
        [SerializeField]
        private float speedMultiplier = 10f;
        [SerializeField]
        private Transform orientation;
        public Vector3 MoveDirection { get; private set; }
        public float wallRunSpeed;
        public bool isWallRunning;
        private MovementState _movementState;
        private bool _sprinting;
        
        [Header("Slope Detection")]
        [SerializeField]
        private float maxSlopeAngle = 35f;
        private RaycastHit _slopeHit;
        private bool _exitingSlope = false;

        [Header("Jumping")]
        [SerializeField]
        [Range(0.0f, 2f)]
        private float heightMultiplier = 1.5f;
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

        [SerializeField]private int _dashesRemaining;

        public bool isCrouched = true;
        [SerializeField] private Transform crouchTransform;
        [SerializeField] private float crouchHeight = 0.5f;
        private float _initialHeight;
        
        [Header("Dashing")]
        [SerializeField] private int maxDashes =3;
        [SerializeField] private float dashRegenTime = 2f;
        [SerializeField] private GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
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

       
      
        public bool Sliding { get; set; }

        public override void OnNetworkSpawn()
        { 
            crouchTransform.GetComponent<Renderer>().material.color = Color.red;
            if (IsOwner) return;
            
            enabled = false;
          
        }

        


        private void Awake()
        {
            
            playerHitEventChannel.OnEventRaised += OnPlayerHit;
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _playerInputActions = new PlayerInputActions();
      
            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Jump.performed += Jump;
            _playerInputActions.Player.Sprint.canceled +=
                _ => Dash();
            _initialHeight = crouchTransform.localScale.y;


            _speed = walkSpeed;
            _rootTransformHeight = transform.localScale.y;
            _readyToJump = true;
            _isGrounded = true;
            _exitingSlope = false;
            _speed *= speedMultiplier;

        }

        private void Start()
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void OnPlayerHit(PlayerHitEvent arg0)
        {
            if (arg0.KillerPlayerClientID == OwnerClientId)
            {
                _dashesRemaining = 3;
            }
        }

        private void Dash()
        {
            if (_dashesRemaining <= 0 || isWallRunning) return;
            _rb.AddForce(100 * orientation.forward, ForceMode.Impulse);
            _dashesRemaining--;
        }


        private void StateHandler()
        {
            if (isWallRunning)
            {
                _movementState = MovementState.WallRunning;
                _speed = wallRunSpeed * speedMultiplier;
            }
            
            else if (_sprinting)
            {
                _movementState = MovementState.Sprinting;
                _speed = sprintSpeed * speedMultiplier;
            }
            
            else if (isCrouched)
            {
                _movementState = MovementState.Crouching;
                _speed = walkSpeed * speedMultiplier;
            }
            
            else
            {
                _movementState = MovementState.Walking;
                _speed = walkSpeed * speedMultiplier;
            }
        }

  
    

        private void Jump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _readyToJump && _isGrounded && !isWallRunning)
            {
                _readyToJump = false;
                _exitingSlope = true;
                _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
         
                StartCoroutine(ResetJump());
            }
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
            
            if (_dashTimer > dashRegenTime && _dashesRemaining < maxDashes)
            {
                _dashesRemaining++;
                _dashTimer = 0;
            }
            
            _isGrounded= Physics.OverlapSphereNonAlloc(groundDetectionTransform.position, 0.5f,
                _groundColliders, playerGamePlayInfo.groundLayerMask) > 0;
      
            
        }

        private void FixedUpdate() => MovePlayer();
        

        private void MovePlayer()
        {
          
            var direction = _playerInputActions.Player.Move.ReadValue<Vector2>();
            if(!Sliding)
                MoveDirection = (orientation.forward * direction.y) + (orientation.right * direction.x);
            
            
            ControlSpeed();
            
            HandleFalling();

            StateHandler();
            
           
            if (OnSlope() && !_exitingSlope)
            {
                _rb.AddForce(GetSlopeMovementDirection(MoveDirection) * _speed, ForceMode.Force);

                if (_rb.velocity.y > 0)
                {
                   
                    _rb.AddForce(Vector3.down * 8f, ForceMode.Force);
                }
            }

            // on ground
            else if(_isGrounded && !isWallRunning)
                _rb.AddForce(MoveDirection.normalized * _speed, ForceMode.Force);

            // in air
            else if(!_isGrounded)
                _rb.AddForce(MoveDirection.normalized * (_speed * airMultiplier), ForceMode.Force);

            // turn gravity off while on slope
            if (!isWallRunning)
            {
                _rb.useGravity = !OnSlope();
            }

        }

        private void HandleFalling()
        {
            if(_rb.velocity.y < 0 && !isWallRunning && !_isGrounded)
            {
                _rb.velocity += ((fallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up).normalized;
            
            }

            _rb.drag = _isGrounded ? groundDrag : airDrag;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var playerPosition = crouchTransform.position;
            Gizmos.DrawLine(new Vector3(playerPosition.x, playerPosition.y + _initialHeight+.13f, playerPosition.z),
                new Vector3(playerPosition.x, playerPosition.y + crouchHeight, playerPosition.z));
            
        }


        private void ControlSpeed()
        {
      
            if (OnSlope() && !_exitingSlope)
            {
                if (_rb.velocity.magnitude > _speed)
                    _rb.velocity = _rb.velocity.normalized * _speed;
            }

            else
            {
                Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

               
                if (flatVel.magnitude > _speed)
                {
                    Vector3 limitedVel = flatVel.normalized * _speed;
                    _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
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
        
        private enum MovementState
        {
            Walking,
            Sprinting,
            Crouching,
            Jumping,
            WallRunning
        }

        public override void OnNetworkDespawn()
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            base.OnNetworkDespawn();
            
        }
    }
    }
