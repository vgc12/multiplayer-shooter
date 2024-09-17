using System;
using System.Collections;
using System.Collections.Generic;
using EditorScripts;
using Player;
using Unity.Netcode;
using UnityEngine;

public class Sliding : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform crouchTransform;
    private Rigidbody _rb;
    private PlayerMovement _playerMovement;

    [Header("Sliding")]
    public float slideForce;
    public float inverseSlideForce;
    [SerializeField] private float maxInverseSlideForce;
    private float _slideTimer;

    public float crouchHeight;
    [SerializeField]
    private Transform cameraTransform;
    private float _initialHeight;

    private Vector3 _slideDirection;
    
    [Header("Input")]
    private PlayerInputActions _playerInputActions;

    private bool _crouching;
    private readonly Collider[] _crouchColliders = new Collider[1];
    [ScriptableObjectDropdown]
    [SerializeField] private PlayerGamePlayInfoScriptableObject playerGamePlayInfo;

    private bool ObjectAbove => Physics.OverlapCapsuleNonAlloc(
        new Vector3(crouchTransform.position.x, crouchTransform.position.y + crouchHeight, crouchTransform.position.z),
        new Vector3(crouchTransform.position.x, crouchTransform.position.y + _initialHeight,
            crouchTransform.position.z),
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
        _playerInputActions.Player.Crouch.started += ctx => StartSlide();
        _playerInputActions.Player.Crouch.canceled += ctx => StopSlide();
        _initialHeight = crouchTransform.localScale.y;
    }

    private void StartSlide()
    {
        if (_playerMovement.isWallRunning)
        {
            return;
        }
        _playerMovement.Sliding = true;
        _slideDirection = _playerMovement.MoveDirection;
        _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        
        inverseSlideForce = 0;
        
        _crouching = true;
        _crouchTime = 0;
    }

    private float _crouchTime = 0f;
    private void Crouch(float height)
    {

        if (IsOwner)
        {
            
            if (_crouchTime < 1f &&(_crouching || (!ObjectAbove && !_crouching)))
            {
                _crouchTime += Time.deltaTime * 5f;
                crouchTransform.localScale =
                    new Vector3(1f, Mathf.Lerp(crouchTransform.localScale.y, height, _crouchTime), 1f);
            
            }
        }
    }
    
    private void Update()
    {
        
        Crouch(_crouching ? crouchHeight : _initialHeight);
    }

    private void FixedUpdate()
    {
        if (_playerMovement.Sliding)
        {
            inverseSlideForce = Mathf.Clamp(inverseSlideForce + (Time.deltaTime * (maxInverseSlideForce * .3f)), 0,
                maxInverseSlideForce);
          
            SlidingMovement();
        }
    }

    private void SlidingMovement()
    {
        
        
        if (!_playerMovement.OnSlope() || _rb.velocity.y > -.1f)
        {
            _rb.AddForce(_slideDirection * slideForce, ForceMode.Force);
            _rb.AddForce(-_slideDirection * inverseSlideForce, ForceMode.VelocityChange);
        }
        else
        {
            var dir = _playerMovement.GetSlopeMovementDirection(_slideDirection);
            _rb.AddForce(dir*slideForce, ForceMode.Force);
            _rb.AddForce(-dir * inverseSlideForce, ForceMode.VelocityChange);
            
        }
    }
    
    
    private void StopSlide()
    {
        _playerMovement.Sliding = false;
        
        _crouching = false;
        _crouchTime = 0;
    }    
}
