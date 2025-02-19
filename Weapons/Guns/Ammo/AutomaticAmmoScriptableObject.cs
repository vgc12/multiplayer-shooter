﻿using UnityEngine;
using Weapons.Guns.ScriptableObjects;

namespace Weapons.Guns.Ammo
{
    [CreateAssetMenu(fileName = "Automatic Ammo Pickup", menuName = "Ammo Pick Ups/Automatic Ammo Pickup", order = 1)]
    public class AutomaticAmmoScriptableObject : AmmoScriptableObject
    {
        public float fireRate = 0.5f;
        
        protected override void Awake()
        {
            base.Awake();
            ammoType = AmmoType.Automatic;
        }
        
        
    }
}