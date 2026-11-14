using UnityEngine;
using System.Collections.Generic;

public class FounderAI : MonoBehaviour, ITimedUpdate
{
    [Header("References")]
    [SerializeField] private Team _team;

    [Header("Founder Settings")]
    [SerializeField] private float _searchInterval = 10f;

    private bool _registered;
    private float _searchTimer;

    private void Awake()
    {
        // Founder has unlimited energy and 2x execution speed.
        // Team.IsFounder is derived from TeamRole.FOUNDER.
    }

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
        if (_team == null) return;

        // Founder has unlimited energy — no energy drain from suspicion.
        _searchTimer += Time.deltaTime;

        if (_searchTimer >= _searchInterval)
        {
            _searchTimer = 0f;

            // Founder does not drain energy.
        }
    }

    /// <summary>
    /// Inherits unfinished (NOT_STARTED / IN_PROGRESS) tasks from a terminated team.
    /// The dead team stays TERMINATED; the Founder just takes over their task list.
    /// </summary>
    public void InheritTasksFrom(Team deadTeam)
    {
        if (deadTeam == null || deadTeam == _team) return;

        var existing = _team.Tasks ?? new Task[0];
        var inherited = new List<Task>(existing);

        foreach (var task in deadTeam.Tasks)
        {
            if (task == null) continue;

            var status = deadTeam.GetTaskStatus(task);
            if (status != TaskStatus.NOT_STARTED && status != TaskStatus.IN_PROGRESS)
                continue;

            // Avoid duplicates by TaskId.
            bool alreadyHas = false;
            foreach (var t in existing)
            {
                if (t != null && t.TaskId == task.TaskId)
                {
                    alreadyHas = true;
                    break;
                }
            }
            if (alreadyHas) continue;

            inherited.Add(task);
        }

        _team.SetTasks(inherited.ToArray());

        Debug.Log(
            $"[FounderAI] {_team.name} inherited {inherited.Count - existing.Length} " +
            $"tasks from terminated team: {deadTeam.name}"
        );
    }
}
