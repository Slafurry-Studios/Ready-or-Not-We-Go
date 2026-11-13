using System;
using System.Collections;
using UnityEngine;

public class MemoryStatus : Status, ITimedUpdate
{
    private Memory _memory;
    private Coroutine _recallCoroutine;
    private bool _isRecalling;
    private bool _registeredWithTimeCounter;

    public event Action<Memory> OnMemoryForgotten;
    public event Action<Memory> OnMemoryRecalled;

    public Memory Memory => _memory;
    public string MemoryName => _memory != null ? _memory.MemoryName : string.Empty;
    public bool IsForgotten => CurrentValue <= MinimumValue;
    public bool IsRecalling => _isRecalling;

    public void Initialize(Memory memory)
    {
        _memory = memory;
        ResetValue();
        _registeredWithTimeCounter = true;

        var timeCounter = FindFirstObjectByType<TimeCounter>();
        if (timeCounter != null)
            timeCounter.Register(this);
    }

    private void OnDestroy()
    {
        if (_recallCoroutine != null)
            StopCoroutine(_recallCoroutine);

        if (_registeredWithTimeCounter)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
                timeCounter.Unregister(this);
        }
    }

    public void TimedUpdate()
    {
        if (_memory == null || _isRecalling) return;

        AddValue(-_memory.DecayRate);

        if (CurrentValue <= MinimumValue)
        {
            OnMemoryForgotten?.Invoke(_memory);
        }
    }

    public void Recall()
    {
        if (_memory == null) return;
        if (_recallCoroutine != null)
            StopCoroutine(_recallCoroutine);

        _recallCoroutine = StartCoroutine(RecallRoutine());
    }

    private IEnumerator RecallRoutine()
    {
        _isRecalling = true;
        float elapsed = 0f;
        float duration = _memory.RecallTime;
        float startValue = CurrentValue;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetValue(Mathf.Lerp(startValue, MaxValue, t));
            yield return null;
        }

        SetValue(MaxValue);
        _isRecalling = false;
        OnMemoryRecalled?.Invoke(_memory);
    }

    public void Forget()
    {
        if (_memory == null) return;
        SetValue(MinimumValue);
        OnMemoryForgotten?.Invoke(_memory);
    }
}
