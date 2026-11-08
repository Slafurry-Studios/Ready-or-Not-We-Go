using UnityEngine;

[CreateAssetMenu(fileName = "Project Data", menuName = "Project Data")]
public class Project : ScriptableObject
{
    [SerializeField] private string _projectName;
    [SerializeField] private readonly Sprint[] _sprints;
    [SerializeField] private int _maxProgress = 100;

    public string ProjectName => _projectName;
    public Sprint[] Sprints => _sprints;
    public int MaxProgress => _maxProgress;
}