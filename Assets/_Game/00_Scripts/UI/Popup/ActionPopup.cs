
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class ActionPopup : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI _titleTxt;
    [SerializeField] private TextMeshProUGUI _descTxt;

    [Header("Execution")]
    [SerializeField] private Team _team;
    [SerializeField] private Button _executeBtn;

    private Action _currentAction;

    private void Awake()
    {
        if (_executeBtn != null)
        {
            _executeBtn.onClick.RemoveAllListeners();
            _executeBtn.onClick.AddListener(ExecuteAction);
        }
    }

    public void SetText(Action action)
    {
        if (action == null) return;

        _currentAction = action;
        _titleTxt.text = action.ActionName;

        StringBuilder description = new StringBuilder();

        if (action.EnergyUsed != 0)
            description.AppendLine($"Uses {action.EnergyUsed} energy.");

        if (action.MinimumDayRequirement > 0)
            description.AppendLine($"Available from Day {action.MinimumDayRequirement}.");

        if (action.RequiredInformation != null && action.RequiredInformation.Length > 0)
        {
            description.AppendLine("Required Information:");
            foreach (var req in action.RequiredInformation)
            {
                if (req != null)
                    description.AppendLine($"- {req.InformationName}");
            }
        }

        _descTxt.text = description.ToString().TrimEnd();

        RefreshExecuteBtn();
    }

    private void RefreshExecuteBtn()
    {
        if (_executeBtn == null) return;

        bool canExecute = _currentAction != null
            && _team != null
            && _team.CanExecuteAction(_currentAction);

        _executeBtn.interactable = canExecute;
    }

    private void ExecuteAction()
    {
        if (_currentAction == null) return;
        if (_team == null) return;

        _team.ExecuteAction(_currentAction);
        gameObject.SetActive(false);
    }
}
