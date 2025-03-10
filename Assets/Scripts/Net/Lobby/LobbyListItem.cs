using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyName;
    [SerializeField] private TMP_Text _lobbyPlayerCount;

    public void SetUp(string lobbyName, int playerCount)
    {
        this._lobbyName.text = lobbyName;
        this._lobbyPlayerCount.text = $"{playerCount}/6";
    }
}
