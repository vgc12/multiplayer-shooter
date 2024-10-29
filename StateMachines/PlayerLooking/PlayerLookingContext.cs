using EventChannels;
using Player;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UI;
using UnityEngine;
using Utilities;

namespace StateMachines.PlayerLooking
{
    public class PlayerLookingContext
    {
        public PlayerInputActions InputActions { get; private set; }
        
        public AnimationCurve ScreenShakeCurve { get; private set; }
        
        public Camera PlayerCamera { get; private set; }
        public Camera GunCamera { get; private set; }

        
        public Transform Orientation  { get; private set; }


        public float SensX  { get; private set; } = 100f;
        public float SensY  { get; private set; } = 100f;

        public float XRotation;
        public float YRotation;
    
        public float NormalFov = 90f;


        
      
        public Transform HeadModelTransform;

        public GenericEventChannelScriptableObject<PlayerDeathEvent> PlayerDeathEventChannel  { get; private set; }
        public GenericEventChannelScriptableObject<PlayerSpawnEvent> PlayerSpawnEventChannel  { get; private set; }
        
        public GenericEventChannelScriptableObject<RagdollSpawnEvent> RagdollSpawnEventChannel { get; private set; }
     
        public Transform KillerTransform;

      

        public readonly Transform ThirdPersonRotationObject;
        public Transform ThirdPersonPositionTransform;
        public readonly Transform CameraHolderTransform;
        public Vector3 PlayerCameraPosition;
        public Transform RotationTarget;
        public readonly float WallRunRotation;

        public PlayerGamePlayInfoScriptableObject InfoScriptableObject;
        public readonly GenericEventChannelScriptableObject<PauseEvent> PauseEventChannel;
        
        private bool _isDead;
        private GameObject _falsePlayerPrefab;

        public Vector2 MouseDelta { get; set; }
        public Transform SlideHeadPosition { get; set; }

        public readonly Transform InitialHeadPosition;
        
        public PlayerMovementContext PlayerMovementContext { get; private set; }

        public MonoBehaviour MonoBehaviour { get; }
        public bool IsDead { get; set; }
        public float CrouchTime { get; set; }
        public GameObject LatestRagdollObject { get; set; }

        public PlayerLookingContext(PlayerLookingStateMachine stateMachine )
        {
            MonoBehaviour = stateMachine;
            WallRunRotation = stateMachine.wallRunRotation;
            InputActions = new PlayerInputActions();
            InputActions.Enable();
            
            PauseEventChannel = stateMachine.EventChannelAccessor.gamePausedEventChannel;
            PlayerDeathEventChannel = stateMachine.EventChannelAccessor.playerDeathEventChannel;
            PlayerSpawnEventChannel = stateMachine.EventChannelAccessor.playerSpawnEventChannel;
            RagdollSpawnEventChannel = stateMachine.EventChannelAccessor.ragdollSpawnEventChannel;
            ScreenShakeCurve = stateMachine.screenShakeCurve;
            InputActions.Player.Look.Enable();
            NormalFov = stateMachine.normalFov;
            PlayerCamera = stateMachine.playerCamera;
            GunCamera = stateMachine.gunCamera;
            ThirdPersonPositionTransform = stateMachine.thirdPersonPositionTransform;
            ThirdPersonRotationObject = stateMachine.thirdPersonRotationObject;
            CameraHolderTransform = stateMachine.cameraHolderTransform;
            SensX = stateMachine.sensitivity;
            SensY = stateMachine.sensitivity;
            PlayerMovementContext = stateMachine.playerMovementStateMachine.Context;
            InfoScriptableObject = stateMachine.playerGamePlayInfoScriptableObject;
            PauseEventChannel = stateMachine.EventChannelAccessor.gamePausedEventChannel;
            SlideHeadPosition = stateMachine.slideHeadPosition;
            InitialHeadPosition = stateMachine.initialHeadPosition;
            Orientation = stateMachine.orientation;
        }
        
    }
}