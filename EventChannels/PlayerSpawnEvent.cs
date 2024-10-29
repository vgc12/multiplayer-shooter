namespace EventChannels
{
    public struct PlayerSpawnEvent
    {
        public readonly ulong RespawningClientId;
        
        public PlayerSpawnEvent(ulong respawningClientId)
        {
            RespawningClientId = respawningClientId;
        }
    }
}