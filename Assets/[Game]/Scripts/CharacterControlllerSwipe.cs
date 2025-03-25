using UnityEngine;

public class CharacterControllerSwipe : MonoBehaviour
{
    public MobileInputManager inputManager; // Input Manager Referansı
    public float moveSpeed = 5f; // Hareket Hızı
    public float movementLimit = 2.5f; // X ekseninde max hareket sınırı

    private void Start()
    {
        if (inputManager != null)
        {
            inputManager.OnSwipe.AddListener(HandleSwipe);
        }
        else
        {
            Debug.LogError("MobileInputManager atanmadı!");
        }
    }

    private void HandleSwipe(Vector2 swipeDirection)
    {
        // Swipe yönünü al ve global sensitivity ile çarp
        float moveAmount = swipeDirection.x * inputManager.globalSensitivity * moveSpeed * Time.deltaTime;

        // Yeni pozisyonu hesapla
        float newX = Mathf.Clamp(transform.position.x + moveAmount, -movementLimit, movementLimit);

        // Transform'u güncelle
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}