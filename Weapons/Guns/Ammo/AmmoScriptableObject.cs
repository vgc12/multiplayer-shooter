using System;
using System.Collections.Generic;
using EventChannels;
using Player;
using Unity.Netcode;
using UnityEngine;
using Weapons.Guns.ScriptableObjects;

namespace Weapons.Guns.Ammo
{
    [CreateAssetMenu(fileName = "Default Ammo Pickup", menuName = "Ammo Pick Ups/Default Ammo Pick Up", order = 1)]
    public class AmmoScriptableObject : ScriptableObject
    {
        public float maxDuration;
        
        public float TimeLeft { get; set; }

        private void Awake()
        {
            ResetTimeLeft();
            
        
        }

        public void ResetTimeLeft()
        {
            TimeLeft = maxDuration;
        }
        
        
        public virtual List<RaycastHit> OnHit(RaycastHit hit, Vector3 bulletDirection)
        {
            return new List<RaycastHit>(){hit};
        }


    }

}