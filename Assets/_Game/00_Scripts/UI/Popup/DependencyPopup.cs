using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class DependencyPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleTxt;
    [SerializeField] private TextMeshProUGUI _descTxt;
    [SerializeField] private Button _closeBtn;

    private Task _blockedTask;

    private void Awake()
    {
        if (_closeBtn != null)
        {
            _closeBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(ClosePopup);
        }
    }

    public void Show(Task blockedTask, Task[] blockingDependencies)
    {
        if (blockedTask == null) return;

        _blockedTask = blockedTask;
        gameObject.SetActive(true);

        _titleTxt.text = $"Blocked: {blockedTask.TaskName}";

        StringBuilder description = new StringBuilder();
        description.AppendLine("Cannot start this task yet. Dependencies not completed:");

        if (blockingDependencies != null && blockingDependencies.Length > 0)
        {
            foreach (var dep in blockingDependencies)
            {
                if (dep == null) continue;
                description.AppendLine($"- {dep.TaskName}");
            }
        }
        else
        {
            description.AppendLine("- None (internal error)");
        }

        description.AppendLine();
        description.AppendLine("Complete the dependency tasks first.");

        _descTxt.text = description.ToString().TrimEnd();
    }

    private void ClosePopup()
    {
        _blockedTask = null;
        gameObject.SetActive(false);
    }
}
