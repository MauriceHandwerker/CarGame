using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsernameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private Button _setUsernameButton;

    [SerializeField] private GameObject _menuAfterClose;

    public static string USERNAME_PREFS = "USERNAME";

    private void Awake()
    {
        this._setUsernameButton.onClick.AddListener(() => SetUsername());
    }

    private void OnEnabled()
    {
        if (PlayerPrefs.HasKey(USERNAME_PREFS))
        {
            this._usernameInput.text = PlayerPrefs.GetString(USERNAME_PREFS);
        }

        this._usernameInput.Select();
    }

    private void Update()
    {
        if(this._usernameInput.text.Length > 5 && this._setUsernameButton.interactable == false)
        {
            this._setUsernameButton.interactable = true;
        } else
        {
            this._setUsernameButton.interactable = false;
        }
    }

    private void SetUsername()
    {
        PlayerPrefs.SetString(USERNAME_PREFS, this._usernameInput.text);
        PlayerPrefs.Save();
        this.gameObject.SetActive(false);
        this._menuAfterClose.SetActive(true);
    }
}
