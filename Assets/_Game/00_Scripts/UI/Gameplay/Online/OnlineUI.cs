using UnityEngine;
using UnityEngine.UI;

public class OnlineUI : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private Team _team;

    [Header("UI Elements")]
    [SerializeField] private Image _onlineUI;
    [SerializeField] private Sprite onlineSprite;
    [SerializeField] private Sprite offlineSprite;

    [Header("Colors")]
    [SerializeField] private Color onlineColor = Color.green;
    [SerializeField] private Color offlineColor = Color.red;

    private void OnEnable()
    {
        if (_team != null)
        {
            _team.OnConditionChanged += HandleConditionChanged;
            HandleConditionChanged(_team.TeamCondition);
        }
    }

    private void OnDisable()
    {
        if (_team != null)
        {
            _team.OnConditionChanged -= HandleConditionChanged;
        }
    }

    private void HandleConditionChanged(TeamCondition condition)
    {
        switch (condition)
        {
            case TeamCondition.ONLINE:
                SetOnlineUI();
                break;
            case TeamCondition.INACTIVE:
            case TeamCondition.TERMINATED:
                SetOfflineUI();
                break;
        }
    }

    public void SetOnlineUI()
    {
        if (_onlineUI != null)
        {
            if (onlineSprite != null)
            {
                _onlineUI.sprite = onlineSprite;
            }
            _onlineUI.color = onlineColor;
        }
    }

    public void SetOfflineUI()
    {
        if (_onlineUI != null)
        {
            if (offlineSprite != null)
            {
                _onlineUI.sprite = offlineSprite;
            }
            _onlineUI.color = offlineColor;
        }
    }
}