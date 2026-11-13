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

    [Header("Sleep Settings")]
    [SerializeField] private float _normalSleepMultiplier = 4f;
    [SerializeField] private float _deepSleepMultiplier = 8f;
    [SerializeField] private float _deepSleepWakeUpEnergy = 90f;
    [SerializeField] private float _maxDeepSleepHours = 10f;
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onWakeUp;
    [SerializeField] private UnityEvent _onDeepSleepEnded;

    private bool _isSleeping;
    private bool _isDeepSleeping;
    private float _deepSleepStartTime;

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

        _isSleeping = true;
        _isDeepSleeping = false;

        _energyStatus.SetSleeping(true);
        _timeCounter.SetTimeMultiplier(_normalSleepMultiplier);

        if (_miniGame != null)
            _miniGame.StartGame();

        CheckEnergy(
            _energyStatus.CurrentValue,
            _energyStatus.MaxValue
        );
    }

    public void StartDeepSleep()
    {
        if (_timeCounter == null || _energyStatus == null)
            return;

        _isSleeping = true;
        _isDeepSleeping = true;
        _deepSleepStartTime = _timeCounter.CurrentTime;

        // Resolve the fade panel automatically if not assigned in the Inspector.
        if (_fadePanel == null)
            _fadePanel = FindFirstObjectByType<UIFade>();

        _energyStatus.SetSleeping(true);
        _timeCounter.SetTimeMultiplier(_deepSleepMultiplier);

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
    }

    public void EndSleep()
    {
        if (!_isSleeping)
            return;

        bool wasDeepSleeping = _isDeepSleeping;

        _isSleeping = false;
        _isDeepSleeping = false;

        _energyStatus.SetSleeping(false);
        _timeCounter.SetTimeMultiplier(1f);

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

        // Deep sleep ends when energy reaches 90, or the maximum sleep
        // duration (8-10 hours) has passed, whichever comes first.
        if (_isDeepSleeping)
        {
            float secondsPerHour = 60f * _timeCounter.MinuteToSecondRatio;
            float elapsedHours = (_timeCounter.CurrentTime - _deepSleepStartTime) / secondsPerHour;

            bool energyReached = currentEnergy >= _deepSleepWakeUpEnergy;
            bool maxTimeReached = elapsedHours >= _maxDeepSleepHours;

            if (energyReached || maxTimeReached)
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
