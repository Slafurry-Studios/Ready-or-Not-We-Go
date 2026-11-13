using UnityEngine;

public class InformationGainExecution : ActionExecution
{
    public override void ExecuteAction(Team team)
    {
        // Information granting is handled by Team.ExecuteAction.
        // This execution type exists as a marker for actions whose sole
        // effect is granting information/memories.
    }
}
