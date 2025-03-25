using UnityEngine;

public class CharacterControllerDrag : MonoBehaviour
{
    public MobileInputManager inputManager; // Input Manager Referansı
    public float movementLimit = 2.5f; // X ekseninde max hareket sınırı
    public float dragSpeed = 0.01f; // Sürükleme hassasiyeti

    private Vector3 startPosition;
    private bool isDragging = false;

    private void Start()
    {
        if (inputManager != null)
        {
            inputManager.OnDragStart.AddListener(StartDrag);
            inputManager.OnDrag.AddListener(HandleDrag);
            inputManager.OnDragEnd.AddListener(EndDrag);
        }
        else
        {
            Debug.LogError("MobileInputManager atanmadı!");
        }
    }

    private void StartDrag(Vector2 touchPosition)
    {
        isDragging = true;
        startPosition = transform.position;
    }

    private void HandleDrag(Vector2 dragDelta)
    {
        if (!isDragging) return;

        // Drag yönünü al ve hassasiyet ile çarp
        float moveAmount = dragDelta.x * inputManager.globalSensitivity * dragSpeed;

        // Yeni pozisyonu hesapla
        float newX = Mathf.Clamp(transform.position.x + moveAmount, -movementLimit, movementLimit);

        // Transform'u güncelle
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void EndDrag(Vector2 touchPosition)
    {
        isDragging = false;
    }
}