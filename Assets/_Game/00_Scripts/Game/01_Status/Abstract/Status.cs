using UnityEngine;

public abstract class Status : MonoBehaviour
{
    [Header("Value Settings")]
    [SerializeField] private readonly float _startingValue = 100;
    [SerializeField] private readonly float _maxValue = 100;
    [SerializeField] private readonly float _minimumValue = 0;

    private float _currentValue;

    public void AddValue(float value)
    {
        _currentValue += value;
        _currentValue = Mathf.Clamp(_currentValue, _minimumValue, _maxValue);
    }

    public void SetValue(float value)
    {
        _currentValue = Mathf.Clamp(value, _minimumValue, _maxValue);
    }

    public void ResetValue()
    {
        _currentValue = _startingValue;
    }

    public float StartingValue => _startingValue;
    public float MaxValue => _maxValue;
    public float MinimumValue => _minimumValue;
    public float CurrentValue => _currentValue;
}