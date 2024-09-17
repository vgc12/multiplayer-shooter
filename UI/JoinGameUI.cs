using System;
using UnityEngine.UIElements;
using Utilities;

namespace UI
{
    public class JoinGameUI
    {
        public Action ButtonJoinClicked { set => _buttonJoin.clicked += value; }
        
            
        private readonly Button _buttonJoin;
        
        public JoinGameUI(VisualElement root)
        {
            _buttonJoin = root.Q<Button>("ButtonJoin");
        }
        
    }
}