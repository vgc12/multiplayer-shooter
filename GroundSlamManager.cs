using System;
using System.Collections.Generic;
using System.Linq;
using EventChannels;
using Player;
using StateMachines.PlayerLooking;
using StateMachines.PlayerMovement;
using Unity.Netcode;

using UnityEngine;
using Utilities;

public class GroundSlamManager : NetworkBehaviour
{
    private GenericEventChannelScriptableObject<GroundSlamEvent> _groundSlamEventChannel;
    private LayerMask _layerMask;
    public static GroundSlamManager Instance;
    
    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public void Awake()
    {
        _groundSlamEventChannel = EventChannelAccessor.Instance.groundSlamEventChannel;
       _groundSlamEventChannel.OnEventRaised += OnSlamEvent;
       _layerMask = LayerMask.GetMask("Player");
    }

    private void OnSlamEvent(GroundSlamEvent groundSlamEvent)
    {
  
        SlamEventServerRpc(groundSlamEvent);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SlamEventServerRpc( GroundSlamEvent groundSlamEvent)
    {
          
        SlamEventClientRpc(groundSlamEvent);
    }
        
    [ClientRpc]
    private void SlamEventClientRpc(GroundSlamEvent groundSlamEvent)
    {
        var hitPlayer = PlayerManager.Instance.players.First(p =>
            p.GetComponent<NetworkObject>().OwnerClientId == groundSlamEvent.HitPlayerClientId);
        
        hitPlayer.GetComponent<Rigidbody>().AddForce(((hitPlayer.transform.position - groundSlamEvent.Origin).normalized + Vector3.up ) * groundSlamEvent.Force, ForceMode.Impulse);

        hitPlayer.GetComponentInChildren<PlayerLookingStateMachine>().ShakeCamera();
    }

   
}
