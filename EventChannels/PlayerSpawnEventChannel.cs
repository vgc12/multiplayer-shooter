using Player;
using UnityEngine;

namespace EventChannels
{
    [CreateAssetMenu(fileName = "Player Spawn Event Channel", menuName = "Events/Player Spawn Event Channel")]
    public class PlayerSpawnEventChannel: GenericEventChannelScriptableObject<PlayerSpawnEvent>
    {
      
    }
}