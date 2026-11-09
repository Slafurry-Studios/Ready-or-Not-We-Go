using System.Collections.Generic;
using UnityEngine;

public class TimeCounter : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private int _maxDay = 7;
    [SerializeField] private float _minuteToSecondRatio = 0.5f;   // 1 in-game minute = 0.5 real second
    [SerializeField] private float _startHour = 12.5f;            // 12:30, set 0 for full 5040s
    [SerializeField] private float _tickInterval = 1f;

    private float _maxTime;
    private float _currentTime;
    private float _tickTimer;
    private int _pauseCount;
    private bool _finished;

    private SessionType _currentSessionType = SessionType.WORKTIME;
    private readonly List<ITimedUpdate> _timedUpdates = new List<ITimedUpdate>();

    private float SecondsPerHour => 60f * _minuteToSecondRatio;
    private float SecondsPerDay => 24f * SecondsPerHour;

    void Awake()
    {
        _maxTime = _maxDay * SecondsPerDay;
        _currentTime = _startHour * SecondsPerHour;
    }

    void Update()
    {
        if (_finished || IsPaused) return;

        float delta = Time.deltaTime;
        _currentTime += delta;
        _tickTimer += delta;

        while (_tickTimer >= _tickInterval)
        {
            _tickTimer -= _tickInterval;
            TimedUpdate();
        }

        if (_currentTime >= _maxTime)
        {
            _currentTime = _maxTime;
            _finished = true;
        }
    }

    private void TimedUpdate()
    {
        for (int i = 0; i < _timedUpdates.Count; i++)
        {
            _timedUpdates[i].TimedUpdate();
        }
    }

    // ---------- Registration (call from OnEnable / OnDisable) ----------
    public void Register(ITimedUpdate target)
    {
        if (!_timedUpdates.Contains(target)) _timedUpdates.Add(target);
    }

    public void Unregister(ITimedUpdate target) => _timedUpdates.Remove(target);

    // ---------- Pause (counter, so VN and sleep minigame don't cancel each other) ----------
    public void Pause() => _pauseCount++;
    public void Resume() => _pauseCount = Mathf.Max(0, _pauseCount - 1);
    public bool IsPaused => _pauseCount > 0;

    // ---------- Read-only values ----------
    public float MaxTime => _maxTime;
    public int MaxDay => _maxDay;
    public float MinuteToSecondRatio => _minuteToSecondRatio;
    public float CurrentTime => _currentTime;
    public bool IsFinished => _finished;
    public SessionType CurrentSessionType => _currentSessionType;

    public int CurrentDay => Mathf.Min(_maxDay, Mathf.FloorToInt(_currentTime / SecondsPerDay) + 1);
    public int CurrentHour => Mathf.FloorToInt((_currentTime % SecondsPerDay) / SecondsPerHour);
    public int CurrentMinute => Mathf.FloorToInt((_currentTime % SecondsPerHour) / _minuteToSecondRatio);
}