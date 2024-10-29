using Unity.Netcode;
using UnityEngine;

namespace EventChannels
{
    public struct GroundSlamEvent: INetworkSerializable
    {
       
        public float Force;
        public ulong HitPlayerClientId;
        public Vector3 Origin;
        
        public GroundSlamEvent(ulong hitPlayerClientId,Vector3 origin, float force)
        {
            HitPlayerClientId = hitPlayerClientId;
            Force = force;
            Origin = origin;
        }



        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Origin);
            serializer.SerializeValue(ref Force);
            serializer.SerializeValue(ref HitPlayerClientId);
        }
    }
}

