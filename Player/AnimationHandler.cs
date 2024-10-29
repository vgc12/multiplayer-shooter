using System;
using StateMachines.PlayerMovement;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player
{
    public class AnimationHandler : NetworkBehaviour
    {




        [SerializeField] private Animator animator;
        
        [Header("Animation Hashes")]
        private static readonly int RunBlendTree = Animator.StringToHash("RunBlendTree");
        private static readonly int Slide = Animator.StringToHash("Slide");
        private static readonly int CrouchBlendTree = Animator.StringToHash("CrouchBlendTree");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int WallRunLeft = Animator.StringToHash("WallRunLeft");
        private static readonly int WallRunRight = Animator.StringToHash("WallRunRight");
        private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
        private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
        
        
        private PlayerMovementStateMachine _playerMovement;
        
        private AnimationState _currentState;
        
        [Header("Blend Trees")]
        [SerializeField] private float acceleration = 0.1f;
        [SerializeField] private float deceleration = 0.1f;
        [SerializeField] private float maxVelocity = 1f;
        [SerializeField] private float roundThreshold = 0.05f;

        [Header("Wall Running Repositioning")] 
        [SerializeField] private Transform modelRoot;
        [SerializeField] private float wallRunPositionOffset;

        private float _velocityX;
        private float _velocityY;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                enabled = false;
            }
        }

        private void Awake()
        {
        
            _playerMovement = GetComponent<PlayerMovementStateMachine>();
           
        }


        
        private void Update()
        {
       
            bool forward = _playerMovement.Context.MoveInput.y > 0;
            bool backward = _playerMovement.Context.MoveInput.y < 0;
            bool left = _playerMovement.Context.MoveInput.x < 0;
            bool right = _playerMovement.Context.MoveInput.x > 0;


            ChangeVelocity(forward, backward, left, right);
            LockOrResetVelocity(left, right, forward, backward);

            Debug.Log(_velocityX);
            Debug.Log(_velocityY);
            
            
            HandleAnimationState();
        }

        private void HandleAnimationState()
        {
            animator.SetFloat(HorizontalMovement,Mathf.Clamp(_velocityX,-1,1));
            animator.SetFloat(VerticalMovement,Mathf.Clamp(_velocityY,-1,1) );
            if (!_playerMovement.Context.IsWallRunning)
            {
               // wallRunRig.weight = 0;
               modelRoot.localPosition = new Vector3(0, modelRoot.localPosition.y, modelRoot.localPosition.z);
            }
            

            if (_playerMovement.Context.IsWallRunning)
            {
                var wallLeft = _playerMovement.Context.WallLeft;
                PlayAnimation(wallLeft ? AnimationState.WallRunningLeft : AnimationState.WallRunningRight);
                
                modelRoot.localPosition = new Vector3((wallLeft ? wallRunPositionOffset : -wallRunPositionOffset), modelRoot.localPosition.y, modelRoot.localPosition.z);
                
          
            
            }
            else if(_playerMovement.Context.IsSliding)
            {
                PlayAnimation(AnimationState.Sliding);
            }
            else if (_playerMovement.Context.Crouching)
            {
                PlayAnimation(AnimationState.Crouch);
            }
            else 
            {
                PlayAnimation(AnimationState.Running);
            }
/*
            void SetFootPosition(Collider wallCollider)
            {
                rightLegTarget.position = wallCollider.ClosestPoint(rightLegTarget.position);
                leftLegTarget.position = wallCollider.ClosestPoint(leftLegTarget.position);
            }
            */
        }

        private void ChangeVelocity(bool forward, bool backward, bool left, bool right)
        {
            if (forward && _velocityY < maxVelocity)
            {
                _velocityY += acceleration * Time.deltaTime;
            }
            
            if (backward && _velocityY > -maxVelocity)
            {
                _velocityY -= acceleration * Time.deltaTime;
            }
            
            if (left && _velocityX > -maxVelocity)
            {
                _velocityX -= acceleration * Time.deltaTime;
            }
            
            if (right && _velocityX < maxVelocity)
            {
                _velocityX += acceleration * Time.deltaTime;
            }
            
            if(!forward && _velocityY > 0.0f)
            {
                _velocityY -= deceleration * Time.deltaTime;
            }
            
            if(!backward && _velocityY < 0.0f)
            {
                _velocityY += deceleration * Time.deltaTime;
            }
            
            if(!left && _velocityX < 0.0f )
            {
                _velocityX += deceleration * Time.deltaTime;
            }
            
            if(!right && _velocityX > 0.0f)
            {
                _velocityX -= deceleration * Time.deltaTime;
            }
            
            
        }

        private void LockOrResetVelocity(bool left, bool right, bool forward, bool backward)
        {
            if(!left && !right && _velocityX != 0.0f && (_velocityX > -roundThreshold && _velocityX < roundThreshold))
            {
                _velocityX = 0.0f;
            }
            
            if(!forward && !backward && _velocityY != 0.0f && (_velocityY > -roundThreshold && _velocityY < roundThreshold))
            {
                _velocityY = 0.0f;
            }


            if (forward && _velocityY > maxVelocity)
            {
                _velocityY -= Time.deltaTime * deceleration;
                
                if(_velocityY > maxVelocity && _velocityY < (maxVelocity + roundThreshold))
                {
                    _velocityY = maxVelocity;
                }
            }
            else if(forward && _velocityY < maxVelocity && _velocityY > (maxVelocity - roundThreshold))
            {
                _velocityY = maxVelocity;
            }
            
            
            if (backward && _velocityY < -maxVelocity)
            {
                _velocityY += Time.deltaTime * deceleration;
                
                if(_velocityY < -maxVelocity && _velocityY > (-maxVelocity - roundThreshold))
                {
                    _velocityY = -maxVelocity;
                }
            }
            else if(backward && _velocityY > -maxVelocity && _velocityY < (-maxVelocity + roundThreshold))
            {
                _velocityY = -maxVelocity;
            }
            
            
            if (left && _velocityX < -maxVelocity)
            {
                _velocityX += Time.deltaTime * deceleration;
                
                if(_velocityX < -maxVelocity && _velocityX > (-maxVelocity - roundThreshold))
                {
                    _velocityX = -maxVelocity;
                }
            }
            else if(left && _velocityX > -maxVelocity && _velocityX < (-maxVelocity + roundThreshold))
            {
                _velocityX = -maxVelocity;
            }
            
            
            if (right && _velocityX > maxVelocity)
            {
                _velocityX -= Time.deltaTime * deceleration;
                
                if(_velocityX > maxVelocity && _velocityX < (maxVelocity + roundThreshold))
                {
                    _velocityX = maxVelocity;
                }
            }
            else if(right && _velocityX < maxVelocity && _velocityX > (maxVelocity - roundThreshold))
            {
                _velocityX = maxVelocity;
            }
        }


        internal void PlayAnimation(AnimationState state)
        {

            if (_currentState == state)
            {
                return;
            }
            
            switch (state)
            {
                case AnimationState.Idle:
                    animator.CrossFade(Idle, 0.1f);
                    break;
                case AnimationState.Running:
                    animator.CrossFade(RunBlendTree, 0.1f);
                    break;
                case AnimationState.WallRunningLeft:
                    animator.CrossFade(WallRunLeft, 0.1f);
                    break;
                case AnimationState.WallRunningRight:
                    animator.CrossFade(WallRunRight, 0.1f);
                    break;
                case AnimationState.Jumping:
                    break;
                case AnimationState.Sliding:
                    animator.CrossFade(Slide, 0.1f);
                    break;
                case AnimationState.Crouch:
                    animator.CrossFade(CrouchBlendTree, 0.1f);
                    break;
            }
            _currentState = state;
        }
    }
    
    public enum AnimationState
    {
        Idle,
        Running,
        Jumping,
        Sliding,
        WallRunningRight,
        WallRunningLeft,
        Crouch
    }
}


