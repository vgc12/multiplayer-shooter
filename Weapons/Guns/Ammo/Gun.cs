using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using EditorScripts;
using EventChannels;
using Player;
using UI;
using Unity.Netcode;
using UnityEngine;
using Utilities;
using Weapons.Guns.Ammo;

namespace Weapons.Guns.ScriptableObjects
{

    public enum AmmoType
    {
        Default,
        Ricochet,
        Automatic
    }
    
    public class Gun : NetworkBehaviour
    {
       
        public bool shouldShoot;
        
        
        [SerializeField] private Transform cameraHolder;
        
       
        private GenericEventChannelScriptableObject<PlayerHitEvent> _playerHitEventChannel;
        private GenericEventChannelScriptableObject<AmmoPickedUpEvent> _powerUpPickedUpEventChannel;
        private GenericEventChannelScriptableObject<PlayerDeathEvent> _playerDeathEventChannel;
        private GenericEventChannelScriptableObject<PlayerSpawnEvent> _playerSpawnEventChannel;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;
        
        //[ScriptableObjectDropdown]
        public TrailConfigScriptableObject trailConfiguration;

        public GameObject impact;

        public bool shouldHaveSpread = false;

        private MonoBehaviour _activeMonoBehavior;
        private GameObject _model;
        private float _lastShootTime;
        [SerializeField] private ParticleSystem firstPersonMuzzleFlash;
        [SerializeField] private ParticleSystem otherPlayerMuzzleFlash;
        [SerializeField] private GameObject trailObject;


        private Dictionary<AmmoType,AmmoScriptableObject> _ammoPickUps;
        public AmmoScriptableObject defaultAmmo;
       
        public float fireRate = 0.5f;
        private float _initialFireRate;
        public LayerMask hitMask;

        public Vector3 spread = new(0.1f, 0.1f, 0.1f);

        private PlayerInputActions _playerInputActions;
        
        private bool _isPaused;
        private bool _isDead;
        public override void OnNetworkSpawn()
        {
          
            if (IsOwner) return;
            gameObject.layer = LayerMask.NameToLayer("OtherPlayerWeapons");
            enabled = false;
           
        }
    
       
        public void Awake()
        {

            _initialFireRate = fireRate;
            _lastShootTime = 0;
            _ammoPickUps = new Dictionary<AmmoType, AmmoScriptableObject>();

            _playerDeathEventChannel = EventChannelAccessor.Instance.playerDeathEventChannel;
            _playerHitEventChannel = EventChannelAccessor.Instance.playerHitEventChannel;
            _powerUpPickedUpEventChannel = EventChannelAccessor.Instance.ammoPickedUpEventChannel;
            _playerSpawnEventChannel = EventChannelAccessor.Instance.playerSpawnEventChannel;
            _pauseEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
            
            _powerUpPickedUpEventChannel.OnEventRaised += OnPowerUpPickedUp;
            _playerDeathEventChannel.OnEventRaised += OnDeath;
            _playerSpawnEventChannel.OnEventRaised += OnPlayerSpawned;
            _pauseEventChannel.OnEventRaised += OnPause;
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.Player.Fire.performed += _ => shouldShoot = true;
            _playerInputActions.Player.Fire.canceled += _ => shouldShoot = false;
            
            gameObject.layer = LayerMask.NameToLayer("PersonalWeapon");
            firstPersonMuzzleFlash = GetComponentInChildren<ParticleSystem>();
            _ammoPickUps.Add(defaultAmmo.ammoType, defaultAmmo);
          
        }

        private void OnPause(PauseEvent pause)
        {
            if(pause.PlayerId != NetworkManager.Singleton.LocalClientId || !IsOwner)
                return;
            _isPaused = pause.IsPaused;
        }

        private void OnPlayerSpawned(PlayerSpawnEvent playerSpawnEvent)
        {
            if (playerSpawnEvent.RespawningClientId == NetworkManager.Singleton.LocalClientId)
            {
                _isDead = false;
            }
        }

        private void OnDeath(PlayerDeathEvent playerDeathEvent)
        {
            if (playerDeathEvent.KilledClientId == NetworkManager.Singleton.LocalClientId)
            {
                _isDead = true;
            }
        }

        
        public void OnPowerUpPickedUp(AmmoPickedUpEvent ammoPickedUpEvent)
        {
           
                var ammoScriptableObject = ammoPickedUpEvent.AmmoPickUp.ammoScriptableObject;
                ammoScriptableObject.ResetTimeLeft();
                
                if (_ammoPickUps.ContainsKey(ammoScriptableObject.ammoType))
                {
                    return;
                };
                
                _ammoPickUps.Add( ammoScriptableObject.ammoType, ammoScriptableObject);
                
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
            if(ammoScriptableObject is AutomaticAmmoScriptableObject)
            {
                fireRate = _initialFireRate;
            }
        
            _ammoPickUps.Remove(ammoScriptableObject.ammoType);
            
        }
        

