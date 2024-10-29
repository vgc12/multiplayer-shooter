using Unity.Netcode;

namespace EventChannels
{
    public struct PlayerDeathEvent 
    {
        public readonly NetworkObject KilledNetworkObject;
        public readonly ulong KilledClientId;
        public readonly ulong KillerClientId;
        

        public PlayerDeathEvent(ulong killerClientId, ulong killedClientId,  NetworkObject killedNetworkObject)
        {
            KillerClientId = killerClientId;
            KilledClientId = killedClientId;
            KilledNetworkObject = killedNetworkObject;
            
        }
    }
}