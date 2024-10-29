using System.Collections;
using System.Linq;
using EventChannels;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerHealth : NetworkBehaviour
    {
    
        public NetworkVariable<int> health = new(writePerm: NetworkVariableWritePermission.Server);


        [SerializeField] private GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
        
        [SerializeField] private GenericEventChannelScriptableObject<PlayerDeathEvent> playerDeathEventChannel;
        
        [SerializeField] private GenericEventChannelScriptableObject<PlayerSpawnEvent> playerSpawnEventChannel;
        
        [SerializeField] private ushort maxHealth = 100;
    
     

        private GeneralPlayerInfo _playerInfo;
    


        public GameObject playerDeathObject;

        
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                health.Value = maxHealth;
                playerHitEventChannel.OnEventRaised += OnPlayerHit;
            }

       
            _playerInfo = GetComponent<GeneralPlayerInfo>();
        
            
            if (!IsOwner)
            {
                enabled = false;
            }

        }

        private void OnPlayerHit(PlayerHitEvent playerHitEvent)
        {
            if (playerHitEvent.HitPlayerClientID == OwnerClientId)
            {
                DamagePlayer(playerHitEvent);
            }

        }
    
    
        public void DamagePlayer(PlayerHitEvent playerHitEvent)
        {
        
            health.Value -= playerHitEvent.Damage;
            if (health.Value <= 0)
            {
                StartCoroutine(Respawn(playerHitEvent));
            }
        }

       

        private IEnumerator Respawn(PlayerHitEvent playerHitEvent)
        {
            float respawnTime = 15f;
            
           
            LocalDeathEffectHandler.Instance.SpawnDeathModelClientRpc(playerHitEvent.ForceDirection,playerHitEvent.HitForce, playerHitEvent.HitPlayerClientID);
            ToggleCollidersAndRendererClientRpc(false);
            SendDeathEventOutClientRpc(playerHitEvent);
            
            yield return new WaitForSeconds(respawnTime);
            SendSpawnEventClientRpc(playerHitEvent.HitPlayerClientID);
            ToggleCollidersAndRendererClientRpc(true);
          
            
            health.Value = maxHealth;
            yield return null;
        }

       

        [ClientRpc]
        private void SendDeathEventOutClientRpc(PlayerHitEvent playerHitEvent)
        {
            if (OwnerClientId != playerHitEvent.HitPlayerClientID) return;
            
        
            var hitPlayer = PlayerManager.Instance.players.First(p =>
                p.GetComponent<NetworkObject>().OwnerClientId == playerHitEvent.HitPlayerClientID);
            playerDeathEventChannel.RaiseEvent(new PlayerDeathEvent(
                playerHitEvent.KillerPlayerNetworkObjectReference, playerHitEvent.HitPlayerClientID, hitPlayer));
        }
        
        
        
        
        
        // use clientrpc params
        [ClientRpc]
        private void SendSpawnEventClientRpc(ulong respawningClientId, ClientRpcParams clientRpcParams = default)
        {
            if (OwnerClientId == respawningClientId)
            {
                playerSpawnEventChannel.RaiseEvent(new PlayerSpawnEvent(respawningClientId));
            }
        }



        [ClientRpc]
        private void ToggleCollidersAndRendererClientRpc(bool isSpawning)
        {
            var rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = isSpawning ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete;
            GetComponent<Rigidbody>().isKinematic = !isSpawning;
           
            
        
            if (!IsOwner )
            {
                _playerInfo.ToggleThirdPersonRenderers(isSpawning);
                _playerInfo.ToggleThirdPersonColliders(isSpawning);
                return;
            }
            
            if (!isSpawning)
            {
                var respawnPosition =
                    PlayerManager.Instance.respawnPoints[
                        Random.Range(0, PlayerManager.Instance.respawnPoints.Length)];
                GetComponent<ClientNetworkTransform>().Teleport(respawnPosition.position, respawnPosition.rotation,
                    new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
            }
            
            
            _playerInfo.ToggleFirstPersonRenderers(isSpawning);
            _playerInfo.ToggleFirstPersonColliders(isSpawning);
            
        }
        
        
        
        
    }
}
