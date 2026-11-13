using UnityEngine;

[System.Serializable]
public class MemoryBank
{
    public Memory[] memories;
    public int MaxSize;

    public float CurrentTotalSize
    {
        get
        {
            if (memories == null) return 0f;
            float total = 0f;
            foreach (var m in memories)
            {
                if (m != null) total += m.MemorySize;
            }
            return total;
        }
    }

    public bool AddMemory(Memory memory)
    {
        if (memory == null) return false;

        var current = memories ?? new Memory[0];

        // Duplicate check
        foreach (var m in current)
        {
            if (m != null && m.information == memory.information)
                return false;
        }

        // MaxSize check
        if (CurrentTotalSize + memory.MemorySize > MaxSize) return false;

        var newMemories = new Memory[current.Length + 1];
        for (int i = 0; i < current.Length; i++)
            newMemories[i] = current[i];
        newMemories[current.Length] = memory;
        memories = newMemories;
        return true;
    }

    public void RemoveMemory(Memory memory)
    {
        if (memory == null || memories == null) return;

        Memory[] filtered = new Memory[0];
        bool found = false;

        foreach (var m in memories)
        {
            if (m != null && m.information == memory.information)
            {
                found = true;
                continue;
            }

            var temp = new Memory[filtered.Length + 1];
            for (int i = 0; i < filtered.Length; i++) temp[i] = filtered[i];
            temp[filtered.Length] = m;
            filtered = temp;
        }

        if (found)
            memories = filtered;
    }

    public bool HasMemory(Information information)
    {
        if (information == null || memories == null) return false;
        foreach (var m in memories)
        {
            if (m != null && m.information == information)
                return true;
        }
        return false;
    }
}
