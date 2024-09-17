using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkScripts
{
    public class MatchMaking : MonoBehaviour
    {
        public static MatchMaking Instance;
    
        private UnityTransport _transport;
        private string _playerId;
        private Lobby _connectedLobby;
    
        public const string LobbyCode = "j";

        public async void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                await Authenticate();
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            }
            else
            {
                Destroy(this);
            }
        
        
        
        }

 


        private async Task Authenticate()
        {
            var options = new InitializationOptions();
           
            
#if UNITY_EDITOR
            options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif


        
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerId = AuthenticationService.Instance.PlayerId;
            Debug.LogError(_playerId);
        }

        public async void CreateOrJoinLobby()
        {
        
        
            _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
        }

        private async Task<Lobby> QuickJoinLobby()
        {
            try
            {
            
                var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

                var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[LobbyCode].Value);
            
                JoinAsClient(allocation);
            
                return lobby;   
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.Log("unable to join lobby");
                return null;
            }

        }
    
        public async Task JoinLobbyAsync(Lobby lobby)
        {
            try
            {
           
                var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[LobbyCode].Value);
            
                JoinAsClient(allocation);
            
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("unable to join lobby");
         
            }
        }
    
  

    
    

        public async Task<Lobby> CreateLobby()
        {
            try
            {
                const int maxPlayers = 4;

                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var options = new CreateLobbyOptions()
                {
                    Data = new Dictionary<string, DataObject>
                        { { LobbyCode, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } },
                };
           
                var lobby = await Lobbies.Instance.CreateLobbyAsync(joinCode, maxPlayers, options);
           
                StartCoroutine(PingLobby(lobby.Id, 15));
           
                JoinAsHost(allocation);
                return lobby;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("unable to create lobby");
                return null;
            }

        }



        private static IEnumerator PingLobby(string lobbyID, float seconds)
        {
            var delay = new WaitForSeconds(seconds);
            while (true)
            {
                try
                {
                    Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                yield return delay;
            }

        }
    
        public async Task<List<Lobby>> GetAvailableLobbies() {
            var options = new QueryLobbiesOptions {
                Count = 15,

                Filters = new List<QueryFilter> {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ)
                }
            };

            var allLobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            return allLobbies.Results;
        }


        private void JoinAsClient(JoinAllocation allocation)
        {
            var relayServerData = new RelayServerData(allocation, "dtls");
            _transport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }

        private void JoinAsHost(Allocation allocation)
        {
            var relayServerData = new RelayServerData(allocation, "dtls");
            _transport.SetRelayServerData(relayServerData);


            bool x = NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Game1", LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            try
            {
                StopAllCoroutines();
                if (_connectedLobby == null) return;
            
                if (_connectedLobby.HostId == _playerId)
                {
                    Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);

                }
                else
                {
                    Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
                }
            }catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
