using UnityEngine;

public class StatusModifierExecution : ActionExecution
{
    [SerializeField] private StatusModifier[] _modifiers;

    public override void ExecuteAction(Team team)
    {
        if (_modifiers == null) return;

        foreach (var m in _modifiers)
        {
            if (m == null || m.targetStatus == null) continue;
            m.targetStatus.AddValue(m.modifierValue);
        }
    }
}
