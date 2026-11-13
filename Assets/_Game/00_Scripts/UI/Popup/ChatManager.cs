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
        AddLine(team.Name, $"finished {task.TaskName}.");
    }

    private void OnTaskStarted(Task task, Team team, bool isForced)
    {
        if (task == null || team == null) return;
        if (isForced)
            AddLine(team.Name, $"was forced to work on {task.TaskName}.");
        else
            AddLine(team.Name, $"started working on {task.TaskName}.");
    }

    private void OnInformationGained(Information info)
    {
        if (info == null) return;
        foreach (var team in FindObjectsByType<Team>(FindObjectsSortMode.None))
        {
            if (team.HasInformation(info.InformationId))
            {
                AddLine(team.Name, $"learned {info.InformationName}.");
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
}
