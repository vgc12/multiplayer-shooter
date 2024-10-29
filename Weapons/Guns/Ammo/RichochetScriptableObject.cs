using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using Weapons.Guns.ScriptableObjects;

namespace Weapons.Guns.Ammo
{
    [CreateAssetMenu(fileName = "Ricochet Ammo Pickup", menuName = "Ammo Pick Ups/Ricochet Ammo Pickup", order = 1)]
    public class RicochetAmmoScriptableObject : AmmoScriptableObject
    {
        public int maxReflections = 3;

        protected override void Awake()
        {
            base.Awake();
            ammoType = AmmoType.Ricochet;
        }


        public override List<RaycastHit> OnHit(RaycastHit hit, Vector3 bulletDirection)
        {
            int reflectionCount = 0;
            List<RaycastHit> allHits = new List<RaycastHit>(){hit};
            while (true)
            {
                //
                if (reflectionCount < maxReflections)
                {
                    Vector3 reflectedDirection = Vector3.Reflect(bulletDirection, hit.normal);
                    Ray ray = new Ray(hit.point, reflectedDirection);
                    Debug.DrawRay( hit.point, reflectedDirection * 1000f, Color.red, 10f);
                    if (Physics.Raycast(ray, out RaycastHit newHit, float.MaxValue))
                    {
                        allHits.Add(newHit);
                        Debug.Log(newHit.collider.name);
                        if (newHit.transform.root.TryGetComponent(out PlayerHealth health) && health.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                        {
            
                            reflectionCount = 0;
                        }
                        else
                        {

                            reflectionCount++;
                        }

                        hit = newHit;
                        bulletDirection = reflectedDirection;
                        continue;
                    }
                }

                break;
            }

            return allHits;
        }
    }

}