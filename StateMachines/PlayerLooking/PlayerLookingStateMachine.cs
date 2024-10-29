using System.Collections.Generic;
using EventChannels;
using Player;
using StateMachines.General;
using StateMachines.PlayerMovement;
using UI;
using UnityEngine;
using Utilities;

namespace StateMachines.PlayerLooking
{
    public class PlayerLookingStateMachine : NetworkBehaviorStateMachine<PlayerLookingStateMachine.PlayerLookingState>
    {
        private PlayerLookingContext Context { get; set; }
        public float normalFov;
        public Camera gunCamera;
        public Camera playerCamera;
        public Transform thirdPersonPositionTransform;
        public Transform thirdPersonRotationObject;
        public PlayerGamePlayInfoScriptableObject playerGamePlayInfoScriptableObject;
        public Transform cameraHolderTransform;
        public Transform slideHeadPosition;
        public Transform orientation;
        public float sensitivity;

        public PlayerMovementStateMachine playerMovementStateMachine;
        public Transform initialHeadPosition;
        public float wallRunRotation;
        public AnimationCurve screenShakeCurve;

        public EventChannelAccessor EventChannelAccessor => EventChannelAccessor.Instance;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
            }
        }
        
        
        public void Awake()
        {
            Context = new PlayerLookingContext(this);

            Context.PlayerDeathEventChannel.OnEventRaised += OnPlayerKilled;
            Context.PlayerSpawnEventChannel.OnEventRaised += OnPlayerSpawned;
            Context.RagdollSpawnEventChannel.OnEventRaised += OnRagdollSpawned;
            
            
            Context.InputActions.Player.Look.performed += ctx => Context.MouseDelta = ctx.ReadValue<Vector2>();
          
            Context.PauseEventChannel.OnEventRaised += OnPause;
            States = new Dictionary<PlayerLookingState, BaseState<PlayerLookingState>>()
            {
                {
                    PlayerLookingState.Default, new DefaultLookingState(Context,this)
                },
                {
                    PlayerLookingState.Crouch, new CrouchLookingState(Context,this)
                },
                {
                    PlayerLookingState.Standing, new StandingLookingState(Context,this)
                },
                {
                    PlayerLookingState.Walking, new WalkingLookingState(Context,this)
                },
                {
                    PlayerLookingState.WallRunning, new WallRunningLookingState(Context,this)
                },
                {
                    PlayerLookingState.Slamming, new SlammingLookingState(Context,this)
                },
                {
                    PlayerLookingState.Dead, new DeadLookingState(Context,this)
                },
            };
            
            CurrentState = States[PlayerLookingState.Default];
            
            
            
        }

        private void OnRagdollSpawned(RagdollSpawnEvent ragdollSpawnEvent)
        {
            if (ragdollSpawnEvent.KilledClientId == OwnerClientId && IsOwner)
            {
                Context.LatestRagdollObject = ragdollSpawnEvent.Ragdoll;
            }
        }


        private void OnPlayerSpawned(PlayerSpawnEvent playerSpawnEvent)
        {
            if(playerSpawnEvent.RespawningClientId == OwnerClientId && IsOwner)
            {
                Context.IsDead = false;
            }
        }

        private void OnPlayerKilled(PlayerDeathEvent playerDeathEvent)
        {
            if (playerDeathEvent.KilledClientId == OwnerClientId && IsOwner)
            {
                Context.IsDead = true;
            }
        }

        private void OnPause(PauseEvent pauseEvent)
        {
            
        }

        public enum PlayerLookingState
        {
           
            Dead,
            Default,
            Crouch,
            Standing,
            Walking,
            WallRunning,
            Slamming
        }

        public void ShakeCamera()
        {
           StartCoroutine(Context.PlayerCamera.transform.Shake(.3f, screenShakeCurve));
        }
    }
}