using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    private UnityTransport _unityTransport ;
    private void Start()
    {
  
       _unityTransport  = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    
    
    public async Task<string> CreateRelay()
    {
        try
        {
          var allocation =  await RelayService.Instance.CreateAllocationAsync(4);
          var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
          Debug.Log("Join code: " + joinCode);
          
          RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
          _unityTransport.SetRelayServerData(relayServerData);
          NetworkManager.Singleton.StartHost();
          return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }

        return "";
    }
    
    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with code: " + joinCode);
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            var relayServerData = new RelayServerData(allocation, "dtls");
            _unityTransport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            
            Debug.Log("Joined relay: " + allocation.AllocationId);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
