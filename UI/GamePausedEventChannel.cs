using EventChannels;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "Game Paused Event Channel", menuName = "Events/Game Paused Event Channel")]
    public class GamePausedEventChannel : GenericEventChannelScriptableObject<PauseEvent>
    {
        
    }
}