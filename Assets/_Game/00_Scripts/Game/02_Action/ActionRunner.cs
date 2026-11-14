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

    private readonly Dictionary<Action, ActiveActionEntry> _activeActions = new();
    private bool _isPaused;

    private class ActiveActionEntry
    {
        public Coroutine Coroutine;
        public Team Team;
        public float Duration;
    }

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
        var entry = new ActiveActionEntry
        {
            Team = team,
            Duration = action.Duration
        };
        var coroutine = StartCoroutine(ExecuteActionRoutine(action, team, entry));
        entry.Coroutine = coroutine;
        _activeActions[action] = entry;
        return true;
    }

    /// <summary>Pause all active action coroutines (called when sleep starts).</summary>
    public void PauseAll() => _isPaused = true;

    /// <summary>Resume all active action coroutines (called when sleep ends).</summary>
    public void ResumeAll() => _isPaused = false;

    /// <summary>Simulate progress for all active actions of a team over elapsed game hours.</summary>
    public void SimulateActionProgressForSleep(Team team, float elapsedHours)
    {
        if (team == null) return;

        float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
        float secondsToAdvance = elapsedHours * secondsPerHour;

        var toComplete = new List<Action>();
        foreach (var kvp in _activeActions)
        {
            if (kvp.Value.Team != team) continue;

            float progressIncrement = secondsToAdvance / kvp.Value.Duration;
            float currentProgress = team.GetActionProgress(kvp.Key);
            float newProgress = Mathf.Clamp01(currentProgress + progressIncrement);
            team.SetActionProgress(kvp.Key, newProgress);

            if (newProgress >= 1f)
                toComplete.Add(kvp.Key);
        }

        foreach (var action in toComplete)
        {
            if (!_activeActions.TryGetValue(action, out var entry)) continue;
            CompleteActionFromSleep(action, entry.Team);
        }
    }

    private void CompleteActionFromSleep(Action action, Team team)
    {
        action.ActionExecution?.ExecuteAction(team);

        if (_artsyEnergy != null)
            _artsyEnergy.AddValue(-action.EnergyUsed);

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
    }

    private IEnumerator ExecuteActionRoutine(Action action, Team team, ActiveActionEntry entry)
    {
        float elapsed = 0f;
        float duration = entry.Duration;

        while (elapsed < duration)
        {
            while (_isPaused) yield return null;
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
