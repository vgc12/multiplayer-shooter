using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventChannels;
using NetworkScripts;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Utilities;
using Cursor = UnityEngine.Cursor;

namespace UI
{
    public class PauseUI
    {
        public Action ButtonResumeClicked { set => _buttonResume.clicked += value; }
        public Action ButtonMainMenuClicked { set => _buttonMenu.clicked += value; }
        public Action ButtonDesktopClicked { set => _buttonDesktop.clicked += value; }
        
        public bool MenuOpened { get => _menuOpened;
            set
            {
                _menuOpened = value;
                if (_menuOpened)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                _gamePausedEventChannel.RaiseEvent(new PauseEvent(_menuOpened, NetworkManager.Singleton.LocalClientId));
           
            }
        }
        private bool _menuOpened;
        
        private readonly Button _buttonResume;
        
        private readonly Button _buttonMenu;
        
        private readonly Button _buttonDesktop;

       
        
        private readonly GenericEventChannelScriptableObject<PauseEvent> _gamePausedEventChannel;
    
        
        public PauseUI(VisualElement root,ref VisualElement queuedElement, ref VisualElement currentElement)
        {
         
            _gamePausedEventChannel = EventChannelAccessor.Instance.gamePausedEventChannel;
         
            
            _buttonMenu = root.Q<Button>("ButtonQuitToMenu");
            ButtonMainMenuClicked = MatchMaking.Instance.Disconnect;
            ButtonMainMenuClicked = () => NetworkManager.Singleton.Shutdown();
            ButtonMainMenuClicked =()=> SceneManager.LoadScene("MainMenu");
            
            
            _buttonDesktop = root.Q<Button>("ButtonQuitToDesktop");
            ButtonDesktopClicked = MatchMaking.Instance.Disconnect;
            ButtonDesktopClicked = Application.Quit;
            
            
            _buttonResume = root.Q<Button>("ButtonResume");
           
        }

        
        

       
    }
}