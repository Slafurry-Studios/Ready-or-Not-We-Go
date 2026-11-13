using System.Collections.Generic;
using UnityEngine;

public class MemoryContainerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Team _team;
    [SerializeField] private MemoryUI _memoryUIPrefab;
    [SerializeField] private Transform _containerParent;

    private readonly List<MemoryUI> _spawnedItems = new List<MemoryUI>();

    private void Start()
    {
        RefreshUI();
    }

    public void SetTeam(Team team)
    {
        _team = team;
        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearUI();

        if (_team == null || _team.MemoryBank == null || _team.MemoryBank.memories == null) return;
        if (_memoryUIPrefab == null) return;

        Transform parent = _containerParent != null ? _containerParent : transform;
        Memory[] memories = _team.MemoryBank.memories;

        for (int i = 0; i < memories.Length; i++)
        {
            if (memories[i] == null) continue;

            MemoryUI item = Instantiate(_memoryUIPrefab, parent);
            MemoryStatus status = item.GetComponent<MemoryStatus>();

            if (status != null)
            {
                status.Initialize(memories[i]);
                item.SetMemoryStatus(status);
            }

            _spawnedItems.Add(item);
        }
    }

    public void ClearUI()
    {
        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            if (_spawnedItems[i] != null)
            {
                Destroy(_spawnedItems[i].gameObject);
            }
        }

        _spawnedItems.Clear();
    }
}