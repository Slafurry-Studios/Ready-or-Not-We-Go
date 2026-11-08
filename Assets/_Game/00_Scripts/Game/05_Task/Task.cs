using UnityEngine;

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

    [Header("Settings")]
    [Tooltip("Value between 0 and 2")]
    [Range(0f, 2f)]
    [SerializeField] private float _unexpectedInterval = 1f;

    [Header("Progress & Dependencies")]
    [Tooltip("Progress contribution to the overall project (0% to 100%)")]
    [Range(0f, 100f)]
    [SerializeField] private float _projectProgress;

    [SerializeField] private Task[] _dependsBy;

    // --- PUBLIC PROPERTIES (Read-Only access) ---
    public string TaskId => _taskId;
    public string TaskName => _taskName;
    
    public float EnergyNeeded => _energyNeeded;
    public float TimeNeeded => _timeNeeded;
    public TeamRole RoleForTask => _roleForTask;

    public float UnexpectedInterval => _unexpectedInterval;
    public float ProjectProgress => _projectProgress;
    public Task[] DependsBy => _dependsBy;

    // Validates field ranges inside the Unity Inspector
    private void OnValidate()
    {
        _unexpectedInterval = Mathf.Clamp(_unexpectedInterval, 0f, 2f);
        _projectProgress = Mathf.Clamp(_projectProgress, 0f, 100f);
    }
}