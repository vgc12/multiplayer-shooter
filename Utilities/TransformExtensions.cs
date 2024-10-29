using System.Collections;
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
        
        public static IEnumerator Shake(this Transform transform, float duration, AnimationCurve curve)
        {
            Vector3 startPosition = transform.localPosition;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float strength = curve.Evaluate(t / duration);
                transform.localPosition = startPosition + Random.insideUnitSphere * strength;
                yield return null;
            }
            transform.localPosition = startPosition;
        }
    }
}
