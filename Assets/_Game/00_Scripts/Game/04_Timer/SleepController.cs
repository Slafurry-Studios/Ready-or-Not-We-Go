using UnityEngine;
using UnityEngine.Events;

public class SleepController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private PendulumMiniGame _miniGame;

    [Header("Sleep Settings")]
    [SerializeField] private float _normalSleepMultiplier = 4f;
    [SerializeField] private float _deepSleepMultiplier = 8f;
    [SerializeField] private float _deepSleepWakeUpEnergy = 80f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onWakeUp;
    [SerializeField] private UnityEvent _onDeepSleepEnded;

    private bool _isSleeping;
    private bool _isDeepSleeping;

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

        _energyStatus.SetSleeping(true);
        _timeCounter.SetTimeMultiplier(_deepSleepMultiplier);

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

        _onWakeUp?.Invoke();

        if (wasDeepSleeping)
            _onDeepSleepEnded?.Invoke();
    }

    private void CheckEnergy(float currentEnergy, float maxEnergy)
    {
        if (!_isSleeping)
            return;

        // Deep sleep berakhir saat energy mencapai 80.
        if (_isDeepSleeping &&
            currentEnergy >= _deepSleepWakeUpEnergy)
        {
            EndSleep();
            return;
        }

        // Tidur normal otomatis berakhir saat energy penuh.
        if (!_isDeepSleeping && currentEnergy >= maxEnergy)
        {
            EndSleep();
        }
    }
}