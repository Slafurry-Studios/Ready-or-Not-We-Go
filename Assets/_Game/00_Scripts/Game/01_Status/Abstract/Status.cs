using System;
using UnityEngine;

public abstract class Status : MonoBehaviour
{
    [Header("Value Settings")]
    [SerializeField] private float _startingValue = 100f;
    [SerializeField] private float _maxValue = 100f;
    [SerializeField] private float _minimumValue = 0f;

    private float _currentValue;

    /// <summary>Fired whenever the value changes: (current, max).</summary>
    public event Action<float, float> OnValueChanged;

    protected virtual void Awake()
    {
        ResetValue();
    }

    public void AddValue(float value) => SetValue(_currentValue + value);

    public void SetValue(float value)
    {
        float clamped = Mathf.Clamp(value, _minimumValue, _maxValue);
        if (Mathf.Approximately(clamped, _currentValue)) return;

        _currentValue = clamped;
        OnValueChanged?.Invoke(_currentValue, _maxValue);
    }

    public void ResetValue()
    {
        _currentValue = Mathf.Clamp(_startingValue, _minimumValue, _maxValue);
        OnValueChanged?.Invoke(_currentValue, _maxValue);
    }

    public float StartingValue => _startingValue;
    public float MaxValue => _maxValue;
    public float MinimumValue => _minimumValue;
    public float CurrentValue => _currentValue;
}