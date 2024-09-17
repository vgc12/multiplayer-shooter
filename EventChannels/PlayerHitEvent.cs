using Unity.Netcode;
using UnityEngine;

namespace EventChannels
{
    public struct PlayerHitEvent
    {
        public readonly ulong KillerPlayerClientID;
        
        public readonly ulong HitPlayerClientID;
        
        public PlayerHitEvent(ulong killerPlayerClientID,ulong hitPlayerClientID)
        {
            KillerPlayerClientID = killerPlayerClientID;
            HitPlayerClientID = hitPlayerClientID;

        }
    }
}