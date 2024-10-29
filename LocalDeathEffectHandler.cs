using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventChannels;
using Player;
using StateMachines.PlayerMovement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;

public class LocalDeathEffectHandler : NetworkBehaviour
{
    
    private GenericEventChannelScriptableObject<PlayerDeathEvent> _playerDeathEventChannel;
    private GenericEventChannelScriptableObject<RagdollSpawnEvent> _ragdollSpawnEventChannel;
    
    [SerializeField] private GameObject falsePlayerPrefab;
    [SerializeField] private GameObject playerDeathObject;
    private GameObject _spawnedPlayerClone;

    private ObjectPool<GameObject> _ragdollPool;
    
    public static LocalDeathEffectHandler Instance { get; private set; }
    
    private RigTarget _cloneRigTarget;
    private Animator _cloneAnimator;
    private PlayerKillerCloneInformation _playerKillerCloneInformation;
    private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
    private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        _playerDeathEventChannel = EventChannelAccessor.Instance.playerDeathEventChannel;
        _ragdollSpawnEventChannel = EventChannelAccessor.Instance.ragdollSpawnEventChannel;
        //_playerDeathEventChannel.OnEventRaised += OnPlayerDeath;
        _ragdollPool = new ObjectPool<GameObject>(() => Instantiate(playerDeathObject), OnTakeFromPool(),
            obj => obj.SetActive(false), Destroy, true, 15, 100); 
    }

    private Action<GameObject> OnTakeFromPool()
    {
        return obj =>
        {
            obj.SetActive(true);
            StartCoroutine(DeactivateObject(obj));
        };
    }

    private IEnumerator DeactivateObject(GameObject obj, float time = 30f)
    {
        yield return new WaitForSeconds(time);
        _ragdollPool.Release(obj);
    }

/*
    private void OnPlayerDeath(PlayerDeathEvent playerDeathEvent)
    {
        if (playerDeathEvent.KilledClientId == NetworkManager.LocalClientId)
        {
            SpawnPlayerModelShowingFrameKilled(playerDeathEvent);
        }
        
    }
    private void SpawnPlayerModelShowingFrameKilled(PlayerDeathEvent playerDeathEvent)
    {
        playerDeathEvent.KillerNetworkObjectReference.TryGet(out var killerNetworkObject, NetworkManager.Singleton);
        var info = killerNetworkObject.GetComponent<GeneralPlayerInfo>();
        
        var rotation = info.PlayerMovement.orientation.rotation;
        var killerRigTarget = info.RigTarget;
        var realPlayerAnimator = info.Animator;
        
        if (!_spawnedPlayerClone)
        {
            _spawnedPlayerClone = Instantiate(falsePlayerPrefab, killerNetworkObject.transform.position, killerNetworkObject.transform.rotation);
            _playerKillerCloneInformation = _spawnedPlayerClone.GetComponent<PlayerKillerCloneInformation>();
            _cloneRigTarget = _playerKillerCloneInformation.rigTarget;
            _cloneAnimator = _playerKillerCloneInformation.animator;
        }
        else
        {
            _spawnedPlayerClone.transform.position = killerNetworkObject.transform.position;
            _spawnedPlayerClone.transform.rotation = killerNetworkObject.transform.rotation;
        }
        
        _playerKillerCloneInformation.orientation.rotation = rotation;
        
        _cloneRigTarget.transform.position = killerRigTarget.transform.position;

        var currentInfo = realPlayerAnimator.GetCurrentAnimatorStateInfo(0);
        var horizontalMovement = realPlayerAnimator.GetFloat(HorizontalMovement);
        var verticalMovement = realPlayerAnimator.GetFloat(VerticalMovement);
        var otherInfo = realPlayerAnimator.GetCurrentAnimatorStateInfo(1);
        _cloneAnimator.SetFloat(HorizontalMovement, horizontalMovement);
        _cloneAnimator.SetFloat(VerticalMovement, verticalMovement);
        
        _cloneAnimator.speed = 0;
        _cloneAnimator.Play(currentInfo.fullPathHash, 0, currentInfo.normalizedTime);
        _cloneAnimator.Play(otherInfo.fullPathHash, 1, otherInfo.normalizedTime);
        _spawnedPlayerClone.layer = LayerMask.NameToLayer("Killer");
    }
    */
    
 
    [ClientRpc]
    public void SpawnDeathModelClientRpc(Vector3 hitDirection,float hitForce, ulong killedClientId)
    {

        var killedNetworkObject = PlayerManager.Instance.players.First(p => p.OwnerClientId == killedClientId);
        var animator = killedNetworkObject.GetComponentInChildren<Animator>();
        var baseLayerInfo = animator.GetCurrentAnimatorStateInfo(0);
        var firstLayerInfo = animator.GetCurrentAnimatorStateInfo(1);
        var obj = _ragdollPool.Get();
        
        var objAnimator = obj.GetComponentInChildren<Animator>();
        objAnimator.Play(baseLayerInfo.fullPathHash, 0, baseLayerInfo.normalizedTime);
        objAnimator.Play(firstLayerInfo.fullPathHash, 1, firstLayerInfo.normalizedTime);
        objAnimator.enabled = false;
        
        obj.transform.position = killedNetworkObject.transform.position;
        obj.transform.rotation = killedNetworkObject.GetComponent<PlayerMovementStateMachine>().orientation.rotation;

        
        obj.GetComponentInChildren<Rigidbody>().AddForce(hitDirection * hitForce, ForceMode.Impulse);
        
        
        _ragdollSpawnEventChannel.RaiseEvent(new RagdollSpawnEvent(obj, killedClientId));
    }
    
    
}

public struct RagdollSpawnEvent
{
    public GameObject Ragdoll;
    public ulong KilledClientId;
    public RagdollSpawnEvent (GameObject ragdoll, ulong killedClientId)
    {
        Ragdoll = ragdoll;
        KilledClientId = killedClientId;
    }
}