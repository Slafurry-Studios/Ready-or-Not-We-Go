using UnityEngine;

[CreateAssetMenu(fileName = "Information Data", menuName = "Information Data")]
public class Information : ScriptableObject
{
    [SerializeField] private string _informationId;
    [SerializeField] private string _informationName;
    [SerializeField] private string _informationDescription;

    public string InformationId => _informationId;
    public string InformationName => _informationName;
    public string InformationDescription => _informationDescription;
}