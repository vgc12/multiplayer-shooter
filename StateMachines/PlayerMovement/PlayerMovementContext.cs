using EventChannels;
using Player;
using StateMachines.General;
using StateMachines.PlayerLooking;
using StateMachines.PlayerMovement;
using UI;
using UnityEngine;
using Utilities;

namespace StateMachines
{
    public class PlayerMovementContext
    {
     
        public PlayerInputActions InputActions { get; }
        public Rigidbody Rb { get; }
        public GenericEventChannelScriptableObject<PauseEvent> PauseEventChannel { get; }
        
        public PlayerLookingContext PlayerLookingContext { get; set; }

        public MonoBehaviour MonoBehaviour { get; private set; }
        
        public bool IsPaused { get; set; } = false;

        public bool IsDead { get; set; } = false;


        [Header("Movement")] 
        private float _sprintSpeed;
   
        
        public float Speed { get; set; }

        public float SpeedMultiplier { get; }
        public Transform Orientation { get; }
        public Vector3 MoveDirection { get; set; }
        public Vector2 MoveInput { get; set; }

        public float WallRunSpeed;

        [Header("Slope Detection")]
        public float MaxSlopeAngle { get; private set; } 
        public bool OnSlope
        {
            get
            {
                if (Physics.Raycast(Orientation.position, Vector3.down, out SlopeHit,
                        RootTransformHeight * .5f + .3f))
                {
                    float angle = Vector3.Angle(Vector3.up, SlopeHit.normal);

                    return angle < MaxSlopeAngle && angle != 0f;
                }

                return false;
            }
        }

        public RaycastHit SlopeHit;
        public bool ExitingSlope { get; set; } 

        [Header("Jumping")]


        //[ScriptableObjectDropdown]
        
        public readonly PlayerGamePlayInfoScriptableObject PlayerGamePlayInfo;

        public float AirMultiplier { get; private set; } = .4f;


        public float FallMultiplier { get; private set; } = 3.5f;
        
        
        
        private bool _objectAbove;
        private readonly Collider[] _crouchColliders = new Collider[1];
        private LayerMask _crouchIgnoreMask;

 


   
        private float _initialHeight;
        
        [Header("Dashing")]
        public int DashesRemaining { get; set; }

        public float DashRegenTime { get; set; } = 2f;
        public float DashForce { get; }
        public GenericEventChannelScriptableObject<PlayerDeathEvent> PlayerDeathEventChannel { get; private set; }
        
        public float DashTimer { get; set; } = 0;
        public int MaxDashes { get; }

        [Header("Ground Detection")]
        private Transform _groundDetectionTransform;
        public bool Grounded =>  Physics.OverlapSphereNonAlloc(Orientation.position, 0.5f,
            _groundColliders, PlayerGamePlayInfo.groundLayerMask) > 0; 
        
        public bool ReadyToJump { get; set; } = true;
        private readonly Collider[] _groundColliders = new Collider[1];
        public float RootTransformHeight { get; } = 1f;

        [Header("Drag")] 
        public float GroundDrag { get; private set; } = 6f;

        public float AirDrag { get; private set; } = 2f;

         
         [Header("Jump")]
         public bool JumpPressed { get; set; } = false;

         public bool DashPressed { get; set; }
        public float JumpForce { get; private set; } = 13f;
        [Header("Sliding and Crouching")]
        public Vector3 SlideDirection { get; set; }
        public bool CrouchPressed { get; set; }
        public bool Crouching { get; set; }
        
        public bool ShouldNotBeCrouching { get; set; }
        public float InitialHeight { get; set; }

        public float CrouchTime { get; set; }
        public Transform HeadCheckTransform { get; }
        
        public bool ObjectAbove => Physics.OverlapSphereNonAlloc(HeadCheckTransform.position,
            0.2f, _crouchColliders, ~PlayerGamePlayInfo.crouchIgnoreMask) > 0;

        public Transform FirstPersonColliderTransform { get; set; }
        public float SpeedChangeThreshold { get; set; }
        public float PreviousSpeed { get; set; }
        public float SlideForce { get; set; }
        public float CrouchHeight { get; set; }
        
        public Vector3 SlamDirection { get; set; }
        
        public Vector3 SlamImpactPoint { get; set; }
        
        [Header("Wall Running")]
        public bool DownwardsRunning { get; set; }
        public bool UpwardsRunning { get; set; }
      

        public readonly float WallRunForce;
        public bool CanWallRun { get; set; } = true;
        public readonly float WallClimbSpeed;
        public readonly float WallJumpUpForce;
        public readonly float WallJumpSideForce;
    
    
        [Header("input")]
        private PlayerInputActions _playerInputActions;
        public readonly float WallCheckDistance;
       
        
        public RaycastHit LeftWallHit;
    
