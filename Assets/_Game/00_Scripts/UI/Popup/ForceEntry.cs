using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForceEntry : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _text;

    private Task _task;
    private Team _team;
    private TaskRunner _taskRunner;

    public void Setup(Task task, Team team, TaskRunner taskRunner)
    {
        _task = task;
        _team = team;
        _taskRunner = taskRunner;
        _text.text = $"Force {team.Name} to do {task.TaskName}";

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnForce);
    }

    private void OnForce()
    {
        _taskRunner.StartTask(_task, TaskVariant.Fast, _team, _team.Energy, true);
    }
}
