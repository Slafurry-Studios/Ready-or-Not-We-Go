using UnityEngine;

public class EnergyStatus : Status, ITimedUpdate
{
    [Header("Energy Settings")]
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private float _energyConsumptionRate = 0.2f;
    [SerializeField] private float _energyRecoveryRate = 0.5f;

    private bool _isSleeping;

    private void OnEnable()
    {
        if (_timeCounter != null)
            _timeCounter.Register(this);
    }

    private void OnDisable()
    {
        if (_timeCounter != null)
            _timeCounter.Unregister(this);
    }

    public void SetSleeping(bool sleeping)
    {
        _isSleeping = sleeping;
    }

    public void TimedUpdate()
    {
        if (_isSleeping)
            AddValue(_energyRecoveryRate);
        else
            AddValue(-_energyConsumptionRate);
    }
}