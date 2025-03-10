using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("MainMenu")]
    [SerializeField] private Button _mainMenuJoinButton;
    [SerializeField] private Button _mainMenuCreateButton;
    [SerializeField] private Button _mainMenuListButton;

    [Header("Create Lobby")]
    [SerializeField] private GameObject _createLobbyMenu;
    [SerializeField] private TMP_InputField _lobbyNameInput;
    [SerializeField] private Toggle _isPrivateToggle;
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _closeCreateLobbyMenu;

    [Header("Join Lobby")]
    [SerializeField] private GameObject _joinLobbyMenu;
    [SerializeField] private TMP_InputField _joinLobbyCodeInput;
    [SerializeField] private Button _joinLobbyButton;
    [SerializeField] private Button _closeJoinMenuButton;

    [Header("List Lobbys")]
    [SerializeField] private GameObject _lobbyListMenu;
    [SerializeField] private Transform _lobbyListContainer;
    [SerializeField] private GameObject _lobbyListItem;
    [SerializeField] private Button _closeLobbyListMenu;

    [Header("Menus")]
    [SerializeField] private GameObject _lobbyMenu;
    [SerializeField] private GameObject _mainMenu;

    [Header("MainMenu Buttons")]
    [SerializeField] private Button _lobbyListButton;
    [SerializeField] private Button _leaveLobbyButton;

    // Static LobbyManager Data
    public static Lobby CurrentLobby;

    private QueryLobbiesOptions _queryLobbiesOptions;

    private async void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        QueryFilter notLockedFilter = new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ);
        this._queryLobbiesOptions = new QueryLobbiesOptions();
        this._queryLobbiesOptions.Filters = new List<QueryFilter> { notLockedFilter };

        // MainMenu
        this._mainMenuJoinButton.onClick.AddListener(() => OpenMenu(ref this._joinLobbyMenu));
        this._mainMenuCreateButton.onClick.AddListener(() => OpenMenu(ref this._createLobbyMenu));
        this._mainMenuListButton.onClick.AddListener(() => OpenLobbyListMenu());

        // Close Menus
        this._closeCreateLobbyMenu.onClick.AddListener(() => CloseMenu(ref this._createLobbyMenu));
        this._closeJoinMenuButton.onClick.AddListener(() => CloseMenu(ref this._joinLobbyMenu));
        this._closeLobbyListMenu.onClick.AddListener(() => CloseMenu(ref this._lobbyListMenu));

        // Buttons
        this._createLobbyButton.onClick.AddListener(() => CreateLobby(this._lobbyNameInput.text, this._isPrivateToggle.isOn));
        this._joinLobbyButton.onClick.AddListener(() => JoinLobby(this._joinLobbyCodeInput.text));
        this._leaveLobbyButton.onClick.AddListener(() => LeaveLobby());
    }

    private async void CreateLobby(string name, bool isPrivate)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Player = CreatePlayerData();

            DataObject dataObjectRelayCode = new DataObject(DataObject.VisibilityOptions.Member, "0");
            options.Data = new Dictionary<string, DataObject> { { "RelayCode", dataObjectRelayCode } };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(name, 6, options);
            this._lobbyListMenu.SetActive(false);
            this._createLobbyMenu.SetActive(false);
            this._lobbyMenu.SetActive(true);
        }
        catch (LobbyServiceException err)
        {
            Debug.LogWarning(err);
        }
    }

    private async void JoinLobby(string lobbyId)
    {
        try
        {
            CurrentLobby  = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, new JoinLobbyByIdOptions { Player = CreatePlayerData() });
            this._lobbyListMenu.SetActive(false);
            this._lobbyMenu.SetActive(true);
        }
        catch (LobbyServiceException err)
        {
            Debug.LogWarning(err);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            CurrentLobby = null;

            this._lobbyMenu.SetActive(false);
            this._mainMenu.SetActive(true);
        }
        catch(LobbyServiceException err)
        {
            Debug.LogWarning(err);
        }
    }

    private void OpenLobbyListMenu()
    {
        OpenMenu(ref this._lobbyListMenu);
        ListLobbys();
    }

    private void OpenMenu(ref GameObject menu)
    {
        if (menu == this._mainMenu) return;

        this._mainMenu.SetActive(false);
        menu.SetActive(true);
    }

    private void CloseMenu(ref GameObject menu)
    {
        if (menu == this._mainMenu) return;

        menu.SetActive(false);
        this._mainMenu.SetActive(true);
    }

    private async void ListLobbys()
    {
        while (this._lobbyListMenu.activeSelf && Application.isPlaying)
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(this._queryLobbiesOptions);

            if (this._lobbyListContainer.childCount > 0)
            {
                foreach (Transform t in this._lobbyListContainer)
                {
                    Destroy(t.gameObject);
                }
            }

            foreach (Lobby lobby in queryResponse.Results)
            {
                GameObject newItem = Instantiate(this._lobbyListItem, this._lobbyListContainer);
                newItem.GetComponent<LobbyListItem>().SetUp(lobby.Name, lobby.Players.Count);
                newItem.GetComponent<Button>().onClick.AddListener(() => JoinLobby(lobby.Id));
            }

            await Task.Delay(1000);
        }
    }

    
    private Player CreatePlayerData()
    {
        PlayerDataObject playerDataObjectName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerPrefs.GetString(UsernameManager.USERNAME_PREFS));
        PlayerDataObject playerDataObjectReady = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false");
        PlayerDataObject playerDataObjectOnline = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false");

        return new Player(
            id: AuthenticationService.Instance.PlayerId, 
            data: new Dictionary<string, PlayerDataObject> { 
                { "Name", playerDataObjectName }, 
                { "Ready", playerDataObjectReady },
                { "Online", playerDataObjectOnline }
            });
    }
}