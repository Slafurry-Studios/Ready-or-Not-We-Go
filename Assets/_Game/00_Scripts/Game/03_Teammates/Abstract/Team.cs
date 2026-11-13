using UnityEngine;
using System;
using System.Collections.Generic;

public class Team : MonoBehaviour
{
    [Header("Basic Information")]
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private TeamRole _teamRole;

    [Header("Status")]
    [SerializeField] private TeamCondition _teamCondition;
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private Task[] _tasks;
    [SerializeField] private MemoryBank _memoryBank;

    [Header("Available Actions")]
    [SerializeField] private Action[] _actionCollection;
    [SerializeField] private Action[] _availableActions;

    public event System.Action OnTasksChanged;
    public event System.Action OnAvailableActionsChanged;
    public event System.Action<TeamCondition> OnConditionChanged;

    private readonly Dictionary<Task, TaskStatus> _runtimeTaskStatus = new();
    private readonly Dictionary<string, Information> _gainedInformation = new();

    public string ID => _id;
    public string Name => _name;
    public TeamRole TeamRole => _teamRole;

    public TeamCondition TeamCondition => _teamCondition;
    public EnergyStatus Energy => _energyStatus;
    public Task[] Tasks => _tasks;
    public MemoryBank MemoryBank => _memoryBank;

    public bool HasInformation(string informationId)
    {
        if (string.IsNullOrEmpty(informationId)) return false;
        return _gainedInformation.ContainsKey(informationId);
    }

    public void GrantInformation(Information info)
    {
        if (info == null || string.IsNullOrEmpty(info.InformationId)) return;
        if (!_gainedInformation.ContainsKey(info.InformationId))
            _gainedInformation[info.InformationId] = info;
    }

    public Action[] ActionCollection => _actionCollection;
    public Action[] AvailableActions => _availableActions;

    public void SetTeamCondition(TeamCondition condition)
    {
        _teamCondition = condition;
        OnConditionChanged?.Invoke(_teamCondition);
    }

    public void SetTasks(Task[] tasks)
    {
        _tasks = tasks;
        OnTasksChanged?.Invoke();
    }

    public void SetAvailableActions(Action[] actions)
    {
        _availableActions = actions;
        OnAvailableActionsChanged?.Invoke();
    }

    public TaskStatus GetTaskStatus(Task task)
    {
        if (task == null) return TaskStatus.NOT_STARTED;
        if (_runtimeTaskStatus.TryGetValue(task, out var status))
            return status;
        return TaskStatus.NOT_STARTED;
    }

    public void SetTaskStatus(Task task, TaskStatus status)
    {
        if (task == null) return;
        _runtimeTaskStatus[task] = status;
    }

    private readonly Dictionary<Task, float> _taskProgress = new();

    public float GetTaskProgress(Task task)
    {
        if (task == null) return 0f;
        if (_taskProgress.TryGetValue(task, out var progress))
            return progress;
        return 0f;
    }

    public void SetTaskProgress(Task task, float progress)
    {
        if (task == null) return;
        _taskProgress[task] = Mathf.Clamp01(progress);
    }

    public bool IsAnyTaskInProgress()
    {
        if (_tasks == null) return false;

        foreach (var t in _tasks)
        {
            if (t != null && GetTaskStatus(t) == TaskStatus.IN_PROGRESS)
                return true;
        }

        return false;
    }

    public bool CanExecuteAction(Action action)
    {
        if (action == null) return false;

        // Energy check
        if (_energyStatus != null && _energyStatus.CurrentValue < action.EnergyUsed)
            return false;

        // Minimum day check
        if (_timeCounter != null && _timeCounter.CurrentDay < action.MinimumDayRequirement)
            return false;

        // Required information check
        if (action.RequiredInformation != null)
        {
            foreach (var req in action.RequiredInformation)
            {
                if (req != null && !HasInformation(req.InformationId))
                    return false;
            }
        }

        return true;
    }

    public void ExecuteAction(Action action)
    {
        if (action == null) return;
        if (!CanExecuteAction(action)) return;

        // Apply action execution (status modifiers, etc.)
        action.ActionExecution?.ExecuteAction(this);

        // Deduct energy
        if (_energyStatus != null)
            _energyStatus.AddValue(-action.EnergyUsed);

        // Grant information / memories
        if (action.InformationGotten != null)
        {
            foreach (var mem in action.InformationGotten)
            {
                if (mem != null && mem.information != null)
                    GrantInformation(mem.information);
            }
        }

        OnAvailableActionsChanged?.Invoke();
    }
}