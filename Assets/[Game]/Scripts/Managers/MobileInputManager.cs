using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum MobileInputType
{
    Swipe,
    Tap,
    LongPress,
    Drag,
    PinchZoom,
    VirtualJoystick
}

public class MobileInputManager : MonoBehaviour
{
    #region Singleton

    public static MobileInputManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    [Header("Input Type Settings")]
    // public Dropdown inputTypeDropdown;
    public MobileInputType currentInputType;

    [Header("Global Settings")] [Tooltip("Tüm dokunmatik girdiler için ortak hassasiyet (sensitivity) değeri.")]
    public float globalSensitivity = 1.0f;

    [Header("Swipe Settings")] public float swipeMinDistance = 50f; // Minimum mesafe

    [Header("Tap Settings")] public float tapMaxMovement = 10f; // Maksimum hareketle tap olarak kabul et
    public float tapMaxTime = 0.3f; // Tap için maksimum süre

    [Header("Long Press Settings")] public float longPressThreshold = 0.8f; // Kaç saniyede long press sayılır

    [Header("Virtual Joystick Settings")] public RectTransform joystickArea; // Joystick için tanımlı alan (Opsiyonel)
    public float joystickMaxDistance = 100f; // Maksimum joystick mesafesi

    [Header("Output Events")] public UnityEvent<Vector2> OnSwipe; // Yön bilgisi döner
    public UnityEvent OnTap; // Tap tespit edildiğinde
    public UnityEvent OnLongPress; // LongPress tetiklendiğinde
    public UnityEvent<Vector2> OnDragStart = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent<Vector2> OnDragEnd = new UnityEvent<Vector2>(); // Sürekli dokunma hareketinde
    public UnityEvent<float> OnPinchZoom; // PinchZoom: artış/azalış oranı
    public UnityEvent<Vector2> OnVirtualJoystick; // Joystick yönü

    private Vector2 startTouchPos;
    private float touchStartTime;
    private bool longPressTriggered = false;
    
    private Vector2 startTouchPosition;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    // Pinch için iki dokunuşun başlangıç mesafesi
    private float initialPinchDistance;

    // Virtual Joystick için başlangıç pozisyonu
    private Vector2 joystickCenter;
    private bool joystickActive = false;

    void Start()
    {
        // if (inputTypeDropdown != null)
        //     inputTypeDropdown.onValueChanged.AddListener(delegate { UpdateInputType(); });

        UpdateInputType();
    }

    void UpdateInputType()
    {
        // currentInputType = (MobileInputType)inputTypeDropdown.value;
        // Reset durumları
        longPressTriggered = false;
        joystickActive = false;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            switch (currentInputType)
            {
                case MobileInputType.Swipe:
                    HandleSwipe();
                    break;
                case MobileInputType.Tap:
                    HandleTap();
                    break;
                case MobileInputType.LongPress:
                    HandleLongPress();
                    break;
                case MobileInputType.Drag:
                    HandleDrag();
                    break;
                case MobileInputType.PinchZoom:
                    HandlePinchZoom();
                    break;
                case MobileInputType.VirtualJoystick:
                    HandleVirtualJoystick();
                    break;
            }
        }
    }

    #region Gesture Handlers

    void HandleSwipe()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            startTouchPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 endTouchPos = touch.position;
            float distance = Vector2.Distance(startTouchPos, endTouchPos);
            if (distance >= swipeMinDistance)
            {
                Vector2 swipeDirection = (endTouchPos - startTouchPos).normalized;
                OnSwipe?.Invoke(swipeDirection * globalSensitivity);
            }
        }
    }

    void HandleTap()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            startTouchPos = touch.position;
            touchStartTime = Time.time;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            float duration = Time.time - touchStartTime;
            float movement = Vector2.Distance(startTouchPos, touch.position);
            if (duration <= tapMaxTime && movement <= tapMaxMovement)
            {
                OnTap?.Invoke();
            }
        }
    }

    void HandleLongPress()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            startTouchPos = touch.position;
            touchStartTime = Time.time;
            longPressTriggered = false;
        }
        else if (touch.phase == TouchPhase.Stationary)
        {
            if (!longPressTriggered && (Time.time - touchStartTime) >= longPressThreshold)
            {
                OnLongPress?.Invoke();
                longPressTriggered = true;
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            longPressTriggered = false;
        }
    }

    void HandleDrag()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    lastTouchPosition = touch.position;
                    isDragging = true;
                    OnDragStart.Invoke(startTouchPosition);
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 dragDelta = (touch.position - lastTouchPosition) * globalSensitivity;
                        OnDrag.Invoke(dragDelta);
                        lastTouchPosition = touch.position;
                    }

                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        OnDragEnd.Invoke(touch.position);
                        isDragging = false;
                    }

                    Vector2 swipeDelta = (touch.position - startTouchPosition) * globalSensitivity;
                    if (swipeDelta.magnitude > 50) // Minimum mesafe kontrolü
                    {
                        OnSwipe.Invoke(swipeDelta);
                    }

                    break;
            }
        }

        // Touch touch = Input.GetTouch(0);
        // if (touch.phase == TouchPhase.Began)
        // {
        //     startTouchPos = touch.position;
        // }
        // else if (touch.phase == TouchPhase.Moved)
        // {
        //     Vector2 dragDelta = touch.deltaPosition * globalSensitivity;
        //     OnDrag?.Invoke(dragDelta);
        // }
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount >= 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                return;
            }

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            float delta = (currentDistance - initialPinchDistance) * globalSensitivity;
            OnPinchZoom?.Invoke(delta);
            initialPinchDistance = currentDistance;
        }
    }

    void HandleVirtualJoystick()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            if (joystickArea != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(joystickArea, touch.position))
                {
                    joystickActive = true;
                    joystickCenter = touch.position;
                }
            }
            else
            {
                joystickActive = true;
                joystickCenter = touch.position;
            }
        }
        else if (touch.phase == TouchPhase.Moved && joystickActive)
        {
            Vector2 direction = touch.position - joystickCenter;
            direction = (Vector2.ClampMagnitude(direction, joystickMaxDistance) / joystickMaxDistance) *
                        globalSensitivity;
            OnVirtualJoystick?.Invoke(direction);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (joystickActive)
            {
                OnVirtualJoystick?.Invoke(Vector2.zero);
                joystickActive = false;
            }
        }
    }

    #endregion
}