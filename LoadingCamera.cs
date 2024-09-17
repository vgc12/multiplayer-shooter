using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadingCamera : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void Awake()
    {
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong obj)
    {
        enabled = false;
    }


    private void OnDestroy()
    {
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }
}
