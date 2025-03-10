using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("Player List")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerListItem;

    [Header("Buttons")]
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _readyButtonText;
    [SerializeField] private Button _startButton;

    [Space(30)]
    [SerializeField] private RelayManager _relayManager;
    [SerializeField] private string _gameSceneName;

    private Player _localPlayer;
    private bool _isLocalReady;
    private bool _lobbyActive = false;
    private bool _isOnline = false;
    private bool _isConnecting = false;


#if UNITY_EDITOR
    [SerializeField] private SceneAsset _gameScene;

    private void OnValidate()
    {
        this._gameSceneName = this._gameScene.name;
    }
#endif

    private void OnEnable()
    {
        this._isLocalReady = true;
        ToggleReady();
        this._lobbyActive = true;

        if (LobbyManager.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            this._startButton.gameObject.SetActive(true);
        }

        this._readyButton.onClick.AddListener(() => ToggleReady());
        this._startButton.onClick.AddListener(() => StartRelay());

        LobbyPings();
        UpdateLobby();
    }

    private void ToggleReady()
    {
        this._isLocalReady = !this._isLocalReady;

        if (this._isLocalReady)
        {
            this._readyButtonText.text = "Ready!";
        } else
        {
            this._readyButtonText.text = "Not Ready!";
        }
    }

    private async void StartRelay()
    {
        if (LobbyManager.CurrentLobby.HostId != AuthenticationService.Instance.PlayerId) return;

        string joinCode = await this._relayManager.CreateRelayConnection();

        DataObject dataObjectRelayCode = new DataObject(DataObject.VisibilityOptions.Member, joinCode);
        UpdateLobbyOptions newLobbyOptions = new UpdateLobbyOptions();
        newLobbyOptions.IsLocked = true;
        newLobbyOptions.Data = new Dictionary<string, DataObject> { { "RelayCode", dataObjectRelayCode } };
        LobbyManager.CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.CurrentLobby.Id, newLobbyOptions);
    }

    private async void UpdateLobby()
    {
        while(LobbyManager.CurrentLobby != null)
        {
            if (LobbyManager.CurrentLobby.Data["RelayCode"].Value != "0" && this._isConnecting == false)
            {
                this._isConnecting = true;
                Debug.Log("Joining Relay");
                await this._relayManager.JoinRelayConnection(LobbyManager.CurrentLobby.Data["RelayCode"].Value);
            }

            if (IsEveryoneOnline())
            {
                NetworkManager.Singleton.SceneManager.LoadScene(this._gameSceneName, LoadSceneMode.Single);
            }

            LobbyManager.CurrentLobby = await LobbyService.Instance.GetLobbyAsync(LobbyManager.CurrentLobby.Id);
            UpdateReadyValue();
            UpdatePlayerList();
            UpdateOnlineValue();

            if (IsEveryoneReady() && LobbyManager.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                this._startButton.interactable = true;
            } else
            {
                this._startButton.interactable = false;
            }

            await Task.Delay(1000);
        }
    }

    private async void UpdateReadyValue()
    {
        if (this._localPlayer == null) return;

        this._localPlayer.Data["Ready"].Value = BetterBool.BoolToString(this._isLocalReady);
        UpdatePlayerOptions newPlayerOptions = new UpdatePlayerOptions();
        newPlayerOptions.Data = this._localPlayer.Data;

        await LobbyService.Instance.UpdatePlayerAsync(LobbyManager.CurrentLobby.Id, this._localPlayer.Id, newPlayerOptions);
    }

    private async void UpdateOnlineValue()
    {
        if (this._localPlayer == null) return;
        if (this._isOnline) return;
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        this._isOnline = true;
        this._localPlayer.Data["Online"].Value = BetterBool.BoolToString(this._isOnline);
        UpdatePlayerOptions newPlayerOptions = new UpdatePlayerOptions();
        newPlayerOptions.Data = this._localPlayer.Data;

        await LobbyService.Instance.UpdatePlayerAsync(LobbyManager.CurrentLobby.Id, this._localPlayer.Id, newPlayerOptions);
    }

    private void UpdatePlayerList()
    {

        if (this._playerListContainer.childCount > 0)
        {
            foreach (Transform t in this._playerListContainer)
            {
                Destroy(t.gameObject);
            }
        }

        foreach (Player player in LobbyManager.CurrentLobby.Players)
        {
            GameObject newPlayerItem = Instantiate(this._playerListItem, this._playerListContainer);
            newPlayerItem.GetComponent<PlayerListItem>().SetUp(player.Data["Name"].Value, BetterBool.StringToBool(player.Data["Ready"].Value));

            if (player.Id == AuthenticationService.Instance.PlayerId) this._localPlayer = player;
        }
    }

    private bool IsEveryoneOnline()
    {
        if (!NetworkManager.Singleton.IsHost) return false;

        foreach (Player player in LobbyManager.CurrentLobby.Players)
        {

            if (player.Id != LobbyManager.CurrentLobby.HostId)
            {
                if (!BetterBool.StringToBool(player.Data["Online"].Value)) return false;
            }
        }

        return true;
    }

    private bool IsEveryoneReady()
    {
        foreach(Transform t in this._playerListContainer)
        {
            if (t.GetComponent<PlayerListItem>().isReady() == false) return false; 
        }

        return true;
    }

    private async void LobbyPings()
    {
        while(this._lobbyActive && LobbyManager.CurrentLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(LobbyManager.CurrentLobby.Id);
            await Task.Delay(15 * 1000); // 15sec
        }
    }
}