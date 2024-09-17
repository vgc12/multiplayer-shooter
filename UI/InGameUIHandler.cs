using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace UI
{
    public class InGameUIHandler : MonoBehaviour
    {
   
        private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset playingScreenAsset;

        private VisualElement _root;
        private VisualElement _playingElement;
        private VisualElement _currentElement;

        private ServerBrowserUI _serverBrowserUI;
        
        private ListView _serverListView;
        
   



    
    

        private void OnEnable()
        {
          
            _uiDocument = GetComponent<UIDocument>();
            _uiDocument.visualTreeAsset = playingScreenAsset;
            _root = _uiDocument.rootVisualElement;
            
            
            InitializeUIScreens();
        
        
            
            
            UIExtensions.ChangeMenu(_playingElement, ref _currentElement);

        }

     


        private void InitializeUIScreens()
        {
            
            _playingElement = _root.Q<VisualElement>("PlayingUI");
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
