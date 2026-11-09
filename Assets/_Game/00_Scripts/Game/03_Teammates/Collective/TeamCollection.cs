using UnityEngine;

[CreateAssetMenu(fileName = "Team Collection Data", menuName = "Team Collection Data")]
public class TeamCollection : ScriptableObject
{
    [SerializeField] public Team[] _teams;
    public Team[] Teams => _teams;
}