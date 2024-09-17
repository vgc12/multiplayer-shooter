using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventChannels
{
    public abstract class AbstractEventChannelListener<TEventChannel, TEventType> : MonoBehaviour
        where TEventChannel : GenericEventChannelScriptableObject<TEventType> 
    {
        [SerializeField] protected TEventChannel eventChannel;
        [SerializeField] protected UnityEvent<TEventType> response;
        protected virtual void OnEnable()
        {
            if(eventChannel == null) return;
            eventChannel.OnEventRaised += OnEventRaised;
        }

        protected void OnDisable()
        {
            if(eventChannel == null) return;
            eventChannel.OnEventRaised -= OnEventRaised;
        }

        private void OnEventRaised(TEventType evt)
        {
            response.Invoke(evt);
        }
    }
}
