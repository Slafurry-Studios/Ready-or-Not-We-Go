using UnityEngine;
using System;

public class Team : MonoBehaviour
{
    [Header("Basic Information")]
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private TeamRole _teamRole;

    [Header("Status")]
    [SerializeField] private TeamCondition _teamCondition;
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private Task[] _tasks;
    [SerializeField] private MemoryBank _memoryBank;

    [Header("Available Actions")]
    [SerializeField] private Action[] _actionCollection;
    [SerializeField] private Action[] _availableActions;

    public event System.Action OnTasksChanged;
    public event System.Action OnAvailableActionsChanged;
    public event System.Action<TeamCondition> OnConditionChanged;

    public string ID => _id;
    public string Name => _name;
    public TeamRole TeamRole => _teamRole;

    public TeamCondition TeamCondition => _teamCondition;
    public EnergyStatus Energy => _energyStatus;
    public Task[] Tasks => _tasks;
    public MemoryBank MemoryBank => _memoryBank;

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
}