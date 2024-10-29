using System;
using System.Collections.Generic;
using StateMachines.General;
using UnityEngine;

namespace StateMachines
{
    public abstract class MonoBehaviorStateMachine<T> : MonoBehaviour, IBaseStateMachine<T> where T : Enum
    {
        
       
        public BaseState<T> CurrentState { get; set; }
        public Dictionary<T, BaseState<T>> States { get; set; }


        private void Start()
        {
            
            CurrentState.EnterState();
        }

        private void Update()
        {
           
          
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