        private void Update()
        {
            
           
            Shoot();
            
        }

       

        private Vector3 _direction;
        public void Shoot()
        
        {
            
   
            if (!(Time.time > fireRate + _lastShootTime) || !shouldShoot || _isDead || _isPaused) return;
            
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


 
        private void FireBullet(Vector3 direction)
        {
            if (Physics.Raycast(cameraHolder.position, direction, out var hit, float.MaxValue, hitMask))
            {
                var initialPos = firstPersonMuzzleFlash.transform.position;

                var hits = new List<RaycastHit>();
    
                List<RaycastHit> allHits = new List<RaycastHit>(){hit};

                if (_ammoPickUps.TryGetValue(AmmoType.Ricochet, out var ricochetAmmo))
                {
                    
                    CalculateRicochetHits(direction, ricochetAmmo as RicochetAmmoScriptableObject, ref allHits);
                }

                foreach (var rayCastHit in allHits)
                {
                    var info = new HitInfo(initialPos, rayCastHit.point, rayCastHit, otherPlayerMuzzleFlash.transform.position, false);
                    bool isPlayer = rayCastHit.transform.root.TryGetComponent(out PlayerHealth health);
                    if ( isPlayer && health.OwnerClientId != OwnerClientId)
                    {
                        //TryDamagePlayerServerRpc(health.OwnerClientId,rayCastHit.normal, new NetworkObjectReference(transform.root.GetComponent<NetworkObject>()));
                        DamageManager.Instance.TryDamagePlayerServerRpc(new PlayerHitEvent(OwnerClientId, 100,
                            -rayCastHit.normal, health.OwnerClientId, 1));
                    }
                    StartCoroutine(PlayTrail(info));
                    RequestPlayTrailServerRpc(info);
                    initialPos = rayCastHit.point;
                }
                

                _hits = hits;
            }
            else
            {
                var info = new HitInfo(firstPersonMuzzleFlash.transform.position,
                    firstPersonMuzzleFlash.transform.position + GetDirection() * trailConfiguration.missDistance,
                    new RaycastHit(),otherPlayerMuzzleFlash.transform.position,false);


                StartCoroutine(PlayTrail(info));
                RequestPlayTrailServerRpc(info);
             

            }
        }

        private static void CalculateRicochetHits(Vector3 direction, RicochetAmmoScriptableObject ricochetAmmo,
            ref List<RaycastHit> allHits)
        {
            int reflectionCount = 0;
            RaycastHit hit = allHits[0];
            while (reflectionCount < ricochetAmmo.maxReflections)
            {
                
         
                Vector3 reflectedDirection = Vector3.Reflect(direction, hit.normal);
                Ray ray = new Ray(hit.point, reflectedDirection);
                Debug.DrawRay( hit.point, reflectedDirection * 1000f, Color.red, 10f);
                if (!Physics.Raycast(ray, out RaycastHit newHit, float.MaxValue)) continue;
                allHits.Add(newHit);
                if (newHit.transform.root.TryGetComponent(out PlayerHealth health) && health.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                {
        
                    reflectionCount = 0;
                }
                else
                {

                    reflectionCount++;
                }

                hit = newHit;
                direction = reflectedDirection;


            }
        }




        [ServerRpc(RequireOwnership = false)]
        private void RequestPlayTrailServerRpc(HitInfo info)
        {
            info.ForOtherPlayers = true;
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
            firstPersonMuzzleFlash.Play();
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
                

                var i = NetworkObjectPool.Singleton.GetNetworkObject(impact,info.HitPoint,  Quaternion.LookRotation(info.HitNormal)).GetComponent<ParticleSystem>();
              
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
            internal Vector3 OtherPlayersStart;
            internal bool ForOtherPlayers;
            
            internal bool HitCollider;

            public HitInfo(Vector3 start, Vector3 end, RaycastHit hit, Vector3 otherPlayersStart, bool forOtherPlayers)
            {
                Start = start;
                End = end;
                OtherPlayersStart = otherPlayersStart;
                ForOtherPlayers = forOtherPlayers;
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
                serializer.SerializeValue(ref OtherPlayersStart);
                serializer.SerializeValue(ref ForOtherPlayers);
                
            }
        }
    }
}
