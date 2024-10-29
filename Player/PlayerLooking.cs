using System;
using System.Collections;
using System.Linq;
using EventChannels;
using UI;
using Unity.Netcode;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

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


        
        [SerializeField] private Transform modelTransform;
        [SerializeField] private Transform headModelTransform;

        private GenericEventChannelScriptableObject<PlayerDeathEvent> _playerDeathEventChannel;
        private GenericEventChannelScriptableObject<PlayerSpawnEvent> _playerSpawnEventChannel;
     
        private Transform _killerTransform;

      

        [SerializeField] private Transform thirdPersonRotationObject;
        [SerializeField] private Transform thirdPersonPositionTransform;
        [SerializeField]
        private Transform cameraPosition;
        private Vector3 _playerCameraPosition;
        private Transform _rotationTarget;

        [SerializeField] private PlayerGamePlayInfoScriptableObject infoScriptableObject;
        private GenericEventChannelScriptableObject<PauseEvent> _pauseEventChannel;
        
        private bool _isDead;
        [SerializeField] private GameObject falsePlayerPrefab;

        public Vector2 MouseDelta { get; private set; }

        [SerializeField]private Transform initialHeadPosition;
        
        private PlayerMovement _playerMovement;
        
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
            _playerDeathEventChannel = EventChannelAccessor.Instance.playerDeathEventChannel;
            _playerSpawnEventChannel = EventChannelAccessor.Instance.playerSpawnEventChannel;

            _playerMovement = transform.root.GetComponent<PlayerMovement>();
            
            _playerInputActions.Player.Enable();
            playerCamera.fieldOfView = normalFov;

            _playerSpawnEventChannel.OnEventRaised += OnSpawn;
            _pauseEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
            _pauseEventChannel.OnEventRaised += OnPause;
            _playerCameraPosition = cameraPosition.position;
            _rotationTarget = transform;

        }

        private void OnPause(PauseEvent pauseEvent)
        {
            if(!IsOwner || pauseEvent.PlayerId != OwnerClientId)
                return;
            enabled = !pauseEvent.IsPaused;
            
        }
        
        private void OnSpawn(PlayerSpawnEvent playerSpawnEvent)
        {
            if (playerSpawnEvent.RespawningClientId != OwnerClientId) return;
            
            _isDead = false;
            playerCamera.fieldOfView = normalFov;
            gunCamera.fieldOfView = normalFov;
         
            playerCamera.transform.localRotation = Quaternion.Euler(playerCamera.transform.localRotation.x, playerCamera.transform.localRotation.y, 0);
            playerCamera.cullingMask = infoScriptableObject.defaultCameraMask;
            thirdPersonRotationObject.position = transform.position;
          
            _rotationTarget = transform;
            transform.localPosition = _playerCameraPosition;
   
        }

 
        
        public float lerpSpeed = 1f;
        public Vector3 x;
        private IEnumerator LerpPosition(Transform tr,Vector3 from, Vector3 to)
        {

            /*

            while (Physics.Raycast(_rotationTarget.position,
                       to - _rotationTarget.position, out var hit,
                       Vector3.Distance(_rotationTarget.position, to)))
                {
                    
                    to = Random.insideUnitSphere + _rotationTarget.position;
                    
                    to.z = 1.41f;
                }
              */ 
          
            float t = 0;
            while (t < 1)
            {
                transform.LookAt(_rotationTarget);
                t += Time.deltaTime *lerpSpeed;
                tr.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

        }
        


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

       
        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            thirdPersonRotationObject.position = transform.position;
            thirdPersonRotationObject.rotation = transform.rotation;
            

            MoveCamera();
        }

        private void MoveCamera()
        {

            
            
            if (_playerMovement.Sliding)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, headModelTransform.position.y, Time.deltaTime * 10f), transform.position.z);
            }
            else
            {
                transform.position = initialHeadPosition.position;
            }
            
            MouseDelta = _playerInputActions.Player.Look.ReadValue<Vector2>();
          
            
            float mouseX = MouseDelta.x * Time.deltaTime * sensX;
            float mouseY = MouseDelta.y * Time.deltaTime * sensY;

            _yRotation += mouseX;
            _xRotation -= mouseY;

            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            _rotationTarget.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            modelTransform.rotation = orientation.rotation;
        }

   
   


        
        public IEnumerator LerpRotation(Transform tr, float from, float to)
        {
            float t = 0;
            while (t < 1 && !_isDead)
            {
                t += Time.deltaTime * 9f;
                tr.localRotation = Quaternion.Euler(tr.localRotation.x, tr.localRotation.y, Mathf.Lerp(from, to, t));
                yield return null;
            }
        }

        private void OnDisable()
        {
       
            _playerInputActions.Player.Disable();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }
    }
}