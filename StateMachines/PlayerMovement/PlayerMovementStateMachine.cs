using System;
using System.Collections.Generic;
using EventChannels;
using Player;
using StateMachines.General;
using StateMachines.PlayerLooking;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace StateMachines.PlayerMovement
{
    public class PlayerMovementStateMachine : NetworkBehaviorStateMachine<PlayerMovementStateMachine.MovementState> 
    {
        [Header("General")]
        public Transform orientation;

    
        public PlayerMovementContext Context { get; private set; }
        internal EventChannelAccessor EventChannelAccessor => EventChannelAccessor.Instance;
        internal GroundSlamManager GroundSlamManager => GroundSlamManager.Instance;
        
        public PlayerGamePlayInfoScriptableObject infoScriptableObject;
        public Rigidbody Rigidbody { get; private set; }
        public Transform cameraHolderTransform;

        public float initialHeight;
        public float slideChangeThreshold;
        public float slideForce;

        [Header("Movement")]
        public float sprintSpeed;
        public float speedMultiplier;
       
        [Header("Dashing")]
        public int maxDashes;
        public float dashRegenTime;
        public float dashForce;
        public Transform headCheckTransform;
        [Header("Sliding")]
        public Transform firstPersonColliderTransform;
        public float crouchHeight;

        [Header("Wall Running")]
        public float wallRunSpeedChangeThreshold;

        public float wallRunForce;
        public float wallJumpUpForce;
        public float wallJumpSideForce;
        public float wallCheckDistance;
        public float wallClimbSpeed;
        public float minJumpHeight = 1.5f;
        public float minSlamHeight;


        public void Awake()
        {
        
            Rigidbody = GetComponent<Rigidbody>();
            
            Context = new PlayerMovementContext(this);
            Context.InputActions.Enable();
            Context.InputActions.Player.Enable();
            Context.InputActions.Player.Jump.performed += Jump;
            Context.InputActions.Player.Jump.canceled += Jump;
            Context.InputActions.Player.Sprint.performed += Dash;
            Context.InputActions.Player.Sprint.canceled += Dash;
            Context.InputActions.Player.Crouch.performed += _ =>
            {
                Context.DownwardsRunning = true;
                Context.CrouchRePressed = true;
            };
            Context.InputActions.Player.Crouch.canceled += _ =>
            {
                Context.DownwardsRunning = false;
                Context.CrouchRePressed = false;
            };
            Context.InputActions.Player.Slam.performed += SlamPressed;
            Context.PauseEventChannel.OnEventRaised += OnPauseEventRaised;
            Context.PlayerDeathEventChannel.OnEventRaised += OnPlayerKilled;
            Context.PlayerSpawnEventChannel.OnEventRaised += OnPlayerSpawned;


            States = new Dictionary<MovementState, BaseState<MovementState>>
            {
                {
                    MovementState.Default, new PlayerMovementBaseState(Context, this)
                },
                {
                    MovementState.Paused, new PauseMovementState( this)
                },
                
                {
                    MovementState.Grounded, new GroundedMovementState(Context, this)
                },
                {
                    MovementState.Air, new AirMovementState(Context, this)
                },
                {
                    MovementState.Jumping, new JumpMovementState(Context, this)
                },
                {
                    MovementState.Idle, new IdleMovementState(Context, this)
                },
                {
                    MovementState.Slope, new SlopeMovementState(Context, this)
                },
                {
                    MovementState.Dash, new DashMovementState(Context, this)
                },
                {
                    MovementState.Sliding, new SlidingMovementState(Context, this)
                },
                {
                    MovementState.Crouching, new CrouchingMovementState(Context, this)
                },
                {
                    MovementState.WallRunning, new WallRunningMovementState(Context, this)
                },
                {
                    MovementState.Dead, new IdleMovementState(Context, this)
                },
                {
                    MovementState.GroundSlam, new SlamMovementState(Context, this)
                }
                
                
                
            };
            CurrentState = States[MovementState.Default];
       
   
        }



        private void OnPlayerSpawned(PlayerSpawnEvent onPlayerSpawnEvent)
        {
            if (onPlayerSpawnEvent.RespawningClientId== OwnerClientId && IsOwner)
            {
                Context.IsDead = false;
            }
        }


  

        private void OnPlayerKilled(PlayerDeathEvent playerDeathEvent)
        {
            if (playerDeathEvent.KillerClientId == OwnerClientId)
            {
                Context.DashesRemaining = Context.MaxDashes;
            }
            else if (playerDeathEvent.KilledClientId == OwnerClientId && IsOwner)
            {
                Context.IsDead = true;
            }
            
            
        }
        
        private void SlamPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && !Context.Grounded && !Context.IsWallRunning &&  Context.HeightFromGround >= Context.MinSlamHeight)
            {
              
                Context.ShouldBeSlamming = true;
                
            }
        }


       
        
        
        private void CrouchPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.started && Context.HeightFromGround < Context.MinJumpHeight + .2f)
            {
                Context.CrouchPressed = true;
                Context.CanSlide = true;
                Context.ShouldNotBeCrouching = false;
          
            }
            else if (ctx.canceled)
            {
                Context.CrouchPressed = false;
         
                Context.ShouldNotBeCrouching = true;
            }
            Context.DownwardsRunning = ctx.performed;
        }
      


        private void Dash(InputAction.CallbackContext ctx)
        {
            
            Context.UpwardsRunning = ctx.performed;
           
            
        }

        private void Jump(InputAction.CallbackContext ctx)
        {
            
      
            if(ctx.performed){
                Context.JumpPressed = true;
            }
            else if (ctx.canceled)
            {
                Context.JumpPressed = false;
            }
       
        }


        private void OnPauseEventRaised(PauseEvent pauseEvent)
        {
            if(!IsOwner || pauseEvent.PlayerId != OwnerClientId)
                return;
            
            Context.IsPaused = pauseEvent.IsPaused;
         
        }

        public enum MovementState
        {
            Default,
            Paused,
            Dash,
            Jumping,
            Crouching,
            Sliding,
            Dead,
            Grounded,
            Slope,
            Air,
            WallRunning,
            Idle,
            GroundSlam
        }
    }
}