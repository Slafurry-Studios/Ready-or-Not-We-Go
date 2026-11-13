
using UnityEngine;

public enum TaskVariant
{
    Slow,
    Normal,
    Fast
}

[CreateAssetMenu(fileName = "Task Data", menuName = "Task Data")]
public class Task : ScriptableObject
{
    [Header("Task Information")]
    [SerializeField] private string _taskId;
    [SerializeField] private string _taskName;

    [Header("Task Requirements")]
    [SerializeField] private float _energyNeeded;
    [SerializeField] private float _timeNeeded;
    [SerializeField] private TeamRole _roleForTask;

    [Header("Variant Multipliers")]
    [SerializeField] private float _slowEnergyMultiplier = 0.8f;
    [SerializeField] private float _slowTimeMultiplier = 1.5f;

    [SerializeField] private float _normalEnergyMultiplier = 1f;
    [SerializeField] private float _normalTimeMultiplier = 1f;

    [SerializeField] private float _fastEnergyMultiplier = 1.5f;
    [SerializeField] private float _fastTimeMultiplier = 0.7f;

    [Header("Settings")]
    [Range(0f, 2f)]
    [SerializeField] private float _unexpectedInterval = 1f;

    [Header("Progress & Dependencies")]
    [Range(0f, 100f)]
    [SerializeField] private float _projectProgress;

    [SerializeField] private Task[] _dependsBy;

    private TaskStatus _status = TaskStatus.NOT_STARTED;

    public string TaskId => _taskId;
    public string TaskName => _taskName;

    public float EnergyNeeded => _energyNeeded;
    public float TimeNeeded => _timeNeeded;
    public TeamRole RoleForTask => _roleForTask;

    public float UnexpectedInterval => _unexpectedInterval;
    public float ProjectProgress => _projectProgress;
    public Task[] DependsBy => _dependsBy;
    public TaskStatus Status => _status;


    public float GetEnergyNeeded(TaskVariant variant)
    {
        return _energyNeeded * GetEnergyMultiplier(variant);
    }

    public float GetTimeNeeded(TaskVariant variant)
    {
        return _timeNeeded * GetTimeMultiplier(variant);
    }

    public void SetStatus(TaskStatus status)
    {
        _status = status;
    }
    
    private float GetEnergyMultiplier(TaskVariant variant)
    {
        return variant switch
        {
            TaskVariant.Slow => _slowEnergyMultiplier,
            TaskVariant.Normal => _normalEnergyMultiplier,
            TaskVariant.Fast => _fastEnergyMultiplier,
            _ => 1f
        };
    }

    private float GetTimeMultiplier(TaskVariant variant)
    {
        return variant switch
        {
            TaskVariant.Slow => _slowTimeMultiplier,
            TaskVariant.Normal => _normalTimeMultiplier,
            TaskVariant.Fast => _fastTimeMultiplier,
            _ => 1f
        };
    }


    private void OnValidate()
    {
        _unexpectedInterval = Mathf.Clamp(_unexpectedInterval, 0f, 2f);
        _projectProgress = Mathf.Clamp(_projectProgress, 0f, 100f);
    }


}
