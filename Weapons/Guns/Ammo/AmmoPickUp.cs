using System;
using System.Collections;
using System.Collections.Generic;
using EventChannels;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Weapons.Guns.Ammo
{
    public class AmmoPickUp : MonoBehaviour
    {
        
        public AmmoScriptableObject ammoScriptableObject;
        public int id;
        [SerializeField] private GenericEventChannelScriptableObject<AmmoPickedUpEvent> ammoPickedUpEventChannel;

     

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.TryGetComponent(out PlayerHealth playerHealth))
            {
                
                ammoPickedUpEventChannel.RaiseEvent(new AmmoPickedUpEvent(this));
                AmmoManager.Instance.DespawnAmmoPickupServerRpc(id);
            }
        }
     
     
        
      
    }
}