using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorScripts;
using EventChannels;
using Player;
//using EditorScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons.Guns.Ammo;

namespace Weapons.Guns.ScriptableObjects
{

    public class Gun : NetworkBehaviour
    {
       
        public bool shouldShoot;
        
        
        [SerializeField] private Transform cameraHolder;
        
       
        [SerializeField] private GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
        [SerializeField] private GenericEventChannelScriptableObject<AmmoPickedUpEvent> powerUpPickedUpEventChannel;

        [ScriptableObjectDropdown]
        public TrailConfigScriptableObject trailConfiguration;

        public GameObject impact;

        public bool shouldHaveSpread = false;

        private MonoBehaviour _activeMonoBehavior;
        private GameObject _model;
        private float _lastShootTime;
        private ParticleSystem _muzzleFlash;
        [SerializeField] private GameObject trailObject;


        private List<AmmoScriptableObject> _ammoPickUps;
        public AmmoScriptableObject defaultAmmo;
       
        public float fireRate = 0.5f;
        private float _initialFireRate;
        public LayerMask hitMask;

        public Vector3 spread = new(0.1f, 0.1f, 0.1f);

        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                powerUpPickedUpEventChannel.OnEventRaised += OnPowerUpPickedUp;
            }
            
            if (IsOwner) return;
            gameObject.layer = LayerMask.NameToLayer("OtherPlayerWeapons");
            enabled = false;
           
        }

        public void Awake()
        {

            _initialFireRate = fireRate;
            _lastShootTime = 0;
            _ammoPickUps = new List<AmmoScriptableObject>();

            powerUpPickedUpEventChannel.OnEventRaised += OnPowerUpPickedUp;
            
            gameObject.layer = LayerMask.NameToLayer("PersonalWeapon");
            _muzzleFlash = GetComponentInChildren<ParticleSystem>();
            _ammoPickUps.Add(defaultAmmo);
          
        }

        
        public void OnPowerUpPickedUp(AmmoPickedUpEvent ammoPickedUpEvent)
        {
           
                var ammoScriptableObject = ammoPickedUpEvent.AmmoPickUp.ammoScriptableObject;
                ammoScriptableObject.ResetTimeLeft();
                
                if (_ammoPickUps.Contains(ammoScriptableObject))
                {
                    
                    return;
                };
                
                _ammoPickUps.Add(ammoPickedUpEvent.AmmoPickUp.ammoScriptableObject);
                
                if(ammoScriptableObject is AutomaticAmmoScriptableObject automaticAmmo)
                {
                    fireRate = automaticAmmo.fireRate;
                }
                
                StartCoroutine(StartAmmoTimer(ammoScriptableObject));
        }
        
        
        private IEnumerator StartAmmoTimer(AmmoScriptableObject ammoScriptableObject)
        {
            
            while (ammoScriptableObject.TimeLeft > 0)
            {
                //Debug.LogError(ammoScriptableObject.GetType() + " | " + ammoScriptableObject.TimeLeft);
                ammoScriptableObject.TimeLeft -= Time.deltaTime;
                yield return null;
            }
            if(ammoScriptableObject is AutomaticAmmoScriptableObject automaticAmmo)
            {
                fireRate = _initialFireRate;
            }
        
            _ammoPickUps.Remove(ammoScriptableObject);
            
        }
        

        private void Update()
        {
            
            if (shouldShoot)
            {
                Shoot();
            }
        }

       

        private Vector3 _direction;
        public void Shoot()
        
        {
   
            if (!(Time.time > fireRate + _lastShootTime)) return;
            
            _lastShootTime = Time.time;
            
            _direction = GetDirection();
            FireBullet(_direction);

        }


        private RaycastHit _hit;

        private List<RaycastHit> _hits;
        public List<Vector3> Hits => _hits.Select(h => h.point).ToList();

        private void OnDrawGizmos()
        {
            if (_hits == null) return;
           
            foreach (RaycastHit hit in _hits)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.3f);
            }
        }


        // this is terrible
        private void FireBullet(Vector3 direction)
        {
            if (Physics.Raycast(cameraHolder.position, direction, out var hit, float.MaxValue, hitMask))
            {
                var initialPos = _muzzleFlash.transform.position;

                var hits = new List<RaycastHit>();
                foreach (AmmoScriptableObject powerUp in _ammoPickUps)
                {
                    var powerUpHits = powerUp.OnHit(hit, direction);
                    foreach (var rayCastHit in powerUpHits.Where(rayCastHit => !hits.Contains(rayCastHit)))
                    {
                       
                        
                        hits.Add(rayCastHit);
                        var info = new HitInfo(initialPos, rayCastHit.point, rayCastHit);
                        if (rayCastHit.transform.root.TryGetComponent(out PlayerHealth health) &&
                            health.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                        {
                            TryDamagePlayerServerRpc(health.OwnerClientId);
                        }

                        StartCoroutine(PlayTrail(info));
                        RequestPlayTrailServerRpc(info);
                        initialPos = rayCastHit.point;
                    }
                }

                _hits = hits;
            }
            else
            {
                var info = new HitInfo(_muzzleFlash.transform.position, _muzzleFlash.transform.position + GetDirection() * trailConfiguration.missDistance, new RaycastHit());


                StartCoroutine(PlayTrail(info));
                RequestPlayTrailServerRpc(info);
             

            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void TryDamagePlayerServerRpc(ulong hitClientID, ServerRpcParams serverRpcParams = default)
        {
            playerHitEventChannel.RaiseEvent(new PlayerHitEvent(serverRpcParams.Receive.SenderClientId,hitClientID));
               
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlayTrailServerRpc(HitInfo info)
        {
            
                PlayTrailClientRpc(info);
         

        }
     

        [ClientRpc]
        private void PlayTrailClientRpc(HitInfo info)
        {
            if (!IsOwner)
            {
                StartCoroutine(PlayTrail(info));
            }
        }

        private IEnumerator PlayTrail(HitInfo info)
        {
            _muzzleFlash.Play();
            TrailRenderer trailInstance = NetworkObjectPool.Singleton.GetNetworkObject(trailObject,info.Start, Quaternion.identity).GetComponent<TrailRenderer>();
            trailInstance.gameObject.SetActive(true);
            trailInstance.transform.position = info.Start;
            yield return null;
            trailInstance.emitting = true;

            float distance = Vector3.Distance(info.Start, info.End);
            float remainingDistance = distance;
            while (remainingDistance > 0)
            {
                trailInstance.transform.position = Vector3.Lerp(info.Start, info.End, Mathf.Clamp01(1 - (remainingDistance / distance)));
                remainingDistance -= trailConfiguration.trailSpeed * Time.deltaTime;
                yield return null;
            }

            trailInstance.transform.position = info.End;

            if (info.HitCollider){
                
                //var i = Instantiate(impact).GetComponent<ParticleSystem>();
                var i = NetworkObjectPool.Singleton.GetNetworkObject(impact,info.HitPoint,  Quaternion.LookRotation(info.HitNormal)).GetComponent<ParticleSystem>();
               // i.transform.position = info.HitPoint;
               // i.transform.rotation = Quaternion.LookRotation(info.HitNormal);
            }

            yield return new WaitForSeconds(trailConfiguration.duration);
            yield return null;
            trailInstance.emitting = false;
            trailInstance.gameObject.SetActive(false);
        }
        
        
        private Vector3 GetDirection()
        {
            Vector3 direction = cameraHolder.forward;
            if (shouldHaveSpread)
            {
                direction += new Vector3(UnityEngine.Random.Range(-spread.x, spread.x),
                    UnityEngine.Random.Range(-spread.y, spread.y),
                    UnityEngine.Random.Range(-spread.z, spread.z));
                direction.Normalize();
            }
            return direction;
        }
        
        
        private struct HitInfo : INetworkSerializable
        {
            internal Vector3 Start;
            internal Vector3 End;
            internal Vector3 HitPoint;
            internal Vector3 HitNormal;
            
            
            internal bool HitCollider;

            public HitInfo(Vector3 start, Vector3 end, RaycastHit hit)
            {
                Start = start;
                End = end;
                HitCollider = hit.collider;
                if (hit.collider)
                {
                    HitPoint = hit.point;
                    HitNormal = hit.normal;
                }
                else
                {
                    HitPoint = Vector3.zero;
                    HitNormal = Vector3.zero;
                }
            }


            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Start);
                serializer.SerializeValue(ref End);
                serializer.SerializeValue(ref HitPoint);
                serializer.SerializeValue(ref HitNormal);
                serializer.SerializeValue(ref HitCollider);
                
            }
        }
    }
}
