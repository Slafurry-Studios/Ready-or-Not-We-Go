
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class WorkBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    private Task _task;
    private TaskPopup _taskPopup;
    private Button _button;

    public void Initialize(Task task, TaskPopup taskPopup)
    {
        _task = task;
        _taskPopup = taskPopup;
        _label.text = _task.TaskName;

        _button = GetComponent<Button>();
        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_task == null || _taskPopup == null) return;

        _taskPopup.SetText(_task);
        _taskPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }
}
