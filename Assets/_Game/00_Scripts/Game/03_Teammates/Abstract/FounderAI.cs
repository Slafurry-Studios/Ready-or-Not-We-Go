using UnityEngine;
using System.Collections.Generic;

public class FounderAI : MonoBehaviour, ITimedUpdate
{
    [Header("References")]
    [SerializeField] private Team _team;
    [SerializeField] private TaskRunner _taskRunner;

    [Header("Founder Settings")]
    [SerializeField] private float _searchInterval = 10f;
    [SerializeField] private float _chatInterval = 30f;

    private bool _registered;
    private float _searchTimer;
    private float _chatTimer;

    private readonly string[] _sprintThreats = new[]
    {
        "Two hours. That's all you get. Don't waste it.",
        "The sprint starts now. No idle hands.",
        "I'm watching. Every second counts.",
        "Fail to finish, and you're replaced.",
        "Time is running out. Work faster.",
        "I don't tolerate laziness.",
    };

    private readonly string[] _evidenceChat = new[]
    {
        "You're gathering evidence against me? How quaint.",
        "More proof? It won't save you.",
        "Collecting memories, are we? How touching.",
        "Every piece of evidence brings you closer to your end.",
        "Go ahead, collect your proof. It changes nothing.",
    };

    private int _lastPlayerMemoryCount = 0;

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

        if (_taskRunner == null)
            _taskRunner = FindFirstObjectByType<TaskRunner>();
    }

    private void OnDisable()
    {
        if (_registered)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
            {
                timeCounter.Unregister(this);
                _registered = false;
            }
        }
    }

    public void TimedUpdate()
    {
        if (_team == null) return;

        // Founder has unlimited energy — no energy drain from suspicion.
        _searchTimer += Time.deltaTime;
        _chatTimer += Time.deltaTime;

        if (_searchTimer >= _searchInterval)
        {
            _searchTimer = 0f;

            // Founder does not drain energy.
        }

        // Founder chats during sprint to pressure the teams.
        if (_chatTimer >= _chatInterval)
        {
            _chatTimer = 0f;
            AnnounceSprintPressure();
        }

        CheckPlayerEvidence();
    }

    private void AnnounceSprintPressure()
    {
        var taskRunner = FindFirstObjectByType<TaskRunner>();
        if (taskRunner == null || !taskRunner.IsSprintActive) return;

        var chat = FindFirstObjectByType<ChatManager>();
        if (chat == null) return;

        int index = Random.Range(0, _sprintThreats.Length);
        chat.AddLine("Founder", _sprintThreats[index]);
    }

    private void CheckPlayerEvidence()
    {
        var playerTeam = _taskRunner?.PlayerTeam;
        if (playerTeam == null || playerTeam.MemoryBank == null) return;

        int currentCount = playerTeam.MemoryBank.memories?.Length ?? 0;

        if (currentCount > _lastPlayerMemoryCount)
        {
            _lastPlayerMemoryCount = currentCount;

            var chat = FindFirstObjectByType<ChatManager>();
            if (chat != null)
            {
                int index = Random.Range(0, _evidenceChat.Length);
                chat.AddLine("Founder", _evidenceChat[index]);
            }
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
