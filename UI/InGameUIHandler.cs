using EventChannels;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utilities;
using Cursor = UnityEngine.Cursor;

namespace UI
{
    public class InGameUIHandler : MonoBehaviour
    {
   
        private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset playingScreenAsset;

        private VisualElement _root;
        private VisualElement _playingElement;
        private VisualElement _deathElement;
        private VisualElement _pauseElement;
        private VisualElement _currentElement;
        private VisualElement _queuedElement;

        private PauseUI _pauseUI;
        
        
        
        private GenericEventChannelScriptableObject<PlayerDeathEvent> _playerDeathEventChannel;
        private GenericEventChannelScriptableObject<PlayerSpawnEvent> _playerSpawnEventChannel;
        private PlayerInputActions _playerInputActions;
        
        
        
        private void OnEnable()
        {
          
            _uiDocument = GetComponent<UIDocument>();
            _uiDocument.visualTreeAsset = playingScreenAsset;
            _root = _uiDocument.rootVisualElement;
            _playerInputActions = new PlayerInputActions();
            
            InitializeUIScreens();

            _queuedElement = _playingElement;
            _playerSpawnEventChannel = EventChannelAccessor.Instance.playerSpawnEventChannel;
            _playerDeathEventChannel = EventChannelAccessor.Instance.playerDeathEventChannel;
            _playerDeathEventChannel.OnEventRaised += OnPlayerDeath;
            _playerSpawnEventChannel.OnEventRaised += OnPlayerSpawn;
            
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            SetUpPauseMenu();
            
            
            
            UIExtensions.ChangeMenu(_playingElement, ref _currentElement);

        }

        private void SetUpPauseMenu()
        {
            _pauseUI = new PauseUI(_root, ref _queuedElement, ref _pauseElement)
            {
                ButtonResumeClicked = TogglePauseMenu
            };
            _playerInputActions.UI.Pause.started += _ => TogglePauseMenu();

        }

        private void TogglePauseMenu()
        {
            Cursor.visible = _pauseUI.MenuOpened;
            _pauseUI.MenuOpened = !_pauseUI.MenuOpened;
            _pauseElement.Display(_pauseUI.MenuOpened);
          
        }

        

        private void OnPlayerDeath(PlayerDeathEvent playerDeathEvent)
        {
          if(playerDeathEvent.KilledNetworkObject.NetworkObjectId == NetworkManager.Singleton.LocalClientId )
          {
              
              UIExtensions.ChangeMenu(_deathElement, ref _currentElement);
          }
        }
        private void OnPlayerSpawn(PlayerSpawnEvent playerSpawnEvent)
        {
            if(playerSpawnEvent.RespawningClientId == NetworkManager.Singleton.LocalClientId)
            {
                UIExtensions.ChangeMenu(_playingElement, ref _currentElement);
            }
        }


        private void InitializeUIScreens()
        {
            _playingElement = _root.Q<VisualElement>("PlayingUI");
            _deathElement = _root.Q<VisualElement>("DeathUI");
            _pauseElement = _root.Q<VisualElement>("PauseUI");
        }




    
        
      
        
   


        /*

   
        private void ToggleDepthOfField(bool active)
        {
            if(volume.profile.TryGet(out DepthOfField depthOfField))
            {
                depthOfField.mode.value = active ? DepthOfFieldMode.Bokeh : DepthOfFieldMode.Off;
            
            }
        } 
    */
    }
}
