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

    private void Awake()
    {
        // Auto-resolve EnergyStatus from same GameObject only (no FindObjectOfType - wrong with multiple teams)
        if (_energyStatus == null)
            _energyStatus = GetComponent<EnergyStatus>();

        // TimeCounter is a single scene system - FindFirstObjectByType is safe here
        if (_timeCounter == null)
            _timeCounter = FindFirstObjectByType<TimeCounter>();

        if (_energyStatus == null)
            Debug.LogWarning($"[Team] {name}: _energyStatus is null. Put EnergyStatus on same GameObject as Team, or assign in Inspector.");
    }

    public event System.Action OnTasksChanged;
    public event System.Action OnAvailableActionsChanged;
    public event System.Action<TeamCondition> OnConditionChanged;
    public event System.Action<Information> OnInformationGained;

    private static readonly Dictionary<Task, TaskStatus> _runtimeTaskStatus = new();
    private readonly Dictionary<string, Information> _gainedInformation = new();

    private static int _terminatedCount;

    public float EnergyCostMultiplier => Mathf.Pow(1.5f, _terminatedCount);

    public static void ResetTerminatedCount() => _terminatedCount = 0;

    public static int GetTerminatedCount() => _terminatedCount;

    public static void SetTerminatedCount(int count) => _terminatedCount = count;

    public static void ResetAllRuntimeStatus() => _runtimeTaskStatus.Clear();

    public string ID => _id;
    public string Name => _name;
    public TeamRole TeamRole => _teamRole;

    public TeamCondition TeamCondition => _teamCondition;
    public EnergyStatus Energy => _energyStatus;
    public Task[] Tasks => _tasks;
    public MemoryBank MemoryBank => _memoryBank;

    public bool IsFounder => _teamRole == TeamRole.FOUNDER;

    public bool HasInformation(string informationId)
    {
        if (string.IsNullOrEmpty(informationId)) return false;
        return _gainedInformation.ContainsKey(informationId);
    }

    public void GrantInformation(Information info, Memory memoryDefinition = null)
    {
        if (info == null || string.IsNullOrEmpty(info.InformationId)) return;
        if (!_gainedInformation.ContainsKey(info.InformationId))
            _gainedInformation[info.InformationId] = info;

        Debug.Log($"[Team] {name} gained information: {info.InformationId}");

        // Add to memory bank with MaxSize check
        if (_memoryBank != null)
        {
            var memDef = memoryDefinition ?? new Memory
            {
                information = info,
                MemoryName = info.InformationName,
                DecayRate = 0.1f,
                RecallTime = 5f,
                MemorySize = 1f
            };

            var memory = new Memory
            {
                MemoryName = memDef.MemoryName,
                DecayRate = memDef.DecayRate,
                RecallTime = memDef.RecallTime,
                MemorySize = memDef.MemorySize,
                information = info
            };

            if (!_memoryBank.AddMemory(memory))
            {
                // Memory bank maxed out - remove from gained info so actions stay blocked
                _gainedInformation.Remove(info.InformationId);
                Debug.LogWarning($"[Team] {name}: memory bank maxed, cannot add {info.InformationId}");
            }
            else
            {
                Debug.Log($"[Team] {name} memory bank now has {_memoryBank.memories.Length} memories");
            }
        }
        else
        {
            Debug.LogWarning($"[Team] {name}: _memoryBank is null — memory won't show in UI. Assign MemoryBank on this Team.");
        }

        OnInformationGained?.Invoke(info);
        Debug.Log($"[Team] {name} fired OnInformationGained for {info.InformationId}");
    }

    public Action[] ActionCollection => _actionCollection;
    public Action[] AvailableActions => _availableActions;

    public void SetTeamCondition(TeamCondition condition)
    {
        _teamCondition = condition;
        OnConditionChanged?.Invoke(_teamCondition);
    }

    public void Terminate()
    {
        if (_teamCondition == TeamCondition.TERMINATED) return;

        _teamCondition = TeamCondition.TERMINATED;
        _terminatedCount++;

        Debug.Log($"[Team] {name} terminated. Terminated count: {_terminatedCount}. Energy multiplier: {EnergyCostMultiplier:F2}x");

        OnConditionChanged?.Invoke(_teamCondition);
    }

    public void SetTasks(Task[] tasks)
    {
        _tasks = tasks;
        OnTasksChanged?.Invoke();
    }

    /// <summary>Returns IDs of tasks assigned to this team that are COMPLETED.</summary>
    public List<string> GetCompletedTaskIds()
    {
        var completed = new List<string>();
        if (_tasks == null) return completed;

        foreach (var t in _tasks)
        {
            if (t == null) continue;
            if (GetTaskStatus(t) == TaskStatus.COMPLETED)
                completed.Add(t.TaskId);
        }
        return completed;
    }

    /// <summary>Restores task statuses for this team's tasks by TaskId.</summary>
    public void RestoreTaskStatuses(List<string> completedTaskIds)
    {
        if (_tasks == null) return;

        var completedSet = new HashSet<string>(completedTaskIds ?? new List<string>());

        foreach (var t in _tasks)
        {
            if (t == null) continue;
            var status = completedSet.Contains(t.TaskId) ? TaskStatus.COMPLETED : TaskStatus.NOT_STARTED;
            SetTaskStatus(t, status);
        }
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
        OnTasksChanged?.Invoke();
    }

    /// <summary>Returns dependency tasks that are not yet COMPLETED (blocking this task).</summary>
    public Task[] GetBlockedDependencies(Task task)
    {
        if (task == null) return System.Array.Empty<Task>();
        if (task.DependsBy == null) return System.Array.Empty<Task>();

        var blocked = new System.Collections.Generic.List<Task>();
        foreach (var dep in task.DependsBy)
        {
            if (dep == null) continue;
            if (GetTaskStatus(dep) != TaskStatus.COMPLETED)
                blocked.Add(dep);
        }
        return blocked.ToArray();
    }

    /// <summary>True if task is NOT_STARTED and all dependency tasks are COMPLETED.</summary>
    public bool CanExecuteTask(Task task)
    {
        if (task == null) return false;
        if (GetTaskStatus(task) != TaskStatus.NOT_STARTED) return false;

        var blocked = GetBlockedDependencies(task);
        if (blocked.Length > 0) return false;

        return true;
    }

    private readonly Dictionary<Task, float> _taskProgress = new();
    private readonly Dictionary<Action, bool> _completedActions = new();
    private readonly Dictionary<Action, float> _actionProgress = new();

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

    public bool GetActionCompleted(Action action)
    {
        if (action == null) return false;
        if (_completedActions.TryGetValue(action, out var completed))
            return completed;
        return false;
    }

    public void SetActionCompleted(Action action, bool completed)
    {
        if (action == null) return;
        _completedActions[action] = completed;
    }

    public bool CanExecuteAction(Action action)
    {
        if (action == null) return false;

        // Already executing
        var actionRunner = FindFirstObjectByType<ActionRunner>();
        if (actionRunner != null && actionRunner.IsActionExecuting(action)) return false;

        // Already completed
        if (_completedActions.ContainsKey(action)) return false;

        // Energy check (Founder has unlimited energy)
        if (_energyStatus != null && !IsFounder && _energyStatus.CurrentValue < action.EnergyUsed * EnergyCostMultiplier)
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

        Debug.Log($"[Team] {name} executing action: {action.ActionName}, duration={action.Duration}, energyUsed={action.EnergyUsed}");

        // Delegate to ActionRunner (persistent, owns the coroutine - same pattern as TaskRunner)
        var actionRunner = FindFirstObjectByType<ActionRunner>();
        if (actionRunner != null)
        {
            Debug.Log($"[Team] {name}: delegating to ActionRunner");
            actionRunner.StartAction(action, this);
            return;
        }

        // Fallback: instant completion if no ActionRunner in scene
        Debug.Log($"[Team] {name}: no ActionRunner in scene, completing instantly");
        CompleteAction(action);
    }

    private void CompleteAction(Action action)
    {
        // Apply action execution (status modifiers, etc.)
        action.ActionExecution?.ExecuteAction(this);

        // Deduct energy (Founder has unlimited energy)
        if (_energyStatus != null && !IsFounder)
            _energyStatus.AddValue(-action.EnergyUsed * EnergyCostMultiplier);

        // Grant information / memories
        if (action.InformationGotten != null)
        {
            foreach (var mem in action.InformationGotten)
            {
                if (mem != null && mem.information != null)
                    GrantInformation(mem.information, mem);
            }
        }

        _completedActions[action] = true;

        OnAvailableActionsChanged?.Invoke();
    }

    public void RemoveInformation(string informationId)
    {
        if (string.IsNullOrEmpty(informationId)) return;

        if (_gainedInformation.ContainsKey(informationId))
        {
            var info = _gainedInformation[informationId];
            _gainedInformation.Remove(informationId);

            // Remove from memory bank
            if (_memoryBank != null && _memoryBank.memories != null)
            {
                Memory toRemove = null;
                foreach (var m in _memoryBank.memories)
                {
                    if (m != null && m.information == info)
                    {
                        toRemove = m;
                        break;
                    }
                }
                if (toRemove != null)
                    _memoryBank.RemoveMemory(toRemove);
            }
        }
    }

    public float GetActionProgress(Action action)
    {
        if (action == null) return 0f;
        if (_actionProgress.TryGetValue(action, out var progress))
            return progress;
        return 0f;
    }

    public void SetActionProgress(Action action, float progress)
    {
        if (action == null) return;
        _actionProgress[action] = Mathf.Clamp01(progress);
    }

    public bool IsActionExecuting(Action action)
    {
        if (action == null) return false;
        var actionRunner = FindFirstObjectByType<ActionRunner>();
        if (actionRunner != null)
            return actionRunner.IsActionExecuting(action);
        return false;
    }
}