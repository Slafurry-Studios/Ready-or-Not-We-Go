
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrowseBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    private Action _action;
    private ActionPopup _actionPopup;
    private Button _button;

    public void Initialize(Action action, ActionPopup actionPopup)
    {
        _action = action;
        _actionPopup = actionPopup;

        _label.text = _action.ActionName;

        _button = GetComponent<Button>();

        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_action == null || _actionPopup == null) return;

        _actionPopup.SetText(_action);
        _actionPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }
}
