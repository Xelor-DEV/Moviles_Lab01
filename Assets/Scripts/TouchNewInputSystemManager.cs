using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class TouchNewInputSystemManager : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputAction touchPositionAction;
    [SerializeField] private InputAction touchPressAction;

    [Header("Thresholds")]
    [SerializeField] private float doubleTapThreshold = 0.5f;
    [SerializeField] private float tapTimeThreshold = 0.3f;
    [SerializeField] private float dragThreshold = 10f;

    [Header("Swipe Thresholds")]
    [SerializeField] private float swipeDistanceThreshold = 50f;
    [SerializeField] private float swipeSpeedThreshold = 1000f;

    [Header("Trail Renderer")]
    [SerializeField] private TrailRenderer swipeTrail;
    [SerializeField] private BrushSettings brushSettings;

    [Header("Touch Events")]
    public UnityEvent<Vector2> OnTap;
    public UnityEvent<Vector2> OnDoubleTap;
    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent OnDragEnd;
    public UnityEvent OnSwipeStart;
    public UnityEvent<Vector2> OnSwipeUpdate;
    public UnityEvent<Vector2> OnSwipeEnd;

    private Vector2 touchStartScreenPosition;
    private Vector2 touchStartWorldPosition;
    private float touchStartTime;
    private bool isTouching = false;
    private bool isDragging = false;
    private int tapCount = 0;
    private float lastTapTime = 0f;

    private Vector2 previousPosition;
    private Vector2 swipeDelta;
    private float swipeSpeed;
    private bool isSwiping = false;

    private void Awake()
    {
        touchPressAction = input.actions["TouchPress"];
        touchPositionAction = input.actions["TouchPosition"];
    }

    private void OnEnable()
    {
        touchPressAction.performed += TouchPressed;
        touchPressAction.canceled += TouchReleased;
        touchPressAction.Enable();
        touchPositionAction.Enable();
    }

    private void OnDisable()
    {
        touchPressAction.performed -= TouchPressed;
        touchPressAction.canceled -= TouchReleased;
        touchPressAction.Disable();
        touchPositionAction.Disable();
    }

    private void Start()
    {
        brushSettings.SwipeTrail = swipeTrail;
    }

    private void TouchPressed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            touchStartTime = Time.time;
            touchStartScreenPosition = touchPositionAction.ReadValue<Vector2>();
            touchStartWorldPosition = GetWorldPosition(touchStartScreenPosition);
            previousPosition = touchStartScreenPosition;
            isTouching = true;
            isSwiping = false;
            if (swipeTrail != null)
            {
                swipeTrail.gameObject.SetActive(false);
                swipeTrail.Clear();
            }
        }
    }

    private void TouchReleased(InputAction.CallbackContext context)
    {
        isTouching = false;
        Vector2 endScreenPosition = touchPositionAction.ReadValue<Vector2>();
        Vector2 endWorldPosition = GetWorldPosition(endScreenPosition);

        float duration = Time.time - touchStartTime;
        float screenDistance = Vector2.Distance(touchStartScreenPosition, endScreenPosition);

        swipeSpeed = screenDistance / duration * Time.deltaTime;

        if (screenDistance >= swipeDistanceThreshold && swipeSpeed >= swipeSpeedThreshold)
        {
            OnSwipeEnd.Invoke(endWorldPosition);
            isSwiping = true;
        }

        if (isDragging == true && isSwiping == false)
        {
            OnDragEnd.Invoke();
        }

        isDragging = false;

        if (duration <= tapTimeThreshold && screenDistance < dragThreshold)
        {
            HandleTap(endWorldPosition);
        }
        if (isSwiping == true)
        {
            OnSwipeEnd.Invoke(endWorldPosition);
            DeactivateTrail();
        }
    }

    private void Update()
    {
        if (isTouching == true)
        {
            Vector2 currentScreenPosition = touchPositionAction.ReadValue<Vector2>();
            Vector2 currentWorldPosition = GetWorldPosition(currentScreenPosition);

            swipeDelta = currentScreenPosition - previousPosition;
            previousPosition = currentScreenPosition;

            if (isDragging == false)
            {
                float screenDistance = Vector2.Distance(touchStartScreenPosition, currentScreenPosition);
                if (screenDistance > dragThreshold)
                {
                    isDragging = true;
                    OnDragStart.Invoke(currentWorldPosition);
                }
            }
            else
            {
                if (isSwiping == false)
                {
                    float currentScreenDistance = Vector2.Distance(touchStartScreenPosition, currentScreenPosition);
                    float currentDuration = Time.time - touchStartTime;
                    float currentSpeed = currentScreenDistance / currentDuration;

                    if (currentScreenDistance >= swipeDistanceThreshold && currentSpeed >= swipeSpeedThreshold)
                    {
                        isSwiping = true;
                        OnSwipeStart.Invoke();
                        ActivateTrail(currentWorldPosition);
                    }
                }

                if (isSwiping == true)
                {
                    UpdateTrailPosition(currentWorldPosition);
                    OnSwipeUpdate.Invoke(currentWorldPosition);
                }
                else
                {
                    OnDrag.Invoke(currentWorldPosition);
                }
            }
        }
    }

    private Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;
        return worldPos;
    }

    private void HandleTap(Vector2 position)
    {
        if (Time.time - lastTapTime <= doubleTapThreshold)
        {
            OnDoubleTap.Invoke(position);
            tapCount = 0;
            lastTapTime = 0f;
        }
        else
        {
            tapCount = 1;
            lastTapTime = Time.time;
            StartCoroutine(CheckDoubleTap(position));
        }
    }

    private IEnumerator CheckDoubleTap(Vector2 position)
    {
        yield return new WaitForSeconds(doubleTapThreshold);

        if (tapCount == 1)
        {
            OnTap.Invoke(position);
            tapCount = 0;
            lastTapTime = 0f;
        }
    }

    private void ActivateTrail(Vector2 startPosition)
    {
        if (swipeTrail != null)
        {
            swipeTrail.transform.position = startPosition;
            swipeTrail.gameObject.SetActive(true);
            swipeTrail.Clear();
        }
    }

    private void UpdateTrailPosition(Vector2 newPosition)
    {
        if (swipeTrail != null)
        {
            swipeTrail.transform.position = newPosition;
        }
    }

    private void DeactivateTrail()
    {
        if (swipeTrail != null)
        {
            swipeTrail.gameObject.SetActive(false);
        }
    }
}