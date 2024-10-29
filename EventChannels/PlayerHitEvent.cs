using Unity.Netcode;
using UnityEngine;

namespace EventChannels
{
    public struct PlayerHitEvent : INetworkSerializable
    {
        public ulong HitPlayerClientID;
            
        public int Damage;
        
        public ulong KillerPlayerNetworkObjectReference;

        public Vector3 ForceDirection ;
        
        public float HitForce;
        
        public PlayerHitEvent( ulong killerPlayerNetworkObjectReference, int damage, Vector3 forceDirection, ulong hitPlayerClientID, float hitForce)
        {
            
            Damage = damage;
            ForceDirection = forceDirection;
            HitPlayerClientID = hitPlayerClientID;
            HitForce = hitForce;
            KillerPlayerNetworkObjectReference = killerPlayerNetworkObjectReference;
        }

    
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HitPlayerClientID);
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref KillerPlayerNetworkObjectReference);
       
            serializer.SerializeValue(ref ForceDirection);
            serializer.SerializeValue(ref HitForce);
        }
    }
}