using Player;
using UnityEngine;

namespace EventChannels
{
    [CreateAssetMenu(fileName = "Player Death Event Channel", menuName = "Events/Player Death Event Channel")]
    public class PlayerDeathEventChannel: GenericEventChannelScriptableObject<PlayerDeathEvent>
    {
      
    }
}