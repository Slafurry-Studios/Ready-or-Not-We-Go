
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class TaskPopup : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI _titleTxt;
    [SerializeField] private TextMeshProUGUI _descTxt;

    [Header("Variant Buttons")]
    [SerializeField] private Button _slowBtn;
    [SerializeField] private Button _normalBtn;
    [SerializeField] private Button _fastBtn;

    private Task _currentTask;
    private TaskVariant _selectedVariant;

    public Task CurrentTask => _currentTask;
    public TaskVariant SelectedVariant => _selectedVariant;

    private void Awake()
    {
        _slowBtn.onClick.AddListener(
            () => SelectVariant(TaskVariant.Slow));

        _normalBtn.onClick.AddListener(
            () => SelectVariant(TaskVariant.Normal));

        _fastBtn.onClick.AddListener(
            () => SelectVariant(TaskVariant.Fast));
    }

    public void SetText(Task task)
    {
        if (task == null) return;

        _currentTask = task;
        _titleTxt.text = task.TaskName;

        UpdateDescription();

        SelectVariant(TaskVariant.Normal);
    }

    private void UpdateDescription()
    {
        if (_currentTask == null) return;

        StringBuilder description = new StringBuilder();

        AppendVariant(description, "Slow", TaskVariant.Slow);
        AppendVariant(description, "Normal", TaskVariant.Normal);
        AppendVariant(description, "Fast", TaskVariant.Fast);

        if (_currentTask.RoleForTask != default)
            description.AppendLine($"Role: {_currentTask.RoleForTask}");

        if (_currentTask.ProjectProgress != 0)
            description.AppendLine($"Project Progress: {_currentTask.ProjectProgress}%");

        _descTxt.text = description.ToString().TrimEnd();
    }

    private void AppendVariant(StringBuilder description, string title, TaskVariant variant)
    {
        float energy = _currentTask.GetEnergyNeeded(variant);
        float time = _currentTask.GetTimeNeeded(variant);

        description.AppendLine($"{title}: {energy} Energy, {time} Time");
    }
    private void SelectVariant(TaskVariant variant)
    {
        if (_currentTask == null) return;

        _selectedVariant = variant;
    }

    public void ClosePopup()
    {
        _currentTask = null;
        gameObject.SetActive(false);
    }
}
