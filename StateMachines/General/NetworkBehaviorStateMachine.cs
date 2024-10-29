using System;
using System.Collections.Generic;
using StateMachines.General;
using Unity.Netcode;
using UnityEngine;

namespace StateMachines
{
    public abstract class NetworkBehaviorStateMachine<T> : NetworkBehaviour, IBaseStateMachine<T> where T : Enum
    {
        
     

        public BaseState<T> CurrentState { get; set; }
        public Dictionary<T, BaseState<T>> States { get; set; }

        
        public override void OnNetworkSpawn()
        { 
          
            if (!IsOwner)
            {
                enabled = false;
            }
         
        }

        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }
            CurrentState.EnterState();
        }

        protected virtual void Update()
        {
            if (!IsOwner)
            {
                return;
            }
            CurrentState.UpdateStates();  
          
            
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
            {
                return;
            }
            CurrentState.FixedUpdateStates();
        }


        private void OnTriggerEnter(Collider other)
        {
            CurrentState.OnTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CurrentState.OnTriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            CurrentState.OnTriggerExit(other);
        }
        
        
    }
}
