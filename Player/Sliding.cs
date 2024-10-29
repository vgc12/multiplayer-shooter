//using EditorScripts;

using System;
using EventChannels;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Player
{
    public class Sliding : NetworkBehaviour
    {
        [Header("References")]
        public Transform orientation;
        public Transform headCheckTransform;
        [SerializeField] private Transform firstPersonColliderTransform;
        private Rigidbody _rb;
        private PlayerMovement _playerMovement;

        [Header("Sliding")]
        public float slideForce;
        public float inverseSlideForce;
        [SerializeField] private float maxInverseSlideForce;
        private float _slideTimer;

        [SerializeField]
        private Transform cameraTransform;
        [SerializeField] private float crouchHeight;
        [SerializeField] private  float initialHeight = 1f;

        private Vector3 _slideDirection;
        private AnimationHandler _animationHandler;
    
        [Header("Input")]
        private PlayerInputActions _playerInputActions;

        
        private readonly Collider[] _crouchColliders = new Collider[1];
        //[ScriptableObjectDropdown]
        [SerializeField] private PlayerGamePlayInfoScriptableObject playerGamePlayInfo;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;
        private bool _isPaused;
        
        
    
        private bool ObjectAbove => Physics.OverlapSphereNonAlloc(headCheckTransform.position,
            0.5f, _crouchColliders, ~playerGamePlayInfo.crouchIgnoreMask) > 0;
    
        public override void OnNetworkSpawn()
        {
            if (IsOwner) return; 
            enabled = false;
      
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.Player.Crouch.started += StartSlide;
            _playerInputActions.Player.Crouch.canceled += StopSlide;
            _pauseEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
            _pauseEventChannel.OnEventRaised += OnPause;
            _crouching = false;
        }
    
        private void OnPause(PauseEvent pauseEvent)
        {
            if(!IsOwner || pauseEvent.PlayerId != OwnerClientId)
                return;
           
            StopSlide();
            _isPaused = pauseEvent.IsPaused;
                
            
        }
        private void StartSlide(InputAction.CallbackContext ctx )
        {
            if (_playerMovement.WallRunning || _isPaused)
            {
                return;
            }

            _playerMovement.Sliding = true;
            var forwardPressed = _playerMovement.MoveDirection.y > 0;
            _slideDirection = forwardPressed ? _playerMovement.MoveDirection : orientation.forward;

            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            inverseSlideForce = 0;
            _crouching = true;
            _crouchTime = 0;

        }


        private float _crouchTime = 0f;
        private bool _crouching;
        private void Crouch(float height)
        {

            if (IsOwner)
            {
            
                if (_crouchTime < 1f &&(_crouching || (!ObjectAbove && !_crouching)))
                {
                    _crouchTime += Time.deltaTime * 5f;
                    firstPersonColliderTransform.localScale =
                        new Vector3(1f, Mathf.Lerp(firstPersonColliderTransform.localScale.y, height, _crouchTime), 1f);
                    
                }
            }
        }
    
        private void Update()
        {
        
            Crouch(_crouching ? crouchHeight : initialHeight);
            
            _playerMovement.Crouching = firstPersonColliderTransform.localScale.y < initialHeight - .1f;
        }

        private float _previousSpeed;
        [SerializeField] private float speedChangeThreshold = 20f;


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(headCheckTransform.position,
                0.5f);
        }

        private void FixedUpdate()
        {
            if (_playerMovement.Sliding)
            {
                inverseSlideForce = Mathf.Clamp(inverseSlideForce + (Time.deltaTime * (maxInverseSlideForce * .3f)), 0,
                    maxInverseSlideForce);
               
          
                SlidingMovement();
                
                if(_previousSpeed - _rb.linearVelocity.magnitude > speedChangeThreshold)
                    StopSlide();
                
                _previousSpeed = _rb.linearVelocity.magnitude;
            }
            
        }

        private void SlidingMovement()
        {
        
        
            if (!_playerMovement.OnSlope() || _rb.linearVelocity.y > -.1f)
            {
                _rb.AddForce(_slideDirection * slideForce, ForceMode.Force);
                _rb.AddForce(-_slideDirection * inverseSlideForce, ForceMode.VelocityChange);
                
            }
            else
            {
                var dir = _playerMovement.GetSlopeMovementDirection(_slideDirection);
                _rb.AddForce(dir * slideForce, ForceMode.Force);
                _rb.AddForce(-dir * inverseSlideForce, ForceMode.VelocityChange);
                
            }
        }
    
    
        private void StopSlide(InputAction.CallbackContext ctx = default)
        {
            
            _crouching = false;
            _crouchTime = 0;
            _previousSpeed = 0;
            _playerMovement.Sliding = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _playerInputActions.Player.Crouch.started -= StartSlide;
            _playerInputActions.Player.Crouch.canceled -= StopSlide;
            _pauseEventChannel.OnEventRaised -= OnPause;
        }
    }
}
