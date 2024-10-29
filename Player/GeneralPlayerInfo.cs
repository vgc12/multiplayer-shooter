using EventChannels;
using StateMachines.PlayerMovement;
using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace Player
{
    public class GeneralPlayerInfo : NetworkBehaviour
    {
        public int PlayerKills { get; set; }
        public int PlayerDeaths { get; set; }

        [SerializeField] private Renderer[] thirdPersonRenderers;
        
        [SerializeField] private Renderer[] firstPersonRenderers;
        
        [SerializeField] private Collider[] thirdPersonColliders;
        
        [SerializeField] private Collider[] firstPersonColliders;
        
       
        
     private GenericEventChannelScriptableObject<PlayerHitEvent> _playerHitEventChannel;
        
        public Animator Animator { get; private set; }
        
        public PlayerMovementStateMachine PlayerMovement { get; private set; }
        
        public RigTarget RigTarget { get; private set; }
        
        
        public override void OnNetworkSpawn()
        {
        
            Animator = GetComponentInChildren<Animator>();
            PlayerMovement = GetComponent<PlayerMovementStateMachine>();
            RigTarget = GetComponentInChildren<RigTarget>();
            if (!IsOwner)
            {
                ToggleFirstPersonRenderers(false);
                ToggleFirstPersonColliders(false);
                enabled = false;
            }
            else
            {
                ToggleThirdPersonRenderers(false);
                ToggleThirdPersonColliders(false);
            }
        }
        
        
        public void ToggleFirstPersonRenderers(bool value)
        {
            foreach (var firstPersonRenderer in firstPersonRenderers)
            {
                firstPersonRenderer.enabled = value;
            }
        }
        
        
        public void ToggleThirdPersonRenderers(bool value)
        {
            foreach (var playerRenderer in thirdPersonRenderers)
            {
                playerRenderer.enabled = value;
            }
        }
        
        public void ToggleFirstPersonColliders(bool value)
        {
            
            foreach (var firstPersonCollider in firstPersonColliders)
            {
                firstPersonCollider.enabled = value;
            }
        }
        
        public void ToggleThirdPersonColliders(bool value)
        {
         
            foreach (var thirdPersonCollider in thirdPersonColliders)
            {
                thirdPersonCollider.enabled = value;
            }
        }
        
        
        
    
        private void Awake()
        {
            _playerHitEventChannel = EventChannelAccessor.Instance.playerHitEventChannel;
            _playerHitEventChannel.OnEventRaised += OnPlayerHit;
        }

        private void OnPlayerHit(PlayerHitEvent arg0)
        {
            if (arg0.HitPlayerClientID == OwnerClientId)
            {
                PlayerDeaths++;
            }
            else
            {
                PlayerKills++;
            }
        }
    }
}
