using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TouchInputController : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField, Range(0.1f, 1f)] private float doubleTapThreshold = 0.2f;
    [SerializeField, Range(0.1f, 1f)] private float holdThreshold = 0.4f;
    [SerializeField, Range(0.1f, 1f)] private float swipeMaxTime = 0.3f;
    [SerializeField, Min(10)] private float doubleTapRadius = 50f;
    [SerializeField, Min(50)] private float swipeMinDistance = 100f;

    [Header("References")]
    [SerializeField] private TrailRenderer swipeTrail;

    [Header("Events")]
    public UnityEvent<Vector2> OnTap;
    public UnityEvent<Vector2> OnDoubleTap;
    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent OnDragEnd;
    public UnityEvent<Vector2> OnSwipe;

    private Vector2 touchStartPos;
    private float touchStartTime;
    private Vector2 lastTapPos;
    private float lastTapTime;
    private bool isDragging;
    private bool isWaitingForDoubleTap;
    private Coroutine doubleTapCoroutine;

    private void Update() 
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Moved:
                    HandleTouchMove(touch);
                    break;

                case TouchPhase.Ended:
                    HandleTouchEnd(touch);
                    break;

                case TouchPhase.Canceled:
                    HandleTouchEnd(touch);
                    break;
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        Vector2 currentPos = touch.position;
        touchStartPos = currentPos;
        touchStartTime = Time.time;

        if (Time.time - lastTapTime <= doubleTapThreshold &&
            Vector2.Distance(currentPos, lastTapPos) <= doubleTapRadius)
        {
            if (doubleTapCoroutine != null)
            {
                StopCoroutine(doubleTapCoroutine);
            }
            isWaitingForDoubleTap = false;
            OnDoubleTap?.Invoke(GetWorldPosition(currentPos));
            lastTapTime = 0;
            return;
        }

        if (isDragging == false)
        {
            lastTapTime = Time.time;
            lastTapPos = currentPos;
            if (doubleTapCoroutine != null)
            {
                StopCoroutine(doubleTapCoroutine);
            }
            doubleTapCoroutine = StartCoroutine(SingleTapCheck(currentPos));
        }

        if (swipeTrail != null)
        {
            swipeTrail.Clear();
            swipeTrail.emitting = true;
        }
    }

    private IEnumerator SingleTapCheck(Vector2 touchPos)
    {
        isWaitingForDoubleTap = true;
        yield return new WaitForSeconds(doubleTapThreshold);

        if (isWaitingForDoubleTap)
        {
            OnTap?.Invoke(GetWorldPosition(touchPos));
            isWaitingForDoubleTap = false;
        }
    }
    private void HandleTouchMove(Touch touch)
    {
        if (isDragging == true)
        {
            OnDrag?.Invoke(GetWorldPosition(touch.position));
            return;
        }

        if (Time.time - touchStartTime >= holdThreshold)
        {
            isDragging = true;
            OnDragStart?.Invoke(GetWorldPosition(touch.position));
        }

        if (swipeTrail != null)
        {
            swipeTrail.transform.position = GetWorldPosition(touch.position);
        }
    }

    private void HandleTouchEnd(Touch touch)
    {
        float swipeDistance = Vector2.Distance(touchStartPos, touch.position);
        float swipeDuration = Time.time - touchStartTime;

        if (swipeDistance >= swipeMinDistance && swipeDuration <= swipeMaxTime)
        {
            OnSwipe?.Invoke(GetWorldPosition(touch.position));
        }

        if (isDragging == true)
        {
            OnDragEnd?.Invoke();
        }

        if (swipeTrail != null)
        {
            swipeTrail.emitting = false;
        }

        isDragging = false;
        isWaitingForDoubleTap = false;
    }

    private Vector2 GetWorldPosition(Vector2 screenPos)
    {
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}