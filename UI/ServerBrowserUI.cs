using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetworkScripts;
using Unity.Services.Lobbies.Models;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UI
{
    public class ServerBrowserUI
    {
        public Action ButtonRefreshClicked { set => _buttonRefresh.clicked += value; }
        public Action ButtonJoinClicked { set => _buttonJoin.clicked += value; }
        public Action ButtonCreateClicked { set => _buttonCreate.clicked += value; }
        
        public Action<IEnumerable<object>> ServerListViewItemsChosen { set => _serverListView.itemsChosen += value; }
        public Action<IEnumerable<object>> ServerListVeiwItemSelectionChanged { set => _serverListView.selectionChanged += value; }
        
        
        private readonly ListView _serverListView;
        
        private readonly Button _buttonRefresh;
        
        private readonly Button _buttonJoin;
        
        private readonly Button _buttonCreate;
        
        private Lobby _selectedLobby;
        
        
        
        public ServerBrowserUI(VisualElement root)
        {
            _serverListView = root.Q<ListView>("ListViewServer");
            
            _serverListView.makeItem = () => new Label();
            _serverListView.selectionType = SelectionType.Single;
            

            ServerListViewItemsChosen = async l =>
            {
                Lobby lobby = l.First() as Lobby;
                await MatchMaking.Instance.JoinLobbyAsync(lobby);
            };

            ServerListVeiwItemSelectionChanged = l =>
            {
                _selectedLobby = l.First() as Lobby;

            };
            
            
            _buttonJoin = root.Q<Button>("ButtonJoin");
            ButtonJoinClicked = JoinClicked;
            
            _buttonCreate = root.Q<Button>("ButtonCreate");
            ButtonCreateClicked = CreateClicked;
            
            _buttonRefresh = root.Q<Button>("ButtonRefresh");
            ButtonRefreshClicked = RefreshServerList;
        }

        private async void CreateClicked() => await MatchMaking.Instance.CreateLobby();

        private async void JoinClicked() => await MatchMaking.Instance.JoinLobbyAsync(_selectedLobby);

        private async void RefreshServerList()
        {
            
            
            var lobbies = await MatchMaking.Instance.GetAvailableLobbies();


            _serverListView.bindItem = BindItem;
            
            _serverListView.itemsSource = lobbies;
            
     
            _serverListView.RefreshItems();
           
            void BindItem(VisualElement element, int i)
            {
                ((Label)element).text = lobbies[i].Name;
            }
            
         
        }
    }
}