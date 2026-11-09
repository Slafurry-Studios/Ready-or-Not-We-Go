using UnityEngine;

[CreateAssetMenu(fileName = "Sprint Data", menuName = "Sprint Data")]
public class Sprint : ScriptableObject
{
    [SerializeField] private string _sprintName;
    [SerializeField] private Task[] _sprintTasks;

    public string SprintName => _sprintName;
    public Task[] SprintTasks => _sprintTasks;
}