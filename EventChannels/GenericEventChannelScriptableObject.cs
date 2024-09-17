using UnityEngine;
using UnityEngine.Events;

namespace EventChannels
{
   public abstract class GenericEventChannelScriptableObject<T> : ScriptableObject
   {
      public UnityAction<T> OnEventRaised;
   
      public void RaiseEvent(T param) => OnEventRaised?.Invoke(param); 
   }
}
