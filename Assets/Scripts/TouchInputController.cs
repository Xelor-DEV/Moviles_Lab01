using UnityEngine;
using UnityEngine.Events;

public class TouchInputController : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField, Range(0.1f, 2f)] private float swipeDuration = 0.5f;

    [Header("Other Settings")]
    [SerializeField, Range(0.1f, 1f)] private float doubleTapThreshold = 0.2f;
    [SerializeField, Range(0.1f, 1f)] private float holdThreshold = 0.4f;
    [SerializeField, Min(10)] private float doubleTapRadius = 50f;

    [Header("References")]
    [SerializeField] private TrailRenderer swipeTrail;
    [SerializeField] private Camera mainCamera;

    [Header("Events")]
    public UnityEvent<Vector2> OnTap;
    public UnityEvent<Vector2> OnDoubleTap;
    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent OnDragEnd;
    public UnityEvent OnSwipeStart;
    public UnityEvent<Vector2> OnSwipeUpdate;
    public UnityEvent<Vector2> OnSwipeEnd;

    private Vector2 touchStartPos;
    private Vector2 currentTouchPos;
    private float touchStartTime;
    private Vector2 lastTapPos;
    private float lastTapTime;
    private bool isDragging;
    private bool doubleTapHandled;
    private GameObject draggedObject;
    private bool isSwiping;
    private float swipeProgress;
    private Coroutine swipeRoutine;

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
                case TouchPhase.Canceled:
                    HandleTouchEnd(touch);
                    break;
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        touchStartPos = touch.position;
        currentTouchPos = touch.position;
        touchStartTime = Time.time;
        isDragging = false;
        doubleTapHandled = false;
        isSwiping = false;
        swipeProgress = 0f;

        Vector2 worldPos = GetWorldPosition(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (Time.time - lastTapTime <= doubleTapThreshold &&
            Vector2.Distance(touch.position, lastTapPos) <= doubleTapRadius)
        {
            OnDoubleTap?.Invoke(worldPos);
            doubleTapHandled = true;
            lastTapTime = 0;
            lastTapPos = Vector2.zero;
            SetTrailActive(false);
            return;
        }

        if (hit.collider != null)
        {
            draggedObject = hit.collider.gameObject;
            isDragging = true;
            OnDragStart?.Invoke(worldPos);
        }

        lastTapPos = touch.position;
        lastTapTime = Time.time;
    }

    private void HandleTouchMove(Touch touch)
    {
        currentTouchPos = touch.position;

        if (isDragging && draggedObject != null)
        {
            Vector2 newPos = GetWorldPosition(touch.position);
            draggedObject.transform.position = newPos;
            OnDrag?.Invoke(newPos);
            return;
        }

        if (!isDragging && !isSwiping)
        {
            StartSwipe();
        }

        if (isSwiping)
        {
            UpdateSwipeProgress();
        }
    }

    private void StartSwipe()
    {
        isSwiping = true;
        swipeProgress = 0f;
        OnSwipeStart?.Invoke();
        SetTrailActive(true);
        UpdateTrailPosition(currentTouchPos);
        StartCoroutine(SwipeTimer());
    }

    private System.Collections.IEnumerator SwipeTimer()
    {
        float startTime = Time.time;
        Vector2 startPos = currentTouchPos;

        while (isSwiping && (Time.time - startTime) < swipeDuration)
        {
            float progress = (Time.time - startTime) / swipeDuration;
            swipeProgress = progress;

            Vector2 currentDirection = (currentTouchPos - startPos).normalized;
            OnSwipeUpdate?.Invoke(currentDirection);

            UpdateTrailPosition(currentTouchPos);
            yield return null;
        }

        if (isSwiping)
        {
            FinalizeSwipe(startPos, currentTouchPos);
        }
    }

    private void UpdateSwipeProgress()
    {
        if (swipeTrail != null)
        {
            swipeTrail.time = Mathf.Lerp(0, swipeDuration, swipeProgress);
        }
    }

    private void HandleTouchEnd(Touch touch)
    {
        if (doubleTapHandled)
        {
            SetTrailActive(false);
            return;
        }

        if (isSwiping)
        {
            Vector2 endPos = touch.position;
            FinalizeSwipe(touchStartPos, endPos);
        }
        else
        {
            float elapsedTime = Time.time - touchStartTime;

            if (!isDragging && elapsedTime < 0.15f)
            {
                OnTap?.Invoke(GetWorldPosition(touch.position));
            }
        }

        if (isDragging)
        {
            OnDragEnd?.Invoke();
            draggedObject = null;
        }

        isDragging = false;
        isSwiping = false;
    }

    private void FinalizeSwipe(Vector2 startPos, Vector2 endPos)
    {
        Vector2 swipeVector = endPos - startPos;
        Vector2 swipeDirection = swipeVector.normalized;

        OnSwipeEnd?.Invoke(swipeDirection);
        SetTrailActive(false);
        isSwiping = false;
    }

    private void SetTrailActive(bool active)
    {
        if (swipeTrail != null)
        {
            swipeTrail.emitting = active;
            swipeTrail.gameObject.SetActive(active);
            if (!active) swipeTrail.Clear();
        }
    }

    private void UpdateTrailPosition(Vector2 screenPos)
    {
        if (swipeTrail != null)
        {
            swipeTrail.transform.position = GetWorldPosition(screenPos);
        }
    }

    private Vector2 GetWorldPosition(Vector2 screenPos)
    {
        return mainCamera.ScreenToWorldPoint(screenPos);
    }
}