using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Slafurry.Systems.Scene;
using Slafurry.Systems.Save;

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
    [SerializeField] private SceneReference _gameOverScene;
    [SerializeField] private SceneReference _winScene;
    [SerializeField] private SceneReference _mainMenuScene;

    [Header("Sprint")]
    [SerializeField] private float _sprintDurationHours = 2f;

    private int _lastDay = -1;
    private float _gameStartTime;
    private bool _deadlineChecked;
    private bool _sprintEnded;
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

    /// <summary>True while the sprint is active and tasks are locked.</summary>
    public bool IsSprintActive => !_sprintEnded;

    public event System.Action OnSprintEnded;

    private void Update()
    {
        if (_timeCounter == null) return;

        // Record game start time on first frame, loading a save if one exists.
        if (_gameStartTime == 0f)
        {
            if (HasSave() && LoadGame())
            {
                // Save already set _gameStartTime, _lastDay, _deadlineChecked.
            }
            else
            {
                _gameStartTime = _timeCounter.CurrentTime;
            }
        }

        // Sprint timer: tasks locked for the first _sprintDurationHours.
        if (!_sprintEnded)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            float hoursElapsed = (_timeCounter.CurrentTime - _gameStartTime) / secondsPerHour;

            if (hoursElapsed >= _sprintDurationHours)
            {
                _sprintEnded = true;
                OnSprintEnded?.Invoke();
                AnnounceSprintEnd();
            }
        }

        int day = _timeCounter.CurrentDay;
        if (day == _lastDay) return;
        _lastDay = day;

        // Announce day change with flavor chat
        var chat = FindFirstObjectByType<ChatManager>();
        chat?.AnnounceDayChange(day);

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

        // 24-hour deadline: non-player teams with incomplete tasks → terminated by Founder.
        // Player team incomplete → Game Over. All complete → Win.
        if (!_deadlineChecked)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            float hoursElapsed = (_timeCounter.CurrentTime - _gameStartTime) / secondsPerHour;

            if (hoursElapsed >= 24f)
            {
                _deadlineChecked = true;

                // Founder warns before the deadline hits
                var founder = FindFirstObjectByType<FounderAI>();
                founder?.AnnounceDeadlineWarning();

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

    /// <summary>Check if any assigned task is incomplete at the 24-hour deadline.
    /// Non-player teams with incomplete tasks → terminated by Founder, tasks inherited.
    /// Player team incomplete → Game Over. All complete → Win.</summary>
    private void CheckDeadline()
    {
        bool playerIncomplete = false;

        foreach (var team in GetAllTeams())
        {
            if (team == null || team.TeamCondition == TeamCondition.TERMINATED) continue;

            bool hasIncomplete = false;
            foreach (var task in team.Tasks)
            {
                if (task == null) continue;
                if (team.GetTaskStatus(task) != TaskStatus.COMPLETED)
                {
                    hasIncomplete = true;
                    break;
                }
            }

            if (hasIncomplete)
            {
                if (team == _playerTeam)
                {
                    playerIncomplete = true;
                }
                else
                {
                    TerminateTeam(team);
                }
            }
        }

        if (playerIncomplete)
            TriggerGameOver();
        else if (AllTasksComplete())
            TriggerWin();
        // If player survived but tasks remain, game continues with inherited tasks.
    }

    /// <summary>Terminate a team for missing the deadline. Founder inherits their unfinished tasks.</summary>
    private void TerminateTeam(Team team)
    {
        if (team == null) return;

        team.Terminate();

        var founder = FindFirstObjectByType<FounderAI>();
        if (founder != null && founder.Team != team)
        {
            founder.InheritTasksFrom(team);
            founder.AnnounceTeamTermination(team.Name);
        }
    }

    /// <summary>True if every task across every team is COMPLETED.</summary>
    private bool AllTasksComplete()
    {
        foreach (var team in GetAllTeams())
        {
            if (team == null || team.TeamCondition == TeamCondition.TERMINATED) continue;

            foreach (var task in team.Tasks)
            {
                if (task == null) continue;
                if (team.GetTaskStatus(task) != TaskStatus.COMPLETED)
                    return false;
            }
        }
        return true;
    }

    private void AnnounceSprintEnd()
    {
        var founder = FindFirstObjectByType<FounderAI>();
        founder?.AnnounceSprintEnd();

        Debug.Log("[TaskRunner] Sprint ended — tasks unlocked.");
    }

    /// <summary>Load the Game Over scene.</summary>
    public void TriggerGameOver()
    {
        Debug.Log("[TaskRunner] Deadline missed — player failed. Game Over.");
        SceneSystem.Load(_gameOverScene);
    }

    /// <summary>Load the Win scene.</summary>
    public void TriggerWin()
    {
        Debug.Log("[TaskRunner] All tasks completed before deadline. Win!");
        SceneSystem.Load(_winScene);
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

    /// <summary>Save current game state to a file.</summary>
    public void SaveGame(string fileName = "gamesave")
    {
        if (_timeCounter == null) return;

        var data = new GameSaveData
        {
            currentDay = _timeCounter.CurrentDay,
            currentTimeSeconds = _timeCounter.CurrentTime,
            gameStartTimeSeconds = _gameStartTime,
            sprintEnded = _sprintEnded,
            terminatedCount = Team.GetTerminatedCount()
        };

        // Collect completed task IDs and terminated team IDs from all teams.
        foreach (var team in GetAllTeams())
        {
            if (team == null) continue;

            data.completedTaskIds.AddRange(team.GetCompletedTaskIds());

            if (team.TeamCondition == TeamCondition.TERMINATED)
                data.terminatedTeamIds.Add(team.ID);
        }

        Save.To(data, fileName);
        Debug.Log($"[TaskRunner] Game saved to '{fileName}' (day {data.currentDay}, time {data.currentTimeSeconds:F0}s).");
    }

    /// <summary>Load game state from a file. Returns true if a save was found and loaded.</summary>
    public bool LoadGame(string fileName = "gamesave")
    {
        if (_timeCounter == null) return false;

        if (!Save.Exists(fileName))
        {
            Debug.Log($"[TaskRunner] No save found at '{fileName}'.");
            return false;
        }

        var data = Save.From<GameSaveData>(fileName, new GameSaveData());

        // Restore time.
        _timeCounter.SetCurrentTime(data.currentTimeSeconds);
        _gameStartTime = data.gameStartTimeSeconds;

        // Restore terminated count.
        Team.SetTerminatedCount(data.terminatedCount);

        // Restore sprint state.
        _sprintEnded = data.sprintEnded;

        // Restore team conditions and task statuses.
        foreach (var team in GetAllTeams())
        {
            if (team == null) continue;

            team.SetTeamCondition(
                data.terminatedTeamIds.Contains(team.ID)
                    ? TeamCondition.TERMINATED
                    : TeamCondition.ONLINE
            );
            team.RestoreTaskStatuses(data.completedTaskIds);
        }

        // Prevent re-spawning tasks on the loaded day.
        _lastDay = _timeCounter.CurrentDay;

        // Deadline already evaluated in the saved state.
        _deadlineChecked = _timeCounter.CurrentTime - _gameStartTime >= 24f * 60f * _timeCounter.MinuteToSecondRatio;

        Debug.Log(
            $"[TaskRunner] Game loaded from '{fileName}' (day {data.currentDay}, " +
            $"time {data.currentTimeSeconds:F0}s, terminated={data.terminatedCount})."
        );
        return true;
    }

    /// <summary>Check if a save file exists.</summary>
    public bool HasSave(string fileName = "gamesave") => Save.Exists(fileName);

    /// <summary>Called by TaskPopup when player clicks a variant button. Runs on this persistent object so the timer survives popup deactivation.</summary>
    public event System.Action<Task, Team> OnTaskCompleted;

    /// <summary>Called when a task starts. Passes Task + Team + isForced.</summary>
    public event System.Action<Task, Team, bool> OnTaskStarted;

    public bool StartTask(Task task, TaskVariant variant, Team team, EnergyStatus energySource = null, bool isForced = false)
    {
        if (task == null) return false;
        if (_activeTasks.ContainsKey(task)) return false;
        if (team == null) return false;
        if (IsSprintActive) return false; // Tasks locked during sprint
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
