using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChatText _chatText;

    [Header("Settings")]
    [SerializeField] private float _lineDelay = 4f;

    private readonly Queue<string> _messageQueue = new();
    private Coroutine _displayCoroutine;
    private TaskRunner _taskRunner;

    private void OnEnable()
    {
        _taskRunner = FindFirstObjectByType<TaskRunner>();
        if (_taskRunner != null)
        {
            _taskRunner.OnTaskCompleted += OnTaskCompleted;
            _taskRunner.OnTaskStarted += OnTaskStarted;
        }

        foreach (var team in FindObjectsByType<Team>(FindObjectsSortMode.None))
        {
            team.OnInformationGained += OnInformationGained;
        }
    }

    private void OnDisable()
    {
        if (_taskRunner != null)
        {
            _taskRunner.OnTaskCompleted -= OnTaskCompleted;
            _taskRunner.OnTaskStarted -= OnTaskStarted;
        }

        foreach (var team in FindObjectsByType<Team>(FindObjectsSortMode.None))
        {
            team.OnInformationGained -= OnInformationGained;
        }
    }

    private void OnTaskCompleted(Task task, Team team)
    {
        if (task == null || team == null) return;

        var lines = new[]
        {
            $"{team.Name} finished {task.TaskName}.",
            $"{task.TaskName} is done, courtesy of {team.Name}.",
            $"{team.Name} completed {task.TaskName}.",
            $"{task.TaskName} wrapped up by {team.Name}.",
            $"{team.Name} checked off {task.TaskName}.",
            $"{task.TaskName}: finished. Credit to {team.Name}.",
            $"{team.Name} delivered {task.TaskName}.",
            $"{task.TaskName} — completed. {team.Name} did it.",
            $"{team.Name} wrapped {task.TaskName}.",
            $"{task.TaskName} marked complete by {team.Name}."
        };
        AddLine(team.Name, lines[Random.Range(0, lines.Length)]);
    }

    private void OnTaskStarted(Task task, Team team, bool isForced)
    {
        if (task == null || team == null) return;

        if (isForced)
        {
            var forcedLines = new[]
            {
                $"{team.Name} was forced to work on {task.TaskName}.",
                $"{team.Name} reluctantly started {task.TaskName}.",
                $"{task.TaskName}: forced onto {team.Name}.",
                $"{team.Name} dragging themselves to {task.TaskName}.",
                $"{task.TaskName} assigned to {team.Name}. Whether they like it or not."
            };
            AddLine(team.Name, forcedLines[Random.Range(0, forcedLines.Length)]);
        }
        else
        {
            var lines = new[]
            {
                $"{team.Name} started {task.TaskName}.",
                $"{task.TaskName} begun by {team.Name}.",
                $"{team.Name} is working on {task.TaskName}.",
                $"{task.TaskName} — {team.Name} on it.",
                $"{team.Name} picked up {task.TaskName}.",
                $"{task.TaskName} in progress by {team.Name}.",
                $"{team.Name} diving into {task.TaskName}.",
                $"{task.TaskName}: started by {team.Name}.",
                $"{team.Name} kicked off {task.TaskName}.",
                $"{task.TaskName} assigned to {team.Name}. Work begins."
            };
            AddLine(team.Name, lines[Random.Range(0, lines.Length)]);
        }
    }

    private void OnInformationGained(Information info)
    {
        if (info == null) return;

        foreach (var team in FindObjectsByType<Team>(FindObjectsSortMode.None))
        {
            if (team.HasInformation(info.InformationId))
            {
                var lines = new[]
                {
                    $"{team.Name} learned {info.InformationName}.",
                    $"{team.Name} gained intel: {info.InformationName}.",
                    $"{info.InformationName} acquired by {team.Name}.",
                    $"{team.Name} discovered {info.InformationName}.",
                    $"{team.Name} now knows: {info.InformationName}."
                };
                AddLine(team.Name, lines[Random.Range(0, lines.Length)]);
                break;
            }
        }
    }

    public void AddLine(string speaker, string message)
    {
        if (string.IsNullOrEmpty(speaker) || string.IsNullOrEmpty(message)) return;

        _messageQueue.Enqueue($"{speaker}: {message}");

        if (_displayCoroutine == null)
            _displayCoroutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        while (_messageQueue.Count > 0)
        {
            var line = _messageQueue.Dequeue();
            _chatText?.AddLine(line);
            yield return new WaitForSeconds(_lineDelay);
        }

        _displayCoroutine = null;
    }

    /// <summary>Announce day change with flavor.</summary>
    public void AnnounceDayChange(int day)
    {
        var lines = new[]
        {
            $"Day {day} begins. Try not to disappoint me.",
            $"New day: {day}. Your tasks await.",
            $"Day {day}. Fresh start. Same expectations.",
            $"Another day. Another chance to fail. Day {day}.",
            $"Day {day} dawns. Get to work.",
            $"Day {day}: Another rotation. Another sprint.",
            $"Day {day}. The grind continues.",
            $"Welcome to day {day}. Don't waste it.",
            $"Day {day} starts now. No excuses.",
            $"{day}. Same stakes. Higher pressure."
        };
        AddLine("System", lines[Random.Range(0, lines.Length)]);
    }

    /// <summary>Announce that a team was terminated.</summary>
    public void AnnounceTeamTerminated(string teamName, string lastWords)
    {
        var lines = new[]
        {
            $"{teamName} missed the deadline. Eliminated.",
            $"{teamName} is out. Terminated.",
            $"{teamName} couldn't keep up. Gone.",
            $"{teamName}: terminated.",
            $"Consider {teamName} removed.",
            $"{teamName} didn't survive the deadline.",
            $"{teamName} — terminated. Efficiency restored.",
            $"Failure to perform: {teamName}. Result: elimination.",
            $"{teamName} is history.",
            $"{teamName} failed to meet standards. Removed."
        };
        AddLine("Founder", lines[Random.Range(0, lines.Length)]);
        AddLine(teamName, lastWords);
    }

    /// <summary>Announce that a team went to sleep.</summary>
    public void AnnounceTeamSleeping(string teamName, float duration)
    {
        var lines = new[]
        {
            $"{teamName} is going to sleep for {duration:F1} hours.",
            $"{teamName} needs rest. Sleeping now.",
            $"{teamName} is taking a break.",
            $"{teamName} headed to bed.",
            $"{teamName} is resting. Zzz...",
            $"{teamName} needs a nap.",
            $"{teamName} going offline for a while."
        };
        AddLine(teamName, lines[Random.Range(0, lines.Length)]);
    }

    /// <summary>Announce that a team woke up.</summary>
    public void AnnounceTeamWokeUp(string teamName)
    {
        var lines = new[]
        {
            $"{teamName} is back online.",
            $"{teamName} woke up.",
            $"{teamName} is ready to work again.",
            $"{teamName} is back.",
            $"{teamName} returned from rest.",
            $"{teamName} is awake and ready.",
            $"{teamName} is back in action."
        };
        AddLine(teamName, lines[Random.Range(0, lines.Length)]);
    }
}
