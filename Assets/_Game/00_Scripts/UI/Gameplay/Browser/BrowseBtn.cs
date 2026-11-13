
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
        if (_team == null || _action == null) return;

        bool executing = _team.IsActionExecuting(_action);
        bool completed = _team.GetActionCompleted(_action);
        bool canStart = !executing && !completed && _team.CanExecuteAction(_action);

        _button.interactable = canStart;
    }

    private void RefreshSlider()
    {
        if (_progressSlider == null)
        {
            _progressSlider = GetComponentInChildren<Slider>(true);
            if (_progressSlider != null)
                _progressSlider.maxValue = 100f;
        }

        if (_progressSlider == null || _team == null || _action == null) return;

        if (_team.IsActionExecuting(_action))
        {
            float progress = _team.GetActionProgress(_action);
            Debug.Log($"[BrowseBtn] slider: executing, progress={progress}, value={progress * 100f}");
            _progressSlider.value = progress * 100f;
        }
        else if (_team.GetActionCompleted(_action))
        {
            _progressSlider.value = 100f;
        }
        else
        {
            _progressSlider.value = 0f;
        }
    }

    private void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_progressSlider != null)
            _progressSlider.maxValue = 100f;

        RefreshSlider();
    }

    private void OnClick()
    {
        if (_action == null || _actionPopup == null || _team == null) return;
        if (!_team.CanExecuteAction(_action)) return;

        _actionPopup.SetText(_action, _team);
        _actionPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }
}
