using UnityEngine;

public class TeamCollection : MonoBehaviour
{
    [SerializeField] public readonly Team[] _teams;
    public Team[] Teams => _teams;
}