using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Utilities
{
    public static class NetworkExtensions 
    {
        
        public static IEnumerable<T> GetClientOwnedObjectsOfType<T>(this NetworkSpawnManager manager, ulong clientId)
        {
            
            foreach (var c in manager.GetClientOwnedObjects(clientId))
            {
                if (c.TryGetComponent<T>(out var component)) 
                    yield return component;
            }
        }
    
    
    }
}