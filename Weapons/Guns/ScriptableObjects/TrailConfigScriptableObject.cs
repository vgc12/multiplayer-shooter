using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Trail Configuration", menuName = "Guns/Trail Configuration", order = 2)]
    public class TrailConfigScriptableObject : ScriptableObject
    {
      
        public float duration = 0.5f;

        [Tooltip("Max Distance before counted as miss")]
        public float missDistance = 100f;

        [Tooltip("How fast the trail will move towards the target position in U/S")]
        public float trailSpeed = 1f;
    }
}
