using UnityEngine;
using UnityEngine.SceneManagement;
using Slafurry.Systems.Scene;
using Slafurry.Systems.Save;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private SceneReference _gameScene;

    [Header("Buttons (assign in Inspector)")]
    [SerializeField] private UnityEngine.UI.Button _startBtn;
    [SerializeField] private UnityEngine.UI.Button _continueBtn;
    [SerializeField] private UnityEngine.UI.Button _exitBtn;

    private void Awake()
    {
        if (_startBtn != null) _startBtn.onClick.AddListener(OnStartClicked);
        if (_continueBtn != null) _continueBtn.onClick.AddListener(OnContinueClicked);
        if (_exitBtn != null) _exitBtn.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (_startBtn != null) _startBtn.onClick.RemoveListener(OnStartClicked);
        if (_continueBtn != null) _continueBtn.onClick.RemoveListener(OnContinueClicked);
        if (_exitBtn != null) _exitBtn.onClick.RemoveListener(OnExitClicked);
    }

    private void OnStartClicked()
    {
        if (_gameScene != null)
            SceneSystem.Load(_gameScene);
        else
            Debug.LogWarning("[MainMenuUI] _gameScene is not assigned.");
    }

    private void OnContinueClicked()
    {
        if (Save.Exists("gamesave"))
        {
            Debug.Log("[MainMenuUI] Continue — loading saved game.");
            if (_gameScene != null)
                SceneSystem.Load(_gameScene);
            else
                Debug.LogWarning("[MainMenuUI] _gameScene is not assigned.");
        }
        else
        {
            Debug.Log("[MainMenuUI] Continue — no save found. Start a new game instead.");
        }
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
