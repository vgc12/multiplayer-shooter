using System;
using EventChannels;
using Player;
using UI;
using UnityEngine;
using Weapons.Guns.Ammo;

namespace Utilities
{
    public class EventChannelAccessor : MonoBehaviour
    {
        public static EventChannelAccessor Instance { get; private set; }
        public GenericEventChannelScriptableObject<PlayerDeathEvent> playerDeathEventChannel;
        public GenericEventChannelScriptableObject<PlayerHitEvent> playerHitEventChannel;
        public GenericEventChannelScriptableObject<PlayerSpawnEvent> playerSpawnEventChannel;
        public GenericEventChannelScriptableObject<AmmoPickedUpEvent> ammoPickedUpEventChannel;
        public GenericEventChannelScriptableObject<PauseEvent> gamePausedEventChannel;
        public GenericEventChannelScriptableObject<GroundSlamEvent> groundSlamEventChannel;
        public GenericEventChannelScriptableObject<RagdollSpawnEvent> ragdollSpawnEventChannel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

    }
}
