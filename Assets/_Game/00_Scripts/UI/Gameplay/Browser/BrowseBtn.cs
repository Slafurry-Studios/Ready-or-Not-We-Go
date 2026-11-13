
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrowseBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
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
