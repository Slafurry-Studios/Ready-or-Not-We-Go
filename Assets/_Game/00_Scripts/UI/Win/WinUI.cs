using UnityEngine;
using TMPro;
using Slafurry.Systems.Scene;

public class WinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneReference _mainMenuScene;
    [SerializeField] private TextMeshProUGUI _reasonText;

    [Header("Buttons (assign in Inspector)")]
    [SerializeField] private UnityEngine.UI.Button _mainMenuBtn;
    [SerializeField] private UnityEngine.UI.Button _exitBtn;

    public static string Reason { get; set; } = "";

    private void Awake()
    {
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.AddListener(OnExitClicked);

        if (_reasonText != null && !string.IsNullOrEmpty(Reason))
            _reasonText.text = Reason;
        else if (_reasonText != null)
            _reasonText.text = "You won!";
    }

    private void OnDestroy()
    {
        if (_mainMenuBtn != null) _mainMenuBtn.onClick.RemoveListener(OnMainMenuClicked);
        if (_exitBtn != null) _exitBtn.onClick.RemoveListener(OnExitClicked);
    }

    private void OnMainMenuClicked()
    {
        Reason = "";
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
