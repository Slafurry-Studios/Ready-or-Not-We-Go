using UnityEngine;

public class EnergyStatus : Status
{
    [Header("Energy Settings")]
    [SerializeField] private float _energyConsumptionRate = 1f;

    private void Update()
    {
        ConsumeEnergy();
    }

    private void ConsumeEnergy()
    {
        AddValue(-_energyConsumptionRate * Time.deltaTime);
    }
}