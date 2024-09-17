using UnityEngine;

namespace Player
{
   public class SnapCamera : MonoBehaviour
   {
      [SerializeField] private Transform cameraPosition;
      private void Update()
      {
         transform.position = cameraPosition.position;
      }
   }
}
