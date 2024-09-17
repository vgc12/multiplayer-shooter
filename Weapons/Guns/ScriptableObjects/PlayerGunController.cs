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
        [SerializeField] private GunType gun;
        public List<Gun> spawnedGuns;
        [Space]
        public Gun currentGun;
        [SerializeField]
        private Transform cameraTransform;
        [SerializeField]
        private Transform gunRoationObject;
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
            //spawnedGuns = NetworkManager.Singleton.SpawnManager.GetClientOwnedObjects(OwnerClientId).Where(c => c.GetComponent<Gun>()).Select(c => c.gameObject).ToList();
            spawnedGuns = GetComponentsInChildren<Gun>().ToList();
            currentGun = spawnedGuns[0];
            
            spawnedGuns.ForEach(c => Debug.LogWarning(c.name));

            
            
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.Player.Fire.performed += _ => currentGun.shouldShoot = true;
            _playerInputActions.Player.Fire.canceled += _ => currentGun.shouldShoot = false;
            
        }

    }
}
