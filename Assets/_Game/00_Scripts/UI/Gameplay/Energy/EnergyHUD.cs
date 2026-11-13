using TMPro;
using UnityEngine;

public class EnergyHUD : MonoBehaviour
{
    [SerializeField] private EnergyStatus _energyStatus;
    [SerializeField] private TMP_Text _label;

    private void OnEnable()
    {
        _energyStatus.OnValueChanged += Refresh;
        Refresh(_energyStatus.CurrentValue, _energyStatus.MaxValue);
    }

    private void OnDisable()
    {
        _energyStatus.OnValueChanged -= Refresh;
    }

    private void Refresh(float current, float max)
    {
        _label.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
}