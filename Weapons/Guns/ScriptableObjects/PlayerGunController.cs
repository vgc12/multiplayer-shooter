using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace Weapons.Guns.ScriptableObjects
{
    [DisallowMultipleComponent]
    public class PlayerGunController : NetworkBehaviour
    {


        public Gun gun;

        private PlayerInputActions _playerInputActions;

      
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
            }
        }

        public void Start()
        {
            
            
        }

    }
}
