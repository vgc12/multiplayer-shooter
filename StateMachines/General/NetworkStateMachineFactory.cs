using System;

namespace StateMachines
{
    public class NetworkStateMachineFactory<T> where T : Enum
    {
     
       private NetworkBehaviorStateMachine<T> _stateMachine;
       public NetworkStateMachineFactory(NetworkBehaviorStateMachine<T> stateMachine)  
       {
           _stateMachine = stateMachine;
       }
       
       public void FillStateMachine()
       {
           foreach (var state in (T[])Enum.GetValues(typeof(T)))
           {
              //_stateMachine.States.Add(state, (BaseState<T>)Activator.CreateInstance(typeof(T), state));
           }
       }
        
    }
}