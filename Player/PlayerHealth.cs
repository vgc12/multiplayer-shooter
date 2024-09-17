using System.Collections;
using EventChannels;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerHealth : NetworkBehaviour
    {
    
        public NetworkVariable<int> health = new(writePerm: NetworkVariableWritePermission.Server);


        public GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
        
        
        public ushort maxHealth = 100;
    
        private MeshRenderer[] _meshRenderers;
    
        private Collider[] _colliders;
    

        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                health.Value = maxHealth;
                playerHitEventChannel.OnEventRaised += OnPlayerHit;
            }

            _colliders = GetComponentsInChildren<Collider>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        
            if (!IsOwner)
            {
                enabled = false;
            }

        }

    private void OnPlayerHit(PlayerHitEvent playerHitEvent)
    {
        if (playerHitEvent.HitPlayerClientID == OwnerClientId)
        {
            DamagePlayer(100);
        }

    }
    
    
        public void DamagePlayer(int damage)
        {
        
            health.Value -= damage;
            if (health.Value <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            ToggleCollidersAndRendererClientRpc(false);
            
            yield return new WaitForSeconds(3);
            ToggleCollidersAndRendererClientRpc(true);
          
            
            health.Value = maxHealth;
            yield return null;
        }


        [ClientRpc]
        private void ToggleCollidersAndRendererClientRpc(bool toggle)
        {
            foreach (var col in _colliders)
            {
                col.enabled = toggle;
            }


            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.enabled = toggle;
            }
            GetComponent<Rigidbody>().isKinematic = !toggle;
            
            if (IsOwner)
            {
                GetComponent<ClientNetworkTransform>().Teleport(PlayerManager.Instance.respawnPoints[Random.Range(0,PlayerManager.Instance.respawnPoints.Length)].position, quaternion.identity,
                    new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
            
            }
        }
    }
}
