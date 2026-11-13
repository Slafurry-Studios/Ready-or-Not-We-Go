using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private EnergyStatus _artsyEnergy;
    [SerializeField] private ProjectStatus _projectStatus;

    private readonly Dictionary<Action, Coroutine> _activeActions = new();

    public event System.Action<Action> OnActionCompleted;

    public bool IsActionExecuting(Action action)
    {
        if (action == null) return false;
        return _activeActions.ContainsKey(action);
    }

    /// <summary>Starts executing an action on a team. Returns false if already executing or prerequisites not met.</summary>
    public bool StartAction(Action action, Team team)
    {
        if (action == null) return false;
        if (_activeActions.ContainsKey(action)) return false;
        if (team == null) return false;
        if (!team.CanExecuteAction(action)) return false;

        // Instant actions (duration <= 0) complete immediately
        if (action.Duration <= 0f)
        {
            CompleteAction(action, team);
            return true;
        }

        // Timed actions run a coroutine on this persistent object
        var coroutine = StartCoroutine(ExecuteActionRoutine(action, team));
        _activeActions[action] = coroutine;
        return true;
    }

    private IEnumerator ExecuteActionRoutine(Action action, Team team)
    {
        float elapsed = 0f;
        float duration = action.Duration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            team.SetActionProgress(action, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        team.SetActionProgress(action, 1f);
        CompleteAction(action, team);
    }

    private void CompleteAction(Action action, Team team)
    {
        Debug.Log($"[ActionRunner] completing action: {action.ActionName} on team {team.name}, energyUsed={action.EnergyUsed}");

        // Apply action execution (status modifiers, etc.)
        action.ActionExecution?.ExecuteAction(team);

        // Deduct energy from artsy's energy status
        if (_artsyEnergy != null)
        {
            _artsyEnergy.AddValue(-action.EnergyUsed);
            Debug.Log($"[ActionRunner] energy drained by {action.EnergyUsed}, remaining={_artsyEnergy.CurrentValue}");
        }
        else
        {
            Debug.LogWarning($"[ActionRunner] _artsyEnergy is null — energy won't drain. Assign EnergyStatus on ActionRunner.");
        }

        // Grant information / memories
        if (action.InformationGotten != null)
        {
            foreach (var mem in action.InformationGotten)
            {
                if (mem != null && mem.information != null)
                    team.GrantInformation(mem.information);
            }
        }

        team.SetActionCompleted(action, true);
        _activeActions.Remove(action);

        OnActionCompleted?.Invoke(action);
        Debug.Log($"[ActionRunner] action {action.ActionName} completed");
    }
}
