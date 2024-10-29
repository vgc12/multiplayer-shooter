using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerNetworkData> _networkData =
        new(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _velocity;
    [SerializeField] private float interpolationTime = 0.1f;

    Rigidbody _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (IsOwner)
        {
            _networkData.Value = new PlayerNetworkData()
            {
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles
            };
        }
        else
        {
            _velocity = _rb.linearVelocity;
            transform.position = Vector3.SmoothDamp(transform.position, _networkData.Value.Position, ref _velocity, interpolationTime);
            transform.rotation = Quaternion.Euler(Mathf.SmoothDampAngle(transform.eulerAngles.x, _networkData.Value.Rotation.x, ref _velocity.x, interpolationTime),
                Mathf.SmoothDampAngle(transform.eulerAngles.y, _networkData.Value.Rotation.y, ref _velocity.y, interpolationTime),
                Mathf.SmoothDampAngle(transform.eulerAngles.z, _networkData.Value.Rotation.z, ref _velocity.z, interpolationTime));
         
        }
    }

    struct PlayerNetworkData : INetworkSerializable
    {
        private float _x, _y, _z;
        private float _xRotation,_yRotation, _zRotation;

        internal Vector3 Position
        {
            get => new Vector3(_x, _y, _z);
            set
            {
                _x = value.x;
                _y = value.y;
                _z = value.z;
            }
        }
        
        internal Vector3 Rotation
        {
            get => new Vector3(_xRotation, _yRotation, _zRotation);
            set
            {
                _xRotation = value.x;
                _yRotation = value.y;
                _zRotation = value.z;
            }
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _z);
            serializer.SerializeValue(ref _xRotation);
            serializer.SerializeValue(ref _yRotation);
            serializer.SerializeValue(ref _zRotation);
        }
    }
}


