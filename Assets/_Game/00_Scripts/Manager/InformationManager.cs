using UnityEngine;

public class InformationManager : MonoBehaviour
{
    public Information[] informationData;

    public Information GetInformationById(string informationId)
    {
        foreach (var info in informationData)
        {
            if (info.InformationId == informationId)
            {
                return info;
            }
        }
        return null;
    }
}