using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ForceExecutePopup : MonoBehaviour
{
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private TaskRunner _taskRunner;
    [SerializeField] private Team _playerTeam;
    [SerializeField] private Button _closeBtn;

    private void Awake()
    {
        if (_closeBtn != null)
        {
            _closeBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(ClosePopup);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshEntries();
    }

    private void RefreshEntries()
    {
        ClearEntries();

        if (_taskRunner == null) return;

        var allTeams = _taskRunner.GetAllTeams();
        foreach (var team in allTeams)
        {
            if (team == null || team == _playerTeam) continue;
            if (team.Tasks == null) continue;

            foreach (var task in team.Tasks)
            {
                if (task == null) continue;
                if (team.GetTaskStatus(task) != TaskStatus.NOT_STARTED) continue;

                var entryObj = Instantiate(_entryPrefab, _content);
                var entry = entryObj.GetComponent<ForceEntry>();
                if (entry != null)
                    entry.Setup(task, team, _taskRunner);
            }
        }
    }

    private void ClearEntries()
    {
        if (_content == null) return;
        foreach (Transform child in _content)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    private void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
