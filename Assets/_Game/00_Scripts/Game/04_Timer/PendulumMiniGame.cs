using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class PendulumMiniGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _pendulum;
    [SerializeField] private Image _pendulumImage;
    [SerializeField] private RectTransform _target;
    [SerializeField] private RectTransform _railing;
    [SerializeField] private SleepController _sleepController;
    [SerializeField] private TimeCounter _timeCounter;
    [SerializeField] private EnergyStatus _energyStatus;

    [Header("Movement")]
    [SerializeField] private float _baseSpeed = 300f;
    [SerializeField] private float _speedIncrease = 40f;
    [SerializeField] private float _maxSpeed = 800f;
    [SerializeField] private float _accuracyRange = 10f;

    [Header("Double Press")]
    [SerializeField] private float _doublePressInterval = 0.35f;

    [Header("Game Feel")]
    [SerializeField] private float _punchScale = 1.3f;
    [SerializeField] private float _punchDuration = 0.2f;
    [SerializeField] private Color _missColor = Color.red;

    [Header("Target Pop")]
    [SerializeField] private float _targetPunchScale = 1.3f;
    [SerializeField] private float _targetPunchDuration = 0.15f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onWakeUp;
    [SerializeField] private UnityEvent _onSleepEightHours;

    private bool _isPlaying;
    private bool _isDeepSleeping;

    private float _direction = 1f;
    private float _currentSpeed;
    private float _lastPressTime = -10f;

    private int _missCount;

    private Vector3 _originalScale;
    private Vector3 _targetOriginalScale;
    private Color _originalColor;

    private Coroutine _punchCoroutine;
    private Coroutine _targetPunchCoroutine;

    private float LeftLimit =>
        _railing.anchoredPosition.x - _railing.rect.width * 0.5f;

    private float RightLimit =>
        _railing.anchoredPosition.x + _railing.rect.width * 0.5f;

    private float HitRange =>
        _target.rect.width * 0.5f + _accuracyRange;

    private void Awake()
    {
        _originalScale = _pendulum.localScale;
        _targetOriginalScale = _target.localScale;
        _originalColor = _pendulumImage.color;
    }

    public void StartGame()
    {
        _isPlaying = true;
        _isDeepSleeping = false;

        _missCount = 0;
        _direction = 1f;
        _currentSpeed = _baseSpeed;
        _lastPressTime = -10f;

        StopEffects();

        _pendulum.localScale = _originalScale;
        _pendulumImage.color = _originalColor;
        _target.localScale = _targetOriginalScale;

        SetPendulumPosition(LeftLimit);
        SetRandomTargetPosition();
    }

    private void Update()
    {
        // Saat deep sleep, pendulum berhenti.
        // Pemain masih bisa double press untuk bangun paksa.
        if (_isDeepSleeping)
        {
            HandleKeyboardInput();
            return;
        }

        if (!_isPlaying)
            return;

        float previousX = _pendulum.anchoredPosition.x;
        float movementDirection = _direction;

        float x = previousX +
                  movementDirection * _currentSpeed * Time.deltaTime;

        bool reachedRight = x >= RightLimit;
        bool reachedLeft = x <= LeftLimit;

        if (reachedRight)
            x = RightLimit;
        else if (reachedLeft)
            x = LeftLimit;

        SetPendulumPosition(x);

        HandleKeyboardInput();

        if (!_isPlaying)
            return;

        CheckTargetPassed(previousX, x, movementDirection);

        if (!_isPlaying)
            return;

        if (reachedRight)
            _direction = -1f;
        else if (reachedLeft)
            _direction = 1f;
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Press();
        }
    }

    public void Press()
    {
        // Double press saat deep sleep = bangun paksa.
        if (_isDeepSleeping)
        {
            float currentTime = Time.unscaledTime;

            if (currentTime - _lastPressTime <= _doublePressInterval)
            {
                ForceWakeUp();
                return;
            }

            _lastPressTime = currentTime;
            Debug.Log("Press again quickly to wake up!");
            return;
        }

        if (!_isPlaying)
            return;

        float distance = Mathf.Abs(
            _pendulum.anchoredPosition.x -
            _target.anchoredPosition.x
        );

        if (distance <= HitRange)
        {
            Hit();
        }
        else
        {
            Miss();
        }
    }

    private void Hit()
    {
        Debug.Log("HIT!");

        PlayPunch(_originalColor);
        PlayTargetPunch();

        // Pendulum semakin cepat setiap berhasil HIT.
        _currentSpeed = Mathf.Min(
            _currentSpeed + _speedIncrease,
            _maxSpeed
        );

        SetRandomTargetPosition();
    }

    private void Miss()
    {
        Debug.Log("MISS!");

        PlayPunch(_missColor);

        _missCount++;

        if (_missCount >= 2)
            EnterDeepSleep();
    }

    private void CheckTargetPassed(
        float previousX,
        float currentX,
        float movementDirection)
    {
        float targetX = _target.anchoredPosition.x;

        bool passedTarget =
            movementDirection > 0f
                ? previousX <= targetX + HitRange &&
                  currentX > targetX + HitRange
                : previousX >= targetX - HitRange &&
                  currentX < targetX - HitRange;

        if (passedTarget)
            SleepForEightHours();
    }

    private void EnterDeepSleep()
    {
        if (!_isPlaying)
            return;

        _isPlaying = false;
        _isDeepSleeping = true;
        _lastPressTime = -10f;

        _sleepController.StartDeepSleep();

        Debug.Log("Character entered deep sleep after 2 misses!");
    }

    private void ForceWakeUp()
    {
        if (!_isDeepSleeping)
            return;

        _isDeepSleeping = false;

        _sleepController.EndSleep();

        _onWakeUp?.Invoke();

        Debug.Log("Character forced awake!");
    }

    private void SleepForEightHours()
    {
        if (!_isPlaying)
            return;

        _isPlaying = false;

        // Actually advance game time by 8 hours so the player wakes up later.
        _timeCounter.AdvanceTime(8f);

        // A full night's rest restores energy to maximum.
        if (_energyStatus != null)
            _energyStatus.SetValue(_energyStatus.MaxValue);

        _sleepController.EndSleep();

        Debug.Log("Character slept for 8 hours!");
        _onSleepEightHours?.Invoke();
    }

    private void WakeUp()
    {
        if (!_isPlaying)
            return;

        _isPlaying = false;

        _sleepController.EndSleep();
        _onWakeUp?.Invoke();
    }

    private void SetPendulumPosition(float x)
    {
        _pendulum.anchoredPosition = new Vector2(
            x,
            _pendulum.anchoredPosition.y
        );
    }

    private void SetRandomTargetPosition()
    {
        float halfTargetWidth = _target.rect.width * 0.5f;

        float minX = LeftLimit + halfTargetWidth;
        float maxX = RightLimit - halfTargetWidth;

        float x = Random.Range(minX, maxX);

        _target.anchoredPosition = new Vector2(
            x,
            _target.anchoredPosition.y
        );
    }

    private void PlayPunch(Color color)
    {
        if (_punchCoroutine != null)
            StopCoroutine(_punchCoroutine);

        _punchCoroutine = StartCoroutine(PunchEffect(color));
    }

    private IEnumerator PunchEffect(Color color)
    {
        float halfDuration = _punchDuration * 0.5f;
        float timer = 0f;

        _pendulumImage.color = color;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            _pendulum.localScale = Vector3.Lerp(
                _originalScale,
                _originalScale * _punchScale,
                t
            );

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            _pendulum.localScale = Vector3.Lerp(
                _originalScale * _punchScale,
                _originalScale,
                t
            );

            _pendulumImage.color = Color.Lerp(
                color,
                _originalColor,
                t
            );

            yield return null;
        }

        _pendulum.localScale = _originalScale;
        _pendulumImage.color = _originalColor;

        _punchCoroutine = null;
    }

    private void PlayTargetPunch()
    {
        if (_targetPunchCoroutine != null)
            StopCoroutine(_targetPunchCoroutine);

        _targetPunchCoroutine = StartCoroutine(TargetPunchEffect());
    }

    private IEnumerator TargetPunchEffect()
    {
        float halfDuration = _targetPunchDuration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            _target.localScale = Vector3.Lerp(
                _targetOriginalScale,
                _targetOriginalScale * _targetPunchScale,
                t
            );

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            _target.localScale = Vector3.Lerp(
                _targetOriginalScale * _targetPunchScale,
                _targetOriginalScale,
                t
            );

            yield return null;
        }

        _target.localScale = _targetOriginalScale;
        _targetPunchCoroutine = null;
    }

    private void StopEffects()
    {
        if (_punchCoroutine != null)
        {
            StopCoroutine(_punchCoroutine);
            _punchCoroutine = null;
        }

        if (_targetPunchCoroutine != null)
        {
            StopCoroutine(_targetPunchCoroutine);
            _targetPunchCoroutine = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_railing == null)
            return;

        Vector3 left = _railing.TransformPoint(
            new Vector3(-_railing.rect.width * 0.5f, 0f, 0f)
        );

        Vector3 right = _railing.TransformPoint(
            new Vector3(_railing.rect.width * 0.5f, 0f, 0f)
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(left, right);

        if (_target != null)
        {
            Vector3 center = _target.position;
            Vector3 offset = _target.right * HitRange;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center - offset, center + offset);
        }

        if (_pendulum != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_pendulum.position, 5f);
        }
    }
}