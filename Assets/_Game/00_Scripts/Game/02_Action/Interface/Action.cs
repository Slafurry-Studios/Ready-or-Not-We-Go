using UnityEngine;

[CreateAssetMenu(fileName = "Action Data", menuName = "Action Data")]
public class Action : ScriptableObject
{
    [Header("Action Info")]
    [SerializeField] private string _actionId;
    [SerializeField] private string _actionName;

    [Header("Action Settings")]
    [SerializeField] private float _duration;
    [SerializeField] private float _energyUsed;

    [Header("Action Requirements")]
    [SerializeField] private Information[] _requiredInformation;
    [SerializeField] private int MinimumDay = 0;

    [Header("Modifiers & Rewards")]
    [SerializeField] private Memory[] _informationGotten;
    [SerializeField] private Action[] _nextActions;
    [SerializeField] private ActionExecution _actionExecution;

    public string ActionId => _actionId;
    public string ActionName => _actionName;
    public float Duration => _duration;
    public float EnergyUsed => _energyUsed;
    public Information[] RequiredInformation => _requiredInformation;
    public int MinimumDayRequirement => MinimumDay;
    public Memory[] InformationGotten => _informationGotten;
    public Action[] NextActions => _nextActions;

    public ActionExecution ActionExecution => _actionExecution;
}