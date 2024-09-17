using System.Collections;
using System.Collections.Generic;
using EventChannels;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public int PlayerKills { get; set; }
    public int PlayerDeaths { get; set; }

    [SerializeField] private GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }
    
    private void Awake()
    {
        playerHitEventChannel.OnEventRaised += OnPlayerHit;
    }

    private void OnPlayerHit(PlayerHitEvent arg0)
    {
        if (arg0.HitPlayerClientID == OwnerClientId)
        {
            PlayerDeaths++;
        }
        else
        {
            PlayerKills++;
        }
    }
}
