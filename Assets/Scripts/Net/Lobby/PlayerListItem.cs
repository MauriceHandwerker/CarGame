using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private Toggle _playerReady;

    public void SetUp(string name, bool isReady)
    {
        this._playerName.text = name;
        this._playerReady.isOn = isReady;
    }

    public bool isReady()
    {
        return this._playerReady.isOn;
    }
}