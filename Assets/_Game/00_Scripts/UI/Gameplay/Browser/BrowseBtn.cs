
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrowseBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Slider _progressSlider;
    private Action _action;
    private ActionPopup _actionPopup;
    private Team _team;
    private Button _button;

    public void Initialize(Action action, ActionPopup actionPopup, Team team)
    {
        _action = action;
        _actionPopup = actionPopup;
        _team = team;

        _label.text = _action.ActionName;

        _button = GetComponent<Button>();

        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);

        if (_progressSlider != null)
        {
            _progressSlider.maxValue = 100f;
            RefreshSlider();
        }

        RefreshInteractable();
    }

    private void Update()
    {
        if (_button == null || _team == null || _action == null) return;
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        bool canInteract = _team.CanExecuteAction(_action);
        _button.interactable = canInteract;

        RefreshSlider();
    }

    private void RefreshSlider()
    {
        if (_progressSlider == null) return;

        bool completed = _team != null && _team.GetActionCompleted(_action);
        _progressSlider.value = completed ? 100f : 0f;
    }

    private void OnClick()
    {
        if (_action == null || _actionPopup == null || _team == null) return;
        if (!_team.CanExecuteAction(_action)) return;

        _actionPopup.SetText(_action);
        _actionPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }
}
