using EventChannels;
using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Interact Event Channel", menuName = "Events/Interact Event Channel")]
    public class PlayerHitEventChannel : GenericEventChannelScriptableObject<PlayerHitEvent>
    {
      
    }
}