        public RaycastHit RightWallHit;
   
        public bool WallLeft { get; set; }
        public bool WallRight { get; set; }
        private Rigidbody _rb;
        private Vector2 _direction;
        private Collider _collider;
    
        public float MinSlamHeight { get; private set; }
    
        private Transform _playerCamera;
        private float _wallRunFOV;
        private float _wallRunRotation;
        private Animator _animator;

        public float velocityX;
        public float velocityY;
        public float maxVelocity;
        public float acceleration;
        public float deceleration;

    


        public readonly float WallRunSpeedChangeThreshold;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;
        private float _previousSpeed;
        private bool _exitingWall;
        public readonly float ExitWallTime;
        public float ExitWallTimer { get; set; }
      
        public bool AboveGround => !Physics.Raycast(Orientation.position, Vector3.down, MinJumpHeight, PlayerGamePlayInfo.groundLayerMask);
        public float MinJumpHeight { get; set; }
        
        public float HeightFromGround => Physics.Raycast(Orientation.position, Vector3.down, out var hit, 100f, PlayerGamePlayInfo.groundLayerMask) ? hit.distance : 0;
        
        public bool CanSlide { get; set; } = true;
        
        public bool ShouldBeSlamming { get; set; }
        public float SlamForce { get; set; } = 0;
        
        public GenericEventChannelScriptableObject<GroundSlamEvent> GroundSlamEventChannel { get; private set; }

        public Vector3 GetSlopeMovementDirection(Vector3 moveDirection) => Vector3.ProjectOnPlane(moveDirection, SlopeHit.normal).normalized;

        public bool IsWallRunning;

        public PlayerMovementStateMachine StateMachine { get; set; }
        public GroundSlamManager GroundSlamManager { get; set; }
        public GenericEventChannelScriptableObject<PlayerSpawnEvent> PlayerSpawnEventChannel { get; set; }


        public Transform CameraHolderTransform { get; private set; }
        
        public bool IsSlamming { get; set; }
        public bool CrouchRePressed { get; set; }
        public bool IsSliding { get; set; }



        

        public PlayerMovementContext( PlayerMovementStateMachine stateMachine)
         {
             
        
             InputActions = new PlayerInputActions();
             Rb = stateMachine.Rigidbody;
             StateMachine = stateMachine;
            GroundSlamManager = stateMachine.GroundSlamManager;

            CameraHolderTransform = stateMachine.cameraHolderTransform;
            PlayerSpawnEventChannel = stateMachine.EventChannelAccessor.playerSpawnEventChannel;
             
            PlayerDeathEventChannel = stateMachine.EventChannelAccessor.playerDeathEventChannel;
            PauseEventChannel = stateMachine.EventChannelAccessor.gamePausedEventChannel;
            GroundSlamEventChannel = stateMachine.EventChannelAccessor.groundSlamEventChannel;
            PlayerGamePlayInfo = stateMachine.infoScriptableObject;
            SpeedMultiplier = stateMachine.speedMultiplier;
            Orientation = stateMachine.orientation;
            Speed = stateMachine.sprintSpeed;
            Speed *= SpeedMultiplier;
     
            MaxDashes = stateMachine.maxDashes;
            DashRegenTime = stateMachine.dashRegenTime;
            DashesRemaining = MaxDashes;
            DashForce = stateMachine.dashForce;
            SlideForce = stateMachine.slideForce;
            
            HeadCheckTransform = stateMachine.headCheckTransform;
            FirstPersonColliderTransform = stateMachine.firstPersonColliderTransform;
            InitialHeight = stateMachine.initialHeight;
            CrouchHeight = stateMachine.crouchHeight;
            SpeedChangeThreshold = stateMachine.slideChangeThreshold;
            
            WallJumpSideForce = stateMachine.wallJumpSideForce;
            WallClimbSpeed = stateMachine.wallClimbSpeed;
            WallJumpUpForce = stateMachine.wallJumpUpForce;
            WallCheckDistance = stateMachine.wallCheckDistance;
            WallRunSpeedChangeThreshold = stateMachine.wallRunSpeedChangeThreshold;
            WallRunForce = stateMachine.wallRunForce;
            MinJumpHeight = stateMachine.minJumpHeight;
            MinSlamHeight = stateMachine.minSlamHeight;
            ExitWallTime = 0.3f;
            ExitWallTimer = ExitWallTime;
            MonoBehaviour = stateMachine;
         }
    }
}