using UnityEngine;

namespace Utilities
{
    public static class TransformExtensions 
    {
        public static void SyncTransform(this Transform transform, Transform targetTransform)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
        
        
        public static void ChangeLayerRecursively(this Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                child.ChangeLayerRecursively(layer);
            }
        }
    }
}
