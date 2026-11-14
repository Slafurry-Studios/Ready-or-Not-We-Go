using UnityEngine;
using UnityEngine.Events;
using Slafurry.Utils.UI;

public class SleepController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private PendulumMiniGame _miniGame;
    [SerializeField] private UIFade _fadePanel;
    [SerializeField] private TaskRunner _taskRunner;
    [SerializeField] private ActionRunner _actionRunner;

    [Header("Sleep Settings")]
    [SerializeField] private float _normalSleepMultiplier = 4f;
    [SerializeField] private float _deepSleepMultiplier = 8f;
    [SerializeField] private float _deepSleepWakeUpEnergy = 90f;
    [SerializeField] private float _minDeepSleepHours = 2f;
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onWakeUp;
    [SerializeField] private UnityEvent _onDeepSleepEnded;

    private bool _isSleeping;
    private bool _isDeepSleeping;
    private float _sleepStartTime;

    public bool IsSleeping => _isSleeping;
    public bool IsDeepSleeping => _isDeepSleeping;

    private void OnEnable()
    {
        if (_energyStatus != null)
            _energyStatus.OnValueChanged += CheckEnergy;
    }

    private void OnDisable()
    {
        if (_energyStatus != null)
            _energyStatus.OnValueChanged -= CheckEnergy;
    }

    public void StartSleep()
    {
        if (_timeCounter == null || _energyStatus == null)
            return;

        // If energy is already full, there is no need to sleep — end immediately
        // without starting the mini game or skipping any time.
        if (_energyStatus.CurrentValue >= _energyStatus.MaxValue)
        {
            EndSleep();
            return;
        }

        _isSleeping = true;
        _isDeepSleeping = false;
        _sleepStartTime = _timeCounter.CurrentTime;

        _energyStatus.SetSleeping(true);
        _timeCounter.SetTimeMultiplier(_normalSleepMultiplier);

        // Pause tasks and actions while sleeping; they resume on wake up.
        _taskRunner?.PauseAll();
        _actionRunner?.PauseAll();

        // Set non-player teams to INACTIVE while sleeping.
        if (_taskRunner != null)
        {
            Team playerTeam = _taskRunner.PlayerTeam;
            foreach (var team in _taskRunner.GetAllTeams())
            {
                if (team == null || team == playerTeam) continue;
                if (team.TeamCondition == TeamCondition.TERMINATED) continue;
                team.SetTeamCondition(TeamCondition.INACTIVE);
            }

            var chat = FindFirstObjectByType<ChatManager>();
            if (chat != null)
                chat.AddLine("System", "Non-player teams are now offline while you sleep.");
        }

        if (_miniGame != null)
            _miniGame.StartGame();

        CheckEnergy(
            _energyStatus.CurrentValue,
            _energyStatus.MaxValue
        );
    }

    public bool StartDeepSleep()
    {
        if (_timeCounter == null || _energyStatus == null)
            return false;

        // Do not enter deep sleep if energy is already above the wake-up threshold.
        if (_energyStatus.CurrentValue > _deepSleepWakeUpEnergy)
        {
            Debug.Log("Energy already above wake-up threshold; skipping deep sleep.");
            return false;
        }

        _isSleeping = true;
        _isDeepSleeping = true;
        _sleepStartTime = _timeCounter.CurrentTime;

        // Resolve the fade panel automatically if not assigned in the Inspector.
        if (_fadePanel == null)
            _fadePanel = FindFirstObjectByType<UIFade>();

        _energyStatus.SetSleeping(true);
        _timeCounter.SetTimeMultiplier(_deepSleepMultiplier);

        // Pause tasks and actions while sleeping; they resume on wake up.
        _taskRunner?.PauseAll();
        _actionRunner?.PauseAll();

        // Set non-player teams to INACTIVE while sleeping.
        if (_taskRunner != null)
        {
            Team playerTeam = _taskRunner.PlayerTeam;
            foreach (var team in _taskRunner.GetAllTeams())
            {
                if (team == null || team == playerTeam) continue;
                if (team.TeamCondition == TeamCondition.TERMINATED) continue;
                team.SetTeamCondition(TeamCondition.INACTIVE);
            }

            var chat = FindFirstObjectByType<ChatManager>();
            if (chat != null)
                chat.AddLine("System", "Non-player teams are now offline while you sleep.");
        }

        // Fade in the black panel when deep sleep begins.
        if (_fadePanel != null)
        {
            _fadePanel.SetImmediate(0f);
            _fadePanel.FadeIn(_fadeDuration);
        }

        Debug.Log("Character entered deep sleep!");

        CheckEnergy(
            _energyStatus.CurrentValue,
            _energyStatus.MaxValue
        );

        return true;
    }

    public void EndSleep()
    {
        if (!_isSleeping)
            return;

        bool wasDeepSleeping = _isDeepSleeping;

        // Calculate elapsed game hours during sleep for other-AI progress simulation.
        float elapsedHours = 0f;
        if (_timeCounter != null)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            elapsedHours = (_timeCounter.CurrentTime - _sleepStartTime) / secondsPerHour;
        }

        // Let non-player AI characters progress on tasks/actions during sleep.
        if (elapsedHours > 0f && _taskRunner != null)
        {
            Team playerTeam = _taskRunner.PlayerTeam;
            foreach (var team in _taskRunner.GetAllTeams())
            {
                if (team == null || team == playerTeam) continue;
                if (team.TeamCondition == TeamCondition.TERMINATED) continue;
                if (team.IsSleeping) continue;
                _taskRunner.SimulateTaskProgressForSleep(team, elapsedHours);
                _actionRunner?.SimulateActionProgressForSleep(team, elapsedHours);
            }
        }

        _isSleeping = false;
        _isDeepSleeping = false;

        // Hide the mini game UI after sleep ends.
        _miniGame?.Hide();

        _energyStatus.SetSleeping(false);
        _timeCounter.SetTimeMultiplier(1f);

        // Resume tasks and actions that were paused during sleep.
        _taskRunner?.ResumeAll();
        _actionRunner?.ResumeAll();

        // Set non-player teams back to ONLINE after sleep.
        if (_taskRunner != null)
        {
            Team playerTeam = _taskRunner.PlayerTeam;
            foreach (var team in _taskRunner.GetAllTeams())
            {
                if (team == null || team == playerTeam) continue;
                if (team.TeamCondition == TeamCondition.TERMINATED) continue;
                team.SetTeamCondition(TeamCondition.ONLINE);
            }

            var chat = FindFirstObjectByType<ChatManager>();
            if (chat != null)
                chat.AddLine("System", "Non-player teams are back online after sleep.");
        }

        Debug.Log("Character woke up!");

        // Fade out the black panel after deep sleep ends.
        if (wasDeepSleeping && _fadePanel != null)
            _fadePanel.FadeOut(_fadeDuration);

        _onWakeUp?.Invoke();

        if (wasDeepSleeping)
            _onDeepSleepEnded?.Invoke();
    }

    private void CheckEnergy(float currentEnergy, float maxEnergy)
    {
        if (!_isSleeping)
            return;

        // Deep sleep ends when energy reaches 90 AND at least 2 hours have passed.
        // No maximum cap — sleep continues until energy recovers (min 2 hours).
        if (_isDeepSleeping)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            float elapsedHours = (_timeCounter.CurrentTime - _sleepStartTime) / secondsPerHour;

            bool energyReached = currentEnergy >= _deepSleepWakeUpEnergy;
            bool minTimeReached = elapsedHours >= _minDeepSleepHours;

            if (energyReached && minTimeReached)
            {
                EndSleep();
                return;
            }
        }

        // Normal sleep automatically ends when energy is full.
        if (!_isDeepSleeping && currentEnergy >= maxEnergy)
        {
            EndSleep();
        }
    }
}
