using System;
using EventChannels;
using Unity.Netcode;
using UnityEngine;
using Utilities;

public class DamageManager : NetworkBehaviour
{
    private GenericEventChannelScriptableObject<PlayerHitEvent> _playerHitEventChannel;

    public static DamageManager Instance { get; private set; }
    
    public override void OnNetworkSpawn()
    {
      if(Instance == null)
          Instance = this;
      else
      {
          Destroy(gameObject);
      }
    }
    
    private void Awake()
    {
        _playerHitEventChannel = EventChannelAccessor.Instance.playerHitEventChannel;
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryDamagePlayerServerRpc(PlayerHitEvent playerHitEvent, ServerRpcParams serverRpcParams = default)
    {
            
        _playerHitEventChannel.RaiseEvent(playerHitEvent);
               
    }
}
