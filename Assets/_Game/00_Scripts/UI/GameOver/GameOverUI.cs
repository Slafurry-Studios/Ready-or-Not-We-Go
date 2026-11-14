using UnityEngine;
using TMPro;
using Slafurry.Systems.Scene;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneReference _gameScene;
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private TextMeshProUGUI _reasonText;

    [Header("Buttons (assign in Inspector)")]
    [SerializeField] private UnityEngine.UI.Button _retryBtn;
    [SerializeField] private UnityEngine.UI.Button _mainMenuBtn;
    [SerializeField] private UnityEngine.UI.Button _exitBtn;

    public static string Reason { get; set; } = "";

    private void Awake()
    {
        if (_retryBtn != null) _retryBtn.onClick.AddListener(OnRetryClicked);
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.AddListener(OnExitClicked);

        if (_reasonText != null && !string.IsNullOrEmpty(Reason))
            _reasonText.text = Reason;
        else if (_reasonText != null)
            _reasonText.text = "Game Over.";
    }

    private void OnDestroy()
    {
        if (_retryBtn != null) _retryBtn.onClick.RemoveListener(OnRetryClicked);
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.RemoveListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.RemoveListener(OnExitClicked);
    }

    private void OnRetryClicked()
    {
        Reason = "";
        if (_gameScene != null)
            SceneSystem.Load(_gameScene);
        else
            Debug.LogWarning("[GameOverUI] _gameScene is not assigned.");
    }

    private void OnMainMenuClicked()
    {
        Reason = "";
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
