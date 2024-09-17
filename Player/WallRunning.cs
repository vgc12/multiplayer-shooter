using System;
using System.Collections;
using System.Collections.Generic;
using EditorScripts;
using Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(NetworkRigidbody))]
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
    
    
    [Header("input")]
    private PlayerInputActions _playerInputActions;

    [Header("Detection")] [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _wallLeft;
    private bool _wallRight;
    [SerializeField] private Transform orientation;
    private PlayerMovement _playerMovement;
    private PlayerLooking _playerLooking;
    private Rigidbody _rb;
    private Vector2 _direction;
    private Collider _collider;
    
    
    [Header("Effects")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform model;
    [SerializeField] private float wallRunFOV;
    [SerializeField] private float wallRunRotation;
    [SerializeField] private Animator animator;
    

    [Header("Exiting")] private bool _exitingWall;
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
        _playerInputActions.Enable();
        _playerInputActions.Player.Sprint.performed += ctx => _upwardsRunning = true;
        _playerInputActions.Player.Sprint.canceled += ctx => _upwardsRunning = false;
        _playerInputActions.Player.Crouch.performed += ctx => _downwardsRunning = true;
        _playerInputActions.Player.Crouch.canceled += ctx => _downwardsRunning = false;
        _playerInputActions.Player.Jump.performed += ctx => WallJump();
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLooking = GetComponentInChildren<PlayerLooking>();
        _canWallRun = true;
        

    }

    // Update is called once per frame
    private void Update()
    {
        CheckForWall();
        
        _direction = _playerInputActions.Player.Move.ReadValue<Vector2>();

 
        if (_direction.y > 0 && (_wallRight || _wallLeft) && AboveGround && !_exitingWall && _canWallRun)
        {
           
            if (!_playerMovement.isWallRunning )
            {
                StartWallRun();
            }
            
            
            
        }
        else if (_exitingWall)

        {
            if (_playerMovement.isWallRunning)
            {
                StopWallRun();
            }

           
        }
        else
        {
           if(_playerMovement.isWallRunning)
                StopWallRun();
        }
    }

    private void FixedUpdate()
    {
        if (_playerMovement.isWallRunning)
        {
            WallRunningMovement();
        }
  
        
    }

    private void StartWallRun()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

       
        animator.CrossFade(_wallRight?"LeanLeft" : "LeanRight", 0.1f);
        StartCoroutine(_playerLooking.LerpFOV(true, wallRunFOV));
        StartCoroutine(_playerLooking.LerpRotation(playerCamera.transform, 0,
            _wallRight ? wallRunRotation : -wallRunRotation));
        
        _playerMovement.isWallRunning = true;
     
    }
    
    private void StopWallRun()
    {
        animator.CrossFade("Idle", 0.1f);
        StartCoroutine(_playerLooking.LerpFOV(false, wallRunFOV));
        StartCoroutine(_playerLooking.LerpRotation(playerCamera.transform, playerCamera.transform.rotation.z, 0));

        
        StartCoroutine(CountDownTimer());
        
        _playerMovement.isWallRunning = false;
    }

    private IEnumerator CountDownTimer()
    {
        _canWallRun = false;
        yield return new WaitForSeconds(exitWallTime);
        _canWallRun = true;
        _exitingWall = false;
    }

    private void WallRunningMovement()
    {
        _rb.useGravity = useGravity;


        var wallNormal = _wallRight ? _rightWallHit.normal :_leftWallHit.normal;
     
        
        var wallForward = Vector3.Cross(wallNormal, transform.up);


        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

     
        if (Vector3.Angle(wallForward, orientation.forward) > 90)
        {
            _exitingWall = true;
            return;
        }
     
        _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        
        if (_upwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, wallClimbSpeed, _rb.velocity.z);
            
        if (_downwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, -wallClimbSpeed, _rb.velocity.z);
   
        
        if (!(_wallLeft && _direction.x > 0) && !(_wallRight && _direction.x < 0))
            _rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
        {
            _rb.AddForce(transform.up* gravityCounterforce, ForceMode.Force);
        }
        
    }


    private void CheckForWall()
    {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, playerGamePlayInfo.wallLayerMask);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, playerGamePlayInfo.wallLayerMask);
    }



    private void WallJump()
    {
        if(!_playerMovement.isWallRunning)
            return;
        _exitingWall = true;
        _exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(forceToApply, ForceMode.Impulse);
        
    }
    
    private bool AboveGround => !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, playerGamePlayInfo.groundLayerMask);
    
    
    
}
