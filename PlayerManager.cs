using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Weapons.Guns.Ammo;
using Random = UnityEngine.Random;

public class PlayerManager : NetworkBehaviour
{
    
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> gunPrefabs;
    public static PlayerManager Instance;
    public Transform[] respawnPoints;
    public List<NetworkObject> players = new();

    public override void OnNetworkSpawn()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong obj)
    {
        if (IsServer)
        {
            RemovePlayer(obj);
        }
    }

    private void OnLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
    {
        Debug.LogWarning(clientid + " | " + scenename + " | " + loadscenemode + " | " + IsServer);
   
        
        if (IsServer)
        { 
            Debug.LogError(clientid);
          var respawnPoint = respawnPoints[Random.Range(0, respawnPoints.Length)];
          Debug.LogWarning(respawnPoint.position);
            var p =NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab.GetComponent<NetworkObject>(),
                clientid, true, true, false, respawnPoint.position, respawnPoint.rotation);
                    
           
            p.name = "Player " + clientid;
            players.Add(p);
            AmmoManager.Instance.SyncAmmoPickups(clientid);
            SyncPlayerListClientRpc();
        }
    }
    
    [ClientRpc]
    private void SyncPlayerListClientRpc()
    {
        players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).Select(p => p.NetworkObject).ToList();
        foreach (var player in players)
        {
            //Debug.Log(player.name);
        }
    }
    

  
    public void RemovePlayer(ulong clientId)
    {
        var player = players.Find(p => p.OwnerClientId == clientId);
        players.Remove(player);
        player.GetComponent<NetworkObject>().Despawn(true);
    }

  
    
}














