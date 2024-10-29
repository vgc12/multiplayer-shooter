using System;
using System.Collections.Generic;
using StateMachines.General;
using UnityEngine;

namespace StateMachines
{
    public interface IBaseStateMachine<T> where T : Enum
    {
        public BaseState<T> CurrentState { get; set;}

        public Dictionary<T, BaseState<T>> States { get; set; }



    }
}