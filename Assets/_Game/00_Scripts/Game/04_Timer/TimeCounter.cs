using UnityEngine;
using System.Linq;

public class TimeCounter : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private readonly int _maxDay = 7;
    [SerializeField] private readonly float _minuteToSecondRatio = 0.5f;

    private float _maxTime = 0;
    private float _currentTime = 0;
    private SessionType _currentSessionType = SessionType.WORKTIME;

    private ITimedUpdate[] timedUpdates;

    void Awake()
    {
        _maxTime = _maxDay * 24 * 60 * _minuteToSecondRatio;
        _currentTime = 0;

        timedUpdates = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                             .OfType<ITimedUpdate>()
                             .ToArray();
        InvokeRepeating(nameof(TimedUpdate), 1f, 1f);
    }

    void Update()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime > _maxTime)
        {
            _currentTime = _maxTime;
            CancelInvoke(nameof(TimedUpdate));
        }
    }

    private void TimedUpdate()
    {
        foreach (var timedUpdate in timedUpdates)
        {
            timedUpdate.TimedUpdate();
        }
    }

    public float MaxTime => _maxTime;
    public float MaxDay => _maxDay;
    public float MinuteToSecondRatio => _minuteToSecondRatio;


    public float CurrentDay => Mathf.Floor(_currentTime / (24 * 60 * _minuteToSecondRatio)) + 1;
    public float CurrentHour => Mathf.Floor(_currentTime % (24 * 60 * _minuteToSecondRatio) / (60 * _minuteToSecondRatio));
    public float CurrentMinute => Mathf.Floor(_currentTime % (60 * _minuteToSecondRatio) / _minuteToSecondRatio);

    public float CurrentTime => _currentTime;
    public SessionType CurrentSessionType => _currentSessionType;
}