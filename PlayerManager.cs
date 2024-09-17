using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Weapons.Guns.Ammo;

public class PlayerManager : NetworkBehaviour
{
    
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> gunPrefabs;
    public static PlayerManager Instance;
    public Transform[] respawnPoints;
    
    public override void OnNetworkSpawn()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    private void OnLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
    {
        Debug.LogWarning(clientid + " | " + scenename + " | " + loadscenemode + " | " + IsServer);
        if (IsServer)
        {
            var p =NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab.GetComponent<NetworkObject>(),
                clientid, true, true, false, respawnPoints[Random.Range(0, respawnPoints.Length)].position, Quaternion.identity
                    );
            p.name = "Player " + clientid;
            AmmoManager.Instance.SyncAmmoPickups(clientid);
           
        }
    }



  
}














