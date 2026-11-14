using UnityEngine;
using Slafurry.Systems.Scene;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TaskRunner _taskRunner;
    [SerializeField] private SceneReference _mainMenuScene;

    [Header("Buttons (assign in Inspector)")]
    [SerializeField] private UnityEngine.UI.Button _retryBtn;
    [SerializeField] private UnityEngine.UI.Button _mainMenuBtn;
    [SerializeField] private UnityEngine.UI.Button _exitBtn;

    private void Awake()
    {
        if (_retryBtn != null) _retryBtn.onClick.AddListener(OnRetryClicked);
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (_retryBtn != null) _retryBtn.onClick.RemoveListener(OnRetryClicked);
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.RemoveListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.RemoveListener(OnExitClicked);
    }

    private void OnRetryClicked()
    {
        if (_taskRunner != null)
            _taskRunner.ResetGameDay1();
        else
            Debug.LogWarning("[GameOverUI] _taskRunner is not assigned.");
    }

    private void OnMainMenuClicked()
    {
        if (_mainMenuScene != null)
            SceneSystem.Load(_mainMenuScene);
        else
            Debug.LogWarning("[GameOverUI] _mainMenuScene is not assigned.");
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
