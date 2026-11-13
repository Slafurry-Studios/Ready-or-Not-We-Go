using System;
using UnityEngine;

public class MemoryStatus : Status
{
    private Memory _memory;

    public event System.Action OnRecalled;

    public Memory Memory => _memory;
    public string MemoryName => _memory != null ? _memory.MemoryName : string.Empty;

    public void Initialize(Memory memory)
    {
        _memory = memory;
        ResetValue();
    }

    public void Recall()
    {
        OnRecalled?.Invoke();
    }
}