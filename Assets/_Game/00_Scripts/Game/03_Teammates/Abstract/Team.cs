using UnityEngine;

public class Team : MonoBehaviour
{
    [Header("Basic Information")]
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private TeamRole _teamRole;

    [Header("Status")]
    [SerializeField] private TeamCondition _teamCondition;
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private SuspicionStatus _suspicionStatus;
    [SerializeField] private Task[] _tasks;
    [SerializeField] private MemoryBank _memoryBank;
    [SerializeField] private TeamFamiliarity[] _teamFamiliarities;

    [Header("Available Actions")]
    [SerializeField] private Action[] _actionCollection;
    [SerializeField] private Action[] _availableActions;

    public string ID => _id;
    public string Name => _name;
    public TeamRole TeamRole => _teamRole;

    public TeamCondition TeamCondition => _teamCondition;
    public EnergyStatus Energy => _energyStatus;
    public SuspicionStatus Suspicion => _suspicionStatus;
    public Task[] Tasks => _tasks;
    public MemoryBank MemoryBank => _memoryBank;
    public TeamFamiliarity[] TeamFamiliarities => _teamFamiliarities;

    public Action[] ActionCollection => _actionCollection;
    public Action[] AvailableActions => _availableActions;
}

