using UnityEngine;

public class FounderAI : MonoBehaviour, ITimedUpdate
{
    [Header("References")]
    [SerializeField] private Team _team;

    [Header("Founder Settings")]
    [SerializeField] private float _suspicionIncreasePerTick = 5f;
    [SerializeField] private float _searchInterval = 10f;

    private bool _registered;
    private float _searchTimer;

    private void OnEnable()
    {
        if (!_registered)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
            {
                timeCounter.Register(this);
                _registered = true;
            }
        }
    }

    private void OnDisable()
    {
        if (_registered)
        {
            var timeCounter = FindFirstObjectByType<TimeCounter>();
            if (timeCounter != null)
                timeCounter.Unregister(this);
            _registered = false;
        }
    }

    public void TimedUpdate()
    {
        if (_team == null) return;

        _searchTimer += Time.deltaTime;

        if (_searchTimer >= _searchInterval)
        {
            _searchTimer = 0f;

            if (_team.Energy != null && _team.Energy.CurrentValue > 0f)
            {
                _team.Energy.AddValue(-_suspicionIncreasePerTick);
            }
        }
    }
}
