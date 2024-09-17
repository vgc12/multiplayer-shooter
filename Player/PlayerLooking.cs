using System.Collections;
using System.Collections.Generic;
using EventChannels;
using Unity.Netcode;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Player
{
    public class PlayerLooking : NetworkBehaviour
    {
    
        //[SerializeField] private GenericEventChannelScriptableObject<SettingsChangedEvent> settingsChangedEventChannel;
    
        private PlayerInputActions _playerInputActions;
        
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Camera gunCamera;

        [SerializeField] 
        private Transform orientation;


        public float sensX = 100f;
        public float sensY = 100f;

        private float _xRotation;
        private float _yRotation;
    
        [SerializeField] private float normalFov = 50f;

        public bool allowMovement = true;
        
        [SerializeField] private Transform modelTransform;
        [SerializeField] private Transform cameraAnchor;


        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                playerCamera.enabled = false;
                GetComponentInChildren<AudioListener>().enabled = false;
                enabled=false;
            }
        }
        
        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Enable();
            playerCamera.fieldOfView = normalFov;
            //settingsChangedEventChannel.OnEventRaised += OnSettingsChanged;
        
        }

        /*
        private void OnSettingsChanged(SettingsChangedEvent settingsChangedEvent)
        {
            SetSensitivity(settingsChangedEvent.Settings.MouseSensitivity);
        }
        */


        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    
        public void SetSensitivity(float sensitivity)
        {
            sensX = sensitivity;
            sensY = sensitivity;
        }

        // Update is called once per frame
        private void Update()
        {
            if (allowMovement)
                MoveCamera();


        }

        private void MoveCamera()
        {
            transform.position = cameraAnchor.position;
            Vector2 mouseDelta = _playerInputActions.Player.Look.ReadValue<Vector2>();

            float mouseX = mouseDelta.x * Time.deltaTime * sensX;
            float mouseY = mouseDelta.y * Time.deltaTime * sensY;

            _yRotation += mouseX;
            _xRotation -= mouseY;

            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            modelTransform.rotation = orientation.rotation;
        }

   
   

        public IEnumerator LerpFOV(bool toggle, float wallRunFov)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2;
                playerCamera.fieldOfView = Mathf.Lerp(toggle? normalFov : wallRunFov, toggle? wallRunFov: normalFov , t);
                gunCamera.fieldOfView = playerCamera.fieldOfView;
                yield return null;
            }
        }

        
        public IEnumerator LerpRotation(Transform tr, float from, float to)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 9f;
                //playerCamera.transform.localRotation = Quaternion.Euler(playerCamera.transform.rotation.x, playerCamera.transform.rotation.y, Mathf.Lerp(from, to, t));
                //modelTransform.localRotation = Quaternion.Euler(modelTransform.rotation.x, modelTransform.rotation.y, Mathf.Lerp(from, to, t));
                tr.localRotation = Quaternion.Euler(tr.localRotation.x, tr.localRotation.y, Mathf.Lerp(from, to, t));
                //playerCamera.transform.rotation = modelTransform.rotation;
                yield return null;
            }
        }

        private void OnDisable()
        {
       
            _playerInputActions.Player.Disable();
        }
    }
}