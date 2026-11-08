using UnityEngine;

public class Action
{
    public string ActionName;
    public StatusModifier[] StatusModifiers;

    public virtual void Execute()
    {
        Debug.Log("Executing action: " + ActionName);
    }
}

