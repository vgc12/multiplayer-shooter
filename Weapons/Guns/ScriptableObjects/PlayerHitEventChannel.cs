using EventChannels;
using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Player Hit Event Channel", menuName = "Events/Player Hit Event Channel")]
    public class PlayerHitEventChannel : GenericEventChannelScriptableObject<PlayerHitEvent>
    {
      
    }
}