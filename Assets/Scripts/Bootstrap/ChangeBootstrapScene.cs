using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeBootstrapScene : MonoBehaviour
{
    [SerializeField] private string _mainMenuSceneName;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset _mainMenuScene;

    private void OnValidate()
    {
        this._mainMenuSceneName = this._mainMenuScene.name;
    }
#endif

    private void Start()
    {
        SceneManager.LoadScene(this._mainMenuSceneName, LoadSceneMode.Single);
    }
}
