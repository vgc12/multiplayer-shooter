using System;
using StateMachines.PlayerLooking;
using StateMachines.PlayerMovement;
using UnityEngine;

namespace StateMachines.General
{
    public abstract class BaseState<T> where T : Enum
    {
        protected BaseState(IBaseStateMachine<T> stateMachine, T stateKey)
        {
            _stateMachine = stateMachine;
            _stateKey = stateKey;
            
        }

        private bool _updateUsed;

        protected bool IsRootState = false;
        
        private readonly IBaseStateMachine<T> _stateMachine;

        private readonly T _stateKey;
        
        private BaseState<T> CurrentSubState { get; set; }
        private BaseState<T> CurrentSuperState { get; set; }
        
        public abstract void EnterState();

        public void UpdateStates()
        {
            UpdateState();
          
      

            CurrentSubState?.UpdateStates();

        }
        
        
        public void FixedUpdateStates()
        {
            FixedUpdateState();
            CurrentSubState?.FixedUpdateStates();
        }

        protected virtual void FixedUpdateState()
        {
           
        }

        protected virtual void UpdateState()
        {
          
            CheckSwitchState();
            CheckSubStates();
        }
        public abstract void ExitState();


        protected void SwitchState(T newState)
        {

           
            ExitStates();
            var state = _stateMachine.States[newState];
            if(_stateMachine.CurrentState == state)
                return;
            if (IsRootState)
            {
                state.EnterState();
                _stateMachine.CurrentState = state;
            }
            else
            {
                CurrentSuperState?.SetSubState(newState);
            }
        }

        protected abstract void CheckSubStates();

        protected abstract void CheckSwitchState();
        
        protected void SetSubState( T newState)
        {
            var state = _stateMachine.States[newState];
            if(CurrentSubState != null && CurrentSubState == state)
                return;

            CurrentSubState?.ExitStates();
            CurrentSubState = state;
            CurrentSubState.SetSuperState(_stateKey);
            CurrentSubState.EnterState();
        }

        private void SetSuperState(T newState)
        {
            CurrentSuperState = _stateMachine.States[newState];
            
        }

        protected virtual void ExitStates()
        {
            ExitState();
            CurrentSubState?.ExitStates();
        }
        
        
        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        public abstract void OnTriggerStay(Collider other);
        
        
    }
}
