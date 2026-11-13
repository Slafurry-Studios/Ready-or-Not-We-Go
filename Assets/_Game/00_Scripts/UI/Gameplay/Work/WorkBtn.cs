
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class WorkBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Slider _progressSlider;
    private Task _task;
    private TaskPopup _taskPopup;
    private Team _team;
    private Button _button;
    private TaskStatus _lastStatus;
    private Coroutine _punchCoroutine;
    private Vector3 _originalScale;

    public void Initialize(Task task, TaskPopup taskPopup, Team team)
    {
        _task = task;
        _taskPopup = taskPopup;
        _team = team;
        _label.text = _task.TaskName;

        _button = GetComponent<Button>();
        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);

        if (_progressSlider != null)
        {
            _progressSlider.maxValue = 100f;
            RefreshSlider();
        }

        _originalScale = transform.localScale;
        RefreshInteractable();
    }

    private void Update()
    {
        if (_task == null || _team == null || _button == null) return;

        var status = _team.GetTaskStatus(_task);
        bool anyInProgress = _team.IsAnyTaskInProgress();
        _button.interactable = status == TaskStatus.NOT_STARTED && !anyInProgress;

        if (status == TaskStatus.IN_PROGRESS)
        {
            if (_progressSlider != null)
                _progressSlider.value = _team.GetTaskProgress(_task) * 100f;
        }

        if (status == TaskStatus.COMPLETED && _lastStatus != TaskStatus.COMPLETED)
        {
            if (_punchCoroutine != null)
                StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(PunchAndHide());
        }

        _lastStatus = status;
    }

    private void RefreshSlider()
    {
        if (_progressSlider == null) return;

        _progressSlider.value = _lastStatus == TaskStatus.COMPLETED ? 100f : 0f;
    }

    private IEnumerator PunchAndHide()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 start = _originalScale;
        Vector3 punch = start * 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            transform.localScale = start * scale;
            yield return null;
        }

        transform.localScale = start;
        gameObject.SetActive(false);
        _punchCoroutine = null;
    }

    private void RefreshInteractable()
    {
        if (_button == null || _team == null || _task == null) return;

        bool canInteract = _team.GetTaskStatus(_task) == TaskStatus.NOT_STARTED
            && !_team.IsAnyTaskInProgress();

        _button.interactable = canInteract;
    }

    private void OnClick()
    {
        if (_task == null || _taskPopup == null || _team == null) return;
        if (_team.GetTaskStatus(_task) != TaskStatus.NOT_STARTED) return;

        _taskPopup.SetText(_task);
        _taskPopup.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }
}
