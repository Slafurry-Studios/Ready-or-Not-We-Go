using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Slafurry.Systems.Scene;

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

    [Header("Game Loop")]
    [SerializeField] private string _gameOverSceneName = "GameOver";
    [SerializeField] private string _winSceneName = "Win";
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private int _lastDay = -1;
    private float _gameStartTime;
    private bool _deadlineChecked;
    private readonly Dictionary<Task, ActiveTaskEntry> _activeTasks = new();
    private bool _isPaused;

    private class ActiveTaskEntry
    {
        public Coroutine Coroutine;
        public Team Team;
        public TaskVariant Variant;
        public float SecondsNeeded;
    }

    public Team PlayerTeam => _playerTeam;

    private void Update()
    {
        if (_timeCounter == null) return;

        // Record game start time on first frame.
        if (_gameStartTime == 0f)
            _gameStartTime = _timeCounter.CurrentTime;

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

        // 24-hour deadline check: any incomplete task → Game Over, all complete → Win.
        if (!_deadlineChecked)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            float hoursElapsed = (_timeCounter.CurrentTime - _gameStartTime) / secondsPerHour;

            if (hoursElapsed >= 24f)
            {
                _deadlineChecked = true;
                CheckDeadline();
            }
        }
    }

    private void SpawnTasksForTeam(Team team, Task[] sprintTasks, int day)
    {
        var existing = team.Tasks ?? System.Array.Empty<Task>();
        var active = new List<Task>();

        // Stack/carry over existing unfinished tasks so day-1 tasks are not
        // dropped on day-2 (which would break dependency chains and deadlock).
        foreach (var t in existing)
        {
            if (t == null) continue;
            if (team.GetTaskStatus(t) != TaskStatus.COMPLETED)
                active.Add(t);
        }

        if (sprintTasks != null)
        {
            foreach (var t in sprintTasks)
            {
                if (t == null) continue;
                if (t.RoleForTask != team.TeamRole) continue; // role match only

                // Skip if this task is already assigned to the team.
                bool alreadyHas = false;
                foreach (var existingTask in existing)
                {
                    if (existingTask != null && existingTask.TaskId == t.TaskId)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
                if (alreadyHas) continue;

                if (team.GetTaskStatus(t) == TaskStatus.NOT_STARTED)
                    active.Add(t);
            }
        }

        team.SetTasks(active.ToArray());
    }

    /// <summary>Pause all active task coroutines (called when sleep starts).</summary>
    public void PauseAll() => _isPaused = true;

    /// <summary>Resume all active task coroutines (called when sleep ends).</summary>
    public void ResumeAll() => _isPaused = false;

    /// <summary>Returns all teams (standard + player) that can receive tasks.</summary>
    public Team[] GetAllTeams()
    {
        var all = new System.Collections.Generic.List<Team>();
        if (_teams != null)
            all.AddRange(_teams);
        if (_playerTeam != null)
            all.Add(_playerTeam);
        return all.ToArray();
    }

    /// <summary>Check if any assigned task is incomplete at the 24-hour deadline.</summary>
    private void CheckDeadline()
    {
        bool anyIncomplete = false;

        foreach (var team in GetAllTeams())
        {
            if (team == null || team.TeamCondition == TeamCondition.TERMINATED) continue;

            foreach (var task in team.Tasks)
            {
                if (task == null) continue;
                if (team.GetTaskStatus(task) != TaskStatus.COMPLETED)
                {
                    anyIncomplete = true;
                    break;
                }
            }

            if (anyIncomplete) break;
        }

        if (anyIncomplete)
            TriggerGameOver();
        else
            TriggerWin();
    }

    /// <summary>Load the Game Over scene.</summary>
    public void TriggerGameOver()
    {
        Debug.Log("[TaskRunner] Deadline missed — incomplete tasks remain. Game Over.");
        SceneSystem.Load(_gameOverSceneName);
    }

    /// <summary>Load the Win scene.</summary>
    public void TriggerWin()
    {
        Debug.Log("[TaskRunner] All tasks completed before deadline. Win!");
        SceneSystem.Load(_winSceneName);
    }

    /// <summary>Reset all state and reload the game scene from day 1 (used by Game Over Retry).</summary>
    public void ResetGameDay1()
    {
        Team.ResetTerminatedCount();
        Team.ResetAllRuntimeStatus();

        foreach (var team in GetAllTeams())
        {
            if (team == null) continue;
            team.SetTeamCondition(TeamCondition.ONLINE);

            foreach (var task in team.Tasks)
            {
                if (task != null)
                    team.SetTaskStatus(task, TaskStatus.NOT_STARTED);
            }
        }

        _gameStartTime = _timeCounter != null ? _timeCounter.CurrentTime : 0f;
        _deadlineChecked = false;

        // Reload the current game scene from day 1.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Called by TaskPopup when player clicks a variant button. Runs on this persistent object so the timer survives popup deactivation.</summary>
    public event System.Action<Task, Team> OnTaskCompleted;

    /// <summary>Called when a task starts. Passes Task + Team + isForced.</summary>
    public event System.Action<Task, Team, bool> OnTaskStarted;

    public bool StartTask(Task task, TaskVariant variant, Team team, EnergyStatus energySource = null, bool isForced = false)
    {
        if (task == null) return false;
        if (_activeTasks.ContainsKey(task)) return false;
        if (team == null) return false;
        if (!team.CanExecuteTask(task)) return false;

        var energy = energySource ?? _artsyEnergy;
        if (energy == null) return false;

        float cost = task.GetEnergyNeeded(variant);
        if (energy.CurrentValue < cost) return false;

        team.SetTaskStatus(task, TaskStatus.IN_PROGRESS);
        energy.AddValue(-cost);

        float seconds = task.GetTimeNeeded(variant) * _timeCounter.MinuteToSecondRatio;
        var entry = new ActiveTaskEntry
        {
            Team = team,
            Variant = variant,
            SecondsNeeded = seconds
        };
        var coroutine = StartCoroutine(WorkRoutine(task, variant, team, entry));
        entry.Coroutine = coroutine;
        _activeTasks[task] = entry;

        OnTaskStarted?.Invoke(task, team, isForced);
        return true;
    }

    private IEnumerator WorkRoutine(Task task, TaskVariant variant, Team team, ActiveTaskEntry entry)
    {
        float seconds = entry.SecondsNeeded;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            while (_isPaused) yield return null;

            // Founder executes 2x faster.
            float speedMultiplier = team.IsFounder ? 2f : 1f;
            elapsed += Time.deltaTime * speedMultiplier;
            team.SetTaskProgress(task, elapsed / seconds);
            yield return null;
        }

        CompleteTaskFromWorkRoutine(task, team, entry);
    }

    private void CompleteTaskFromWorkRoutine(Task task, Team team, ActiveTaskEntry entry)
    {
        team.SetTaskProgress(task, 1f);
        team.SetTaskStatus(task, TaskStatus.COMPLETED);

        if (_projectStatus != null)
            _projectStatus.AddValue(task.ProjectProgress);

        _activeTasks.Remove(task);
        OnTaskCompleted?.Invoke(task, team);
    }

    /// <summary>Simulate progress for all active tasks of a team over elapsed game hours.</summary>
    public void SimulateTaskProgressForSleep(Team team, float elapsedHours)
    {
        if (team == null) return;

        float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
        float secondsToAdvance = elapsedHours * secondsPerHour;

        var toComplete = new List<Task>();
        foreach (var kvp in _activeTasks)
        {
            if (kvp.Value.Team != team) continue;

            float progressIncrement = secondsToAdvance / kvp.Value.SecondsNeeded;
            float currentProgress = team.GetTaskProgress(kvp.Key);
            float newProgress = Mathf.Clamp01(currentProgress + progressIncrement);
            team.SetTaskProgress(kvp.Key, newProgress);

            if (newProgress >= 1f)
                toComplete.Add(kvp.Key);
        }

        foreach (var task in toComplete)
        {
            if (!_activeTasks.TryGetValue(task, out var entry)) continue;
            team.SetTaskProgress(task, 1f);
            team.SetTaskStatus(task, TaskStatus.COMPLETED);
            if (_projectStatus != null)
                _projectStatus.AddValue(task.ProjectProgress);
            _activeTasks.Remove(task);
            OnTaskCompleted?.Invoke(task, entry.Team);
        }
    }
}
