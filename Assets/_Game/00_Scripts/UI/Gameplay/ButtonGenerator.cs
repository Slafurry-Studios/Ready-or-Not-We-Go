using UnityEngine;

public class ButtonGenerator : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private Team _team;

    [Header("Work Buttons")]
    [SerializeField] private GameObject _workBtnPrefab;
    [SerializeField] private Transform _workContent;
    [SerializeField] private TaskPopup _taskPopup;
    [SerializeField] private DependencyPopup _dependencyPopup;

    [Header("Browse Buttons")]
    [SerializeField] private GameObject _browseBtnPrefab;
    [SerializeField] private Transform _browseContent;
    [SerializeField] private ActionPopup _actionPopup;

    private void OnEnable()
    {
        if (_team == null) return;

        _team.OnTasksChanged += GenerateTaskButtons;
        _team.OnAvailableActionsChanged += GenerateActionButtons;

        // Initial generation
        GenerateTaskButtons();
        GenerateActionButtons();
    }

    private void OnDisable()
    {
        if (_team == null) return;

        _team.OnTasksChanged -= GenerateTaskButtons;
        _team.OnAvailableActionsChanged -= GenerateActionButtons;
    }

    private void GenerateTaskButtons()
    {
        ClearButtons(_workContent);

        if (_team.Tasks == null) return;

        foreach (Task task in _team.Tasks)
        {
            if (task == null) continue;

            GameObject obj = Instantiate(_workBtnPrefab, _workContent);

            WorkBtn button = obj.GetComponent<WorkBtn>();
            button.Initialize(task, _taskPopup, _team, _dependencyPopup);
        }
    }

    private void GenerateActionButtons()
    {
        ClearButtons(_browseContent);

        if (_team.AvailableActions == null) return;

        foreach (Action action in _team.AvailableActions)
        {
            if (action == null) continue;

            GameObject obj = Instantiate(_browseBtnPrefab, _browseContent);

            BrowseBtn button = obj.GetComponent<BrowseBtn>();
            button.Initialize(action, _actionPopup, _team);
        }
    }

    private void ClearButtons(Transform content)
    {
        if (content == null) return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
}