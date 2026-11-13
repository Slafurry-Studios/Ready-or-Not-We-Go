using UnityEngine;
using System.Collections.Generic;

public class MemberAI : MonoBehaviour, ITimedUpdate
{
    [Header("References")]
    [SerializeField] private Team _team;
    [SerializeField] private TaskRunner _taskRunner;

    [Header("AI Settings")]
    [SerializeField] private float _decisionChance = 0.5f;

    private bool _registered;

    private void OnEnable()
    {
        if (!_registered)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
            {
                timeCounter.Register(this);
                _registered = true;
            }
        }
    }

    private void OnDisable()
    {
        if (_registered)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
                timeCounter.Unregister(this);
            _registered = false;
        }
    }

    public void TimedUpdate()
    {
        if (_team == null || _taskRunner == null) return;
        if (_team.TeamRole == TeamRole.FOUNDER) return;

        if (_team.IsAnyTaskInProgress()) return;

        if (Random.value > _decisionChance) return;

        foreach (var task in _team.Tasks)
        {
            if (task == null) continue;
            if (_team.CanExecuteTask(task))
            {
                var variant = SelectVariant(task);
                _taskRunner.StartTask(task, variant, _team, _team.Energy);
                return;
            }
        }

        foreach (var action in _team.AvailableActions)
        {
            if (action == null) continue;
            if (_team.CanExecuteAction(action))
            {
                _team.ExecuteAction(action);
                return;
            }
        }
    }

    private TaskVariant SelectVariant(Task task)
    {
        if (_team.Energy == null) return TaskVariant.Normal;

        var energy = _team.Energy.CurrentValue;
        if (energy >= task.GetEnergyNeeded(TaskVariant.Fast))
            return TaskVariant.Fast;
        if (energy >= task.GetEnergyNeeded(TaskVariant.Normal))
            return TaskVariant.Normal;
        return TaskVariant.Slow;
    }
}
