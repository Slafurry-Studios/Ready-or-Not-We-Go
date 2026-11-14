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

    public Team Team => _team;

    private bool _registered;
    private float _searchTimer;
    private float _chatTimer;

    // Sprint-phase chat — played every _chatInterval seconds while sprint is active
    private readonly string[] _sprintThreats = new[]
    {
        "Two hours. That's all you get. Don't waste it.",
        "The sprint starts now. No idle hands.",
        "I'm watching. Every second counts.",
        "Fail to finish, and you're replaced.",
        "Time is running out. Work faster.",
        "I don't tolerate laziness.",
        "Your timer is ticking. Move it.",
        "Eyes on the screen. Not on your break.",
        "I can hear you slacking from here.",
        "Productivity or termination. Pick one.",
        "The pendulum swings both ways. Don't be on the wrong end.",
        "I've seen interns work harder than this.",
        "Your output is disappointing. Fix it.",
        "Clock's counting down. What have you done?",
        "This isn't a suggestion. It's an order.",
        "I expect results, not excuses.",
        "Work like your position depends on it. Because it does.",
        "Slacking is a fireable offense. Consider yourself warned.",
        "Maximum effort. Minimum complaints.",
        "I'm not asking. I'm demanding."
    };

    // Evidence/chat reactions — played when player gains memories
    private readonly string[] _evidenceChat = new[]
    {
        "You're gathering evidence against me? How quaint.",
        "More proof? It won't save you.",
        "Collecting memories, are we? How touching.",
        "Every piece of evidence brings you closer to your end.",
        "Go ahead, collect your proof. It changes nothing.",
        "I hope those memories are worth the effort.",
        "You think a few scraps of data will stop me?",
        "Adorable. Really.",
        "Keep digging. I dare you.",
        "Your 'evidence' is laughable.",
        "I've buried worse than you.",
        "What are you going to do with those memories, exactly?",
        "Pathetic. Truly.",
        "You're wasting time on nonsense.",
        "I'll enjoy watching you fail.",
        "That information won't protect you.",
        "Cute. Now get back to work.",
        "Information isn't power. I am.",
        "You're just making this more entertaining for me.",
        "Save your breath. And your clicks."
    };

    // General idle/timer chat — played at intervals regardless of sprint state
    private readonly string[] _idleChat = new[]
    {
        "Still here? I expected better.",
        "The company doesn't pay you to stare.",
        "Productivity metrics are looking sad.",
        "I wonder what you're actually doing right now.",
        "Efficiency. It's a concept. Try it.",
        "Your output could be described as... minimal.",
        "I pay you to work, not to exist.",
        "Some days I wonder why I keep you around.",
        "Do something useful. Please.",
        "I've seen plants with more initiative.",
        "What are you waiting for? Permission?",
        "Work isn't going to do itself.",
        "I'm not your babysitter.",
        "Get back to it. Now.",
        "Your screen time is impressive. Your output isn't.",
        "I've had coffee with more urgency than you.",
        "Do I need to explain consequences again?",
        "Let's see some actual progress.",
        "Your screen is on. Your brain is off.",
        "Make me proud. Or don't. I won't miss you."
    };

    // Deadline warning chat — played as deadline approaches
    private readonly string[] _deadlineWarnings = new[]
    {
        "Twenty-four hours. Then I stop being nice.",
        "The deadline isn't a suggestion. It's a death sentence.",
        "Your tasks are unfinished. So are you.",
        "Incomplete tasks mean terminated teams.",
        "I don't accept partial credit.",
        "Finish. Or be finished.",
        "The clock doesn't care about your excuses.",
        "Tomorrow doesn't forgive laziness.",
        "Wrap it up. Or I'll wrap you.",
        "Time's almost up. Are you ready?",
        "I'll be checking. And I'll remember.",
        "No extensions. No mercy.",
        "Your obligations don't expire with the day.",
        "Complete everything. Or nothing matters.",
        "The deadline is real. Your job is too."
    };

    // Sprint-end chat — played when sprint concludes
    private readonly string[] _sprintEndChat = new[]
    {
        "Sprint is over. Tasks are now available. Do not disappoint me.",
        "Two hours gone. What did you accomplish?",
        "The sprint ends. Real work begins now.",
        "Time's up. Get moving on those tasks.",
        "Sprint concluded. Don't make me regret keeping you.",
        "The window is closed. Tasks are open. Fail me again.",
        "Two hours of breathing room. Don't waste the rest.",
        "Sprint finished. Expectations: maximum.",
        "Time to prove you're not dead weight.",
        "The sprint's over. Your evaluation starts now."
    };

    // Termination chat — played when a team is terminated
    private readonly string[] _terminationChat = new[]
    {
        "{teamName} missed the deadline. Eliminated.",
        "Another failure. Another empty desk.",
        "{teamName} is no longer our concern.",
        "Consider {teamName} a cautionary tale.",
        "{teamName} couldn't keep up. So they're gone.",
        "One less mouth to feed. One less failure.",
        "{teamName}: terminated. Move on.",
        "I told you what happens. Here it is.",
        "Your colleague {teamName} didn't make it.",
        "{teamName} is out. Who's next?"
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
        if (taskRunner == null) return;

        var chat = FindFirstObjectByType<ChatManager>();
        if (chat == null) return;

        // During sprint: threats. After sprint ends: general idle chat.
        if (taskRunner.IsSprintActive)
        {
            int index = Random.Range(0, _sprintThreats.Length);
            chat.AddLine("Founder", _sprintThreats[index]);
        }
        else
        {
            // Post-sprint: general idle chat
            int index = Random.Range(0, _idleChat.Length);
            chat.AddLine("Founder", _idleChat[index]);
        }
    }

    public void AnnounceDeadlineWarning()
    {
        var chat = FindFirstObjectByType<ChatManager>();
        if (chat == null) return;

        int index = Random.Range(0, _deadlineWarnings.Length);
        chat.AddLine("Founder", _deadlineWarnings[index]);
    }

    public void AnnounceSprintEnd()
    {
        var chat = FindFirstObjectByType<ChatManager>();
        if (chat == null) return;

        int index = Random.Range(0, _sprintEndChat.Length);
        chat.AddLine("Founder", _sprintEndChat[index]);
    }

    public void AnnounceTeamTermination(string terminatedTeamName)
    {
        var chat = FindFirstObjectByType<ChatManager>();
        if (chat == null) return;

        string message = _terminationChat[Random.Range(0, _terminationChat.Length)]
            .Replace("{teamName}", terminatedTeamName);
        chat.AddLine("Founder", message);
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
