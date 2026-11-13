using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private ProjectManager _projectManager;

    [Header("Execution")]
    [SerializeField] private EnergyStatus _artsyEnergy;
    [SerializeField] private ProjectStatus _projectStatus;

    [Header("Teams")]
    [Tooltip("Standard teams that receive daily spawned tasks.")]
    [SerializeField] private Team[] _teams;

    [Tooltip("Player character team. Excluded from _teams array processing.")]
    [SerializeField] private Team _playerTeam;

    private int _lastDay = -1;
    private readonly Dictionary<Task, Coroutine> _activeTasks = new();

    private void Update()
    {
        int day = _timeCounter.CurrentDay;
        if (day == _lastDay) return;
        _lastDay = day;

        var sprintTasks = _projectManager.GetTasksByDay(day);

        if (_teams != null)
        {
            foreach (var team in _teams)
            {
                if (team == null) continue;
                if (team == _playerTeam) continue; // handled separately below

                SpawnTasksForTeam(team, sprintTasks, day);
            }
        }

        // Player character also receives sprint-based tasks
        if (_playerTeam != null)
            SpawnTasksForTeam(_playerTeam, sprintTasks, day);
    }

    private void SpawnTasksForTeam(Team team, Task[] sprintTasks, int day)
    {
        var active = new List<Task>();

        if (sprintTasks != null)
        {
            foreach (var t in sprintTasks)
            {
                if (t == null) continue;
                if (t.RoleForTask != team.TeamRole) continue; // role match only
                if (team.GetTaskStatus(t) == TaskStatus.NOT_STARTED)
                    active.Add(t);
            }
        }

        team.SetTasks(active.ToArray());
    }

    /// <summary>Called by TaskPopup when player clicks a variant button. Runs on this persistent object so the timer survives popup deactivation.</summary>
    public event System.Action<Task> OnTaskCompleted;

    public bool StartTask(Task task, TaskVariant variant, Team team)
    {
        if (task == null) return false;
        if (_activeTasks.ContainsKey(task)) return false;
        if (team == null) return false;
        if (!team.CanExecuteTask(task)) return false;
        if (_artsyEnergy == null) return false;

        float cost = task.GetEnergyNeeded(variant);
        if (_artsyEnergy.CurrentValue < cost) return false;

        team.SetTaskStatus(task, TaskStatus.IN_PROGRESS);
        _artsyEnergy.AddValue(-cost);

        var coroutine = StartCoroutine(WorkRoutine(task, variant, team));
        _activeTasks[task] = coroutine;
        return true;
    }

    private IEnumerator WorkRoutine(Task task, TaskVariant variant, Team team)
    {
        float minutes = task.GetTimeNeeded(variant);
        float seconds = minutes * _timeCounter.MinuteToSecondRatio;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            team.SetTaskProgress(task, elapsed / seconds);
            yield return null;
        }

        team.SetTaskProgress(task, 1f);
        team.SetTaskStatus(task, TaskStatus.COMPLETED);

        if (_projectStatus != null)
            _projectStatus.AddValue(task.ProjectProgress);

        _activeTasks.Remove(task);
        OnTaskCompleted?.Invoke(task);
    }
}
