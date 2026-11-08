using UnityEngine;

[CreateAssetMenu(fileName = "Information Data", menuName = "Game/Information")]
public class Information : ScriptableObject
{
    [SerializeField] private string _informationName;
    [SerializeField] private string _informationDescription;

    public string InformationName => _informationName;
    public string InformationDescription => _informationDescription;
}