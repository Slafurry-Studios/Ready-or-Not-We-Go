using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI memoryNameText;
    [SerializeField] private Slider slider;
    [SerializeField] private Button recallButton;

    private MemoryStatus _memoryStatus;

    private void OnEnable()
    {
        if (recallButton != null)
        {
            recallButton.onClick.AddListener(HandleRecallClicked);
        }

        Subscribe();
    }

    private void OnDisable()
    {
        if (recallButton != null)
        {
            recallButton.onClick.RemoveListener(HandleRecallClicked);
        }

        Unsubscribe();
    }

    public void SetMemoryStatus(MemoryStatus memoryStatus)
    {
        Unsubscribe();
        _memoryStatus = memoryStatus;
        Subscribe();
    }

    private void Subscribe()
    {
        if (_memoryStatus == null) return;

        _memoryStatus.OnValueChanged += UpdateSlider;

        if (memoryNameText != null)
        {
            memoryNameText.text = _memoryStatus.Memory.information.InformationName;
        }

        UpdateSlider(_memoryStatus.CurrentValue, _memoryStatus.MaxValue);
    }

    private void Unsubscribe()
    {
        if (_memoryStatus == null) return;

        _memoryStatus.OnValueChanged -= UpdateSlider;
    }

    private void UpdateSlider(float currentValue, float maxValue)
    {
        if (slider != null)
        {
            slider.maxValue = maxValue;
            slider.value = currentValue;
        }
    }

    private void HandleRecallClicked()
    {
        if (_memoryStatus != null)
        {
            _memoryStatus.Recall();
        }
    }
}