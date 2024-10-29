using System;
using NetworkScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using WebSocketSharp;

namespace UI
{
    public class UIHandler : MonoBehaviour
    {
    
    
        private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset playingScreenAsset;
        [SerializeField] private MatchMaking matchMaking;

        private VisualElement _root;
        private VisualElement _rootVisualElement;
        private VisualElement _serverBrowserElement;
        private VisualElement _currentElement;

        private ServerBrowserUI _serverBrowserUI;
        
        private ListView _serverListView;
        
   



    
    

        private void OnEnable()
        {
          
            _uiDocument = GetComponent<UIDocument>();
            _uiDocument.visualTreeAsset = playingScreenAsset;
            _root = _uiDocument.rootVisualElement;
            
            
            InitializeUIScreens();
        
        
            SetupServerBrowserUI();
            _currentElement = _serverBrowserElement;
            
            UIExtensions.ChangeMenu(_serverBrowserElement, ref _currentElement);

        }

        private void SetupServerBrowserUI()
        {

            _serverBrowserUI = new ServerBrowserUI(_serverBrowserElement);

        }


        private void InitializeUIScreens()
        {
            _serverBrowserElement = _root.Q<VisualElement>("ServerBrowserUI");
        }



      

    
        
      
        
        private void JoinGame()
        {


            matchMaking.CreateOrJoinLobby();
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
