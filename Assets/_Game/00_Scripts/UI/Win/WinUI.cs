using UnityEngine;
using Slafurry.Systems.Scene;

public class WinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneReference _mainMenuScene;

    [Header("Buttons (assign in Inspector)")]
    [SerializeField] private UnityEngine.UI.Button _mainMenuBtn;
    [SerializeField] private UnityEngine.UI.Button _exitBtn;

    private void Awake()
    {
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.RemoveListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.RemoveListener(OnExitClicked);
    }

    private void OnMainMenuClicked()
    {
        if (_mainMenuScene != null)
            SceneSystem.Load(_mainMenuScene);
        else
            Debug.LogWarning("[WinUI] _mainMenuScene is not assigned.");
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
