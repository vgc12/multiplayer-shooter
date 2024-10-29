namespace UI
{
    public struct PauseEvent
    {
        public readonly bool IsPaused;
        public readonly ulong PlayerId;
        
        public PauseEvent(bool isPaused, ulong playerId)
        {
            IsPaused = isPaused;
            PlayerId = playerId;
        }
    }
}