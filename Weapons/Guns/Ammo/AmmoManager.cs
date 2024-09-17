using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons.Guns.Ammo
{
    public class AmmoManager : NetworkBehaviour
    {
        public List<AmmoPickUp> mapAmmoPickUps = new List<AmmoPickUp>();

        public List<AmmoScriptableObject> possibleAmmoPickUps = new List<AmmoScriptableObject>();
        
        public static AmmoManager Instance { get; private set; }
        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if(mapAmmoPickUps.Count == 0)
            {
                mapAmmoPickUps = FindObjectsByType<AmmoPickUp>(FindObjectsSortMode.None).ToList();
            }
            

            for (var i = 0; i < mapAmmoPickUps.Count; i++)
            {
                var ammoPickUp = mapAmmoPickUps[i];
                ammoPickUp.id = i;
                ammoPickUp.gameObject.SetActive(true);
            }
            
        }

        


        [ServerRpc(RequireOwnership = false)]
        public void DespawnAmmoPickupServerRpc(int ammoPickupId)
        {
            
            StartCoroutine(ToggleAmmoPickup(ammoPickupId));
        }

        private IEnumerator ToggleAmmoPickup(int ammoPickupId)
        {
            DespawnAmmoPickupClientRpc(ammoPickupId, Random.Range(0, possibleAmmoPickUps.Count));
            yield return new WaitForSeconds(5);
            RespawnAmmoPickupClientRpc(ammoPickupId);
        }

        [ClientRpc]
        private void RespawnAmmoPickupClientRpc(int ammoPickupId)
        {
            mapAmmoPickUps[ammoPickupId].gameObject.SetActive(true);
        }
        
        [ClientRpc]
        private void DespawnAmmoPickupClientRpc(int ammoPickupId, int pickupType, ClientRpcParams clientRpcParams = default)
        {
            
            mapAmmoPickUps[ammoPickupId].ammoScriptableObject = possibleAmmoPickUps[pickupType];
            mapAmmoPickUps[ammoPickupId].gameObject.SetActive(false);
            
        }

        public void SyncAmmoPickups(ulong clientid)
        {
            foreach (var ammoPickUp in mapAmmoPickUps)
            {
                if (!ammoPickUp.isActiveAndEnabled)
                {
                    DespawnAmmoPickupClientRpc(ammoPickUp.id,
                        possibleAmmoPickUps.IndexOf(ammoPickUp.ammoScriptableObject),
                        new ClientRpcParams()
                            { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientid } } });
                    
                }
                
                
            }
        }
    }
}