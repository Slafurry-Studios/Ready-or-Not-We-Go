using TMPro;
using UnityEngine;

public class TimeHUD : MonoBehaviour
{
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private TMP_Text _label;

    private int _lastMinute = -1;

    void Update()
    {
        int minute = _timeCounter.CurrentMinute;
        int hour = _timeCounter.CurrentHour;
        int day = _timeCounter.CurrentDay;

        // Only rebuild the string when the displayed minute changes (avoids garbage every frame)
        int key = day * 10000 + hour * 100 + minute;
        if (key == _lastMinute) return;
        _lastMinute = key;

        _label.text = $"Day {day} | {hour:00}:{minute:00}";
    }
}