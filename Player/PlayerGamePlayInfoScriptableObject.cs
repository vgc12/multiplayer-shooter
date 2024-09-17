using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Player Game Play Info", menuName = "Player/Player Game Play Info")]
    public class PlayerGamePlayInfoScriptableObject : ScriptableObject
    {
        public LayerMask groundLayerMask;
        public LayerMask wallLayerMask;
        [Tooltip("Layers that will be ignored when checking if there is an object above the player while uncrouching")]
        public LayerMask crouchIgnoreMask;
    }
}
