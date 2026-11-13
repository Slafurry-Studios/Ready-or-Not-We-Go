
using UnityEngine;
using TMPro;
using System.Text;

public class ActionPopup : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI _titleTxt;
    [SerializeField] private TextMeshProUGUI _descTxt;

    public void SetText(Action action)
    {
        if (action == null) return;

        _titleTxt.text = action.ActionName;

        StringBuilder description = new StringBuilder();

        if (action.EnergyUsed != 0)
            description.AppendLine($"Uses {action.EnergyUsed} energy.");

        if (action.SuspicionGenerated != 0)
            description.AppendLine($"Generates {action.SuspicionGenerated} suspicion.");

        _descTxt.text = description.ToString().TrimEnd();
    }
}